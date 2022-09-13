#!/bin/bash
set -e

# Set proxy environment variables
[[ -n $PROXY ]] && export \
  http_proxy=$PROXY \
  https_proxy=$PROXY \
  HTTP_PROXY=$PROXY \
  HTTPS_PROXY=$PROXY

# Set default values (if not specified in docker-compose)
export ILIVALIDATOR_MODEL_DIR="%ITF_DIR;$ILITOOLS_MODELS_DIR;${ILIVALIDATOR_MODEL_DIR:-http://models.interlis.ch/}"
export DELETE_TRANSFER_FILES=${DELETE_TRANSFER_FILES:-false}
export ENABLE_GPKG_VALIDATION=${ENABLE_GPKG_VALIDATION:-false}

# Download and configure ilivalidator and optional ili2pgkg
download_and_configure_ilitool () {
  ilitool=$1
  version=$2
  installDir=$3/$ilitool/$version
  echo -n "Download and configure $ilitool-$version ..."
  curl https://downloads.interlis.ch/$ilitool/$ilitool-$version.zip -LO --silent --show-error && \
    mkdir -p $installDir && unzip -o -q $ilitool-$version.zip -d $installDir && \
    rm $ilitool-$version.zip && \
    echo "done!" || exit 1
}

ILIVALIDATOR_LATEST_VERSION=$(curl https://www.interlis.ch/downloads/ilivalidator --silent | grep -Po '(?<=ilivalidator-)\d+.\d+.\d+' | head -n 1)
export ILIVALIDATOR_VERSION=${ILIVALIDATOR_VERSION:-$ILIVALIDATOR_LATEST_VERSION}
download_and_configure_ilitool ilivalidator $ILIVALIDATOR_VERSION $ILITOOLS_HOME_DIR

[[ $ENABLE_GPKG_VALIDATION = true ]] && \
  ILI2GPKG_LATEST_VERSION=$(curl https://www.interlis.ch/downloads/ili2db --silent | grep -Po '(?<=ili2gpkg-)\d+.\d+.\d+' | head -n 1) && \
  export ILI2GPKG_VERSION=${ILI2GPKG_VERSION:-$ILI2GPKG_LATEST_VERSION} && \
  download_and_configure_ilitool ili2gpkg $ILI2GPKG_VERSION $ILITOOLS_HOME_DIR

# Copy custom web assets to the web app public folder
[[ "$(ls -A $ILICHECK_WEB_ASSETS_DIR)" ]] && \
  echo -n "Copy custom web assets ..." && \
  cp -f $ILICHECK_WEB_ASSETS_DIR/* $ILICHECK_APP_HOME_DIR/ClientApp/build/ && \
  echo "done!"

# Use default user:group if no $PUID and/or $PGID is provided.
groupmod -o -g ${PUID:-941} abc && \
  usermod -o -u ${PGID:-941} abc &> /dev/null && \
  usermod -aG sudo abc && \
  echo "abc ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers

# Change owner for our folders
echo -n "Fix permissions for mounted volumes ..." && \
  chown -R abc:abc $ILICHECK_APP_HOME_DIR && \
  chown -R abc:abc $ILICHECK_APP_LOG_DIR && \
  chown -R abc:abc $ILICHECK_UPLOADS_DIR && \
  chown -R abc:abc $ILICHECK_WEB_ASSETS_DIR && \
  chown -R abc:abc $ILITOOLS_HOME_DIR && \
  chown -R abc:abc $ILITOOLS_CONFIG_DIR && \
  chown -R abc:abc $ILITOOLS_CATALOGS_DIR && \
  chown -R abc:abc $ILITOOLS_MODELS_DIR && \
  chown -R abc:abc $ILITOOLS_PLUGINS_DIR && \
  echo "done!"

# Export current environment for all users and cron jobs
echo -n "Configure environment ..." && \
  env | xargs -I {} echo 'export "{}"' > /etc/profile.d/env.sh && \
  env >> /etc/environment && echo "done!"

# Setup and run cron jobs
[[ -n $TRANSFER_AND_LOG_DATA_RETENTION ]] && \
  echo -n "Setup cron jobs ..." && \
  echo '* * * * * /usr/bin/find $ILICHECK_UPLOADS_DIR -mindepth 1 -maxdepth 1 -type d -not -newermt "$TRANSFER_AND_LOG_DATA_RETENTION ago" -exec rm -r "{}" \; > /proc/1/fd/1 2>/proc/1/fd/2' | crontab - && \
  cron && echo "done!"

echo "
--------------------------------------------------------------------------
ilicheck version:                 $ILICHECK_APP_VERSION
delete transfer files:            $([[ $DELETE_TRANSFER_FILES = true ]] && echo enabled || echo disabled)
transfer and log data retention:  $([[ -n $TRANSFER_AND_LOG_DATA_RETENTION ]] && echo $TRANSFER_AND_LOG_DATA_RETENTION || echo unset)
ilivalidator version:             $ILIVALIDATOR_VERSION `[[ $ILIVALIDATOR_VERSION != $ILIVALIDATOR_LATEST_VERSION ]] && echo "(new version $ILIVALIDATOR_LATEST_VERSION available!)"`
ili2gpkg version:                 $([[ $ENABLE_GPKG_VALIDATION = false ]] && echo "not configured" || echo $ILI2GPKG_VERSION `[[ $ILI2GPKG_VERSION != $ILI2GPKG_LATEST_VERSION ]] && echo "(new version $ILI2GPKG_LATEST_VERSION available!)"`)
ilivalidator config file name:    $([[ -n $ILIVALIDATOR_CONFIG_NAME ]] && echo $ILIVALIDATOR_CONFIG_NAME || echo disabled)
ilivalidator model repositories:  $ILIVALIDATOR_MODEL_DIR
ilivalidator trace messages:      $([[ $ILIVALIDATOR_ENABLE_TRACE = true ]] && echo enabled || echo disabled)
http proxy:                       ${PROXY:-no proxy set}
http proxy exceptions:            $([[ -n $NO_PROXY ]] && echo $NO_PROXY || echo undefined)
user uid:                         $(id -u abc)
user gid:                         $(id -g abc)
timezone:                         $TZ
--------------------------------------------------------------------------
"

echo -e "INTERLIS web check service app is up and running!\n" && \
  sudo -H --preserve-env --user abc dotnet ILICheck.Web.dll

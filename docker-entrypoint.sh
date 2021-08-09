#!/bin/bash
set -e

# Set proxy environment variables
[[ -n $PROXY ]] && export \
  http_proxy=$PROXY \
  https_proxy=$PROXY \
  HTTP_PROXY=$PROXY \
  HTTPS_PROXY=$PROXY

# Set ilivalidator default values (if not specified in docker-compose)
ILIVALIDATOR_LATEST_VERSION=$(curl https://www.interlis.ch/downloads/ilivalidator --silent | grep -Po '(?<=ilivalidator-)\d+.\d+.\d+' | head -n 1)
export ILIVALIDATOR_VERSION=${ILIVALIDATOR_VERSION:-$ILIVALIDATOR_LATEST_VERSION}
export ILIVALIDATOR_MODEL_DIR=${ILIVALIDATOR_MODEL_DIR:-http://models.interlis.ch/}

# Download ilivalidator
echo -n "Downloading and configure ilivalidator-$ILIVALIDATOR_VERSION ..."
curl https://downloads.interlis.ch/ilivalidator/ilivalidator-$ILIVALIDATOR_VERSION.zip -LO --silent --show-error && \
  unzip -o -q ilivalidator-$ILIVALIDATOR_VERSION.zip -d $ILIVALIDATOR_HOME_DIR/$ILIVALIDATOR_VERSION && \
  rm ilivalidator-$ILIVALIDATOR_VERSION.zip && \
  echo "done!" || exit 1

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
  chown -R abc:abc $ILIVALIDATOR_HOME_DIR && \
  chown -R abc:abc $ILIVALIDATOR_CONFIG_DIR && \
  echo "done!"

# Export current environment for all users
echo -n "Configure environment ..." && \
  env | xargs -L 1 -I {} echo 'export "{}"' > /etc/profile.d/env.sh && \
  echo "done!"

echo "
--------------------------------------------------------------------------
ilivalidator version:             $ILIVALIDATOR_VERSION `[[ $ILIVALIDATOR_VERSION != $ILIVALIDATOR_LATEST_VERSION ]] && echo "(new version $ILIVALIDATOR_LATEST_VERSION available!)"`
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

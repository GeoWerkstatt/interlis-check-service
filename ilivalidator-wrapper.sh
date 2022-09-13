#!/bin/bash
set -e

[[ $# -ne 0 ]] && \
  transfer_file_name=${@: -1} && \
  options=${@%"$transfer_file_name"} && \
  is_gpkg=$([[ $transfer_file_name == *.gpkg ]] && echo true || echo false )

# Include optional suitables xml catalogue files for xtf transfer files
catalogue_files=$([[ $transfer_file_name == *.xtf ]] && \
  (find `dirname $transfer_file_name` -maxdepth 1 -type f -iname "*.xml" | grep . || \
  find $ILITOOLS_CATALOGS_DIR -maxdepth 1 -type f -iname "*.xml") || true)

proxy_port=$(echo $PROXY | grep -Eo '[0-9]+' | tail -1)
proxy_host=${PROXY%":$(echo ${PROXY##*:})"} # remove port
proxy_host=${proxy_host#*://} # remove protocol

[[ -n $ILIVALIDATOR_CONFIG_NAME ]] && [[ $is_gpkg = false ]] && options+=" --config $ILITOOLS_CONFIG_DIR/$ILIVALIDATOR_CONFIG_NAME"
[[ -n $ILIVALIDATOR_MODEL_DIR ]] && options+=" --modeldir $ILIVALIDATOR_MODEL_DIR"
[[ -n $proxy_host ]] && options+=" --proxy $proxy_host"
[[ -n $proxy_port ]] && options+=" --proxyPort $proxy_port"
[[ $ILIVALIDATOR_ENABLE_TRACE = true ]] && options+=" --trace"
[[ $(find $ILITOOLS_PLUGINS_DIR -maxdepth 1 -type f -iname "*.jar" | grep .) ]] && [[ $is_gpkg = false ]] && options+=" --plugins $ILITOOLS_PLUGINS_DIR"

# Print executed commands to the Docker container log output
exec {BASH_XTRACEFD}> >(sudo tee /proc/1/fd/2)

# Execute ilivalidator/ili2gpkg with the given options
if [[ $ENABLE_GPKG_VALIDATION = true && $is_gpkg = true ]]
then
  set -x #echo on
  java -jar $ILITOOLS_HOME_DIR/ili2gpkg/$ILI2GPKG_VERSION/ili2gpkg-$ILI2GPKG_VERSION.jar --validate $options --dbfile $transfer_file_name
else
  set -x #echo on
  java -jar $ILITOOLS_HOME_DIR/ilivalidator/$ILIVALIDATOR_VERSION/ilivalidator-$ILIVALIDATOR_VERSION.jar --allObjectsAccessible $options $transfer_file_name $catalogue_files
fi

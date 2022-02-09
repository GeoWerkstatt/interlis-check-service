#!/bin/bash
set -e

[[ $# -ne 0 ]] && \
  transfer_file_name=${@: -1} && \
  options=${@%"$transfer_file_name"}

proxy_port=$(echo $PROXY | grep -Eo '[0-9]+' | tail -1)
proxy_host=${PROXY%":$(echo ${PROXY##*:})"} # remove port
proxy_host=${proxy_host#*://} # remove protocol


[[ -n $ILIVALIDATOR_CONFIG_NAME ]] && options+=" --config $ILIVALIDATOR_CONFIG_DIR/$ILIVALIDATOR_CONFIG_NAME"
[[ -n $ILIVALIDATOR_MODEL_DIR ]] && options+=" --modeldir $ILIVALIDATOR_MODEL_DIR"
[[ -n $proxy_host ]] && options+=" --proxy $proxy_host"
[[ -n $proxy_port ]] && options+=" --proxyPort $proxy_port"
[[ $ILIVALIDATOR_ENABLE_TRACE = true ]] && options+=" --trace"


# Print executed commands to the Docker container log output
exec {BASH_XTRACEFD}> >(sudo tee /proc/1/fd/2)
set -x #echo on

# Execute ilivalidator with the given options
java -jar $ILIVALIDATOR_HOME_DIR/$ILIVALIDATOR_VERSION/ilivalidator-$ILIVALIDATOR_VERSION.jar $options "$transfer_file_name"

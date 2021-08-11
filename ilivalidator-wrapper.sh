#!/bin/bash
set -e

TRANSFER_FILE_NAME=${@: -1}
OPTIONS=${@%"$TRANSFER_FILE_NAME"}
PROXY_PORT=$(echo $PROXY | grep -Eo '[0-9]+' | tail -1)
PROXY_HOST=${PROXY%":$(echo ${PROXY##*:})"} # remove port
PROXY_HOST=${PROXY_HOST#*://} # remove protocol


[[ -n $ILIVALIDATOR_CONFIG_NAME ]] && OPTIONS+=" --config $ILIVALIDATOR_CONFIG_DIR/$ILIVALIDATOR_CONFIG_NAME"
[[ -n $ILIVALIDATOR_MODEL_DIR ]] && OPTIONS+=" --modeldir $ILIVALIDATOR_MODEL_DIR"
[[ -n $PROXY_HOST ]] && OPTIONS+=" --proxy $PROXY_HOST"
[[ -n $PROXY_PORT ]] && OPTIONS+=" --proxyPort $PROXY_PORT"
[[ $ILIVALIDATOR_ENABLE_TRACE = true ]] && OPTIONS+=" --trace"


# Print executed commands to the Docker container log output
exec {BASH_XTRACEFD}> >(sudo tee /proc/1/fd/2)
set -x #echo on

# Execute ilivalidator with the given options
java -jar $ILIVALIDATOR_HOME_DIR/$ILIVALIDATOR_VERSION/ilivalidator-$ILIVALIDATOR_VERSION.jar $OPTIONS $TRANSFER_FILE_NAME

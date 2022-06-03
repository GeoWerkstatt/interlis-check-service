#!/bin/bash
# Basic integration tests to verify upload API with several client-side frameworks.
# These tests are intended to be ran and debugged in development environment only.

set -e

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]:-$0}"; )" &> /dev/null && pwd 2> /dev/null; )

export UPLOAD_URL=http://host.docker.internal:3080/api/v1/upload
export UPLOAD_FILE_NAME=/testdata/example.xtf

docker run -it --rm \
  -v $SCRIPT_DIR/scripts:/scripts \
  -v $SCRIPT_DIR/testdata:/testdata:ro \
  -v /scripts/node_modules \
  -e UPLOAD_URL \
  -e UPLOAD_FILE_NAME \
  nikolaik/python-nodejs \
  /bin/bash -c "cd /scripts/ &&
      ./upload-transfer-file.sh &&
      python3 -m pip install requests && python upload-transfer-file.py &&
      npm install && node upload-transfer-file.js
  "

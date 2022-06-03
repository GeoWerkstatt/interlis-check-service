#!/bin/bash
set -e

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]:-$0}"; )" &> /dev/null && pwd 2> /dev/null; )

docker run -it --rm \
  -v $SCRIPT_DIR/scripts:/scripts \
  -v $SCRIPT_DIR/testdata:/testdata:ro \
  -v /scripts/node_modules \
  -e UPLOAD_URL=http://host.docker.internal:3080/api/v1/upload \
  -e UPLOAD_FILE_NAME=/testdata/example.xtf \
  nikolaik/python-nodejs \
  /bin/bash -c "cd /scripts/ &&
      ./upload-transfer-file.sh &&
      python3 -m pip install requests && python upload-transfer-file.py &&
      npm install && node upload-transfer-file.js
  "

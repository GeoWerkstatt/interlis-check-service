#!/bin/sh
set -e

curl -i -X POST -H "Content-Type: multipart/form-data" -F "file=@${UPLOAD_FILE_NAME}" $UPLOAD_URL

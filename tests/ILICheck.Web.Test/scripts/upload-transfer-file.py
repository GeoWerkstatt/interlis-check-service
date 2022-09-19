import os
import requests

response = requests.post(os.environ.get('UPLOAD_URL'), files={'file':open(os.environ.get('UPLOAD_FILE_NAME'))}).json()

print(response)

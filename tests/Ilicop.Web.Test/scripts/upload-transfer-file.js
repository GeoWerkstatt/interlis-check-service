import { createReadStream } from 'fs';
import FormData from 'form-data';
import fetch from 'node-fetch';

var form = new FormData();
form.append('file', createReadStream(process.env.UPLOAD_FILE_NAME));
const response = await fetch(process.env.UPLOAD_URL, {
  method: 'POST',
  body: form,
});

console.log(await response.json());

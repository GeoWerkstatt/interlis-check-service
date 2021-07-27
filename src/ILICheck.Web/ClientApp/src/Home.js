import './App.css';
import React, { useState, useEffect } from 'react';
import { usePDF } from '@react-pdf/renderer';
import { Button, Container } from 'react-bootstrap';
import { AiOutlinePlayCircle } from 'react-icons/ai';
import { FileDropzone } from './Dropzone';
import { ProtokollPdf } from './ProtokollPdf';
import Protokoll from './Protokoll';

export const Home = props => {
  const { connection, log, setLog } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [testRunTime, setTestRunTime] = useState(null);
  const [pdf, updatePdf] = usePDF({ document: ProtokollPdf });
  const [protokollName, setProtokollName] = useState("");
  const [fileCheckStatusClass, setFileCheckStatusClass] = useState("");
  const [fileCheckStatus, setFileCheckStatus] = useState("");

  // Reset log on file change
  useEffect(() => {
    setLog([]);
  }, [fileToCheck, setLog])


  const checkFile = () => {
    setTestRunning(true);
    setFileCheckStatus("")
    setFileCheckStatusClass("")
    connection.invoke("StartUpload", connection.connectionId, fileToCheck.name);
    uploadFile(fileToCheck);

    setTestRunTime(new Date().toLocaleString());
    setProtokollName("Check_result_" + fileToCheck.name + "-" + testRunTime);
    setTestRunning(false);
  }

  const uploadFile = (file) => {
    const formData = new FormData();
    formData.append(file.name, file);

    fetch(`api/upload?connectionId=${connection.connectionId}`, {
      method: 'POST',
      body: formData,
    })
      .then(res => {
        if (res.status === 200) {
          setLog(log => [...log, `${file.name} successfully uploaded!`])
          setFileCheckStatusClass("valid")
          setFileCheckStatus("Datei enthält keine Fehler!")
        }
        else {
          setFileCheckStatusClass("errors")
          setFileCheckStatus("Datei enthält Fehler!")
          res.text().then(text => {
            setLog(log => [...log, text])
          });
        }
      })
      .catch(err => console.error(err));
  }

  return (
    <div className="app">
      <header className="app-header">
        <p>
          INTERLIS Web-Check-Service
          </p>
      </header>
      <Container>
        <FileDropzone setFileToCheck={setFileToCheck} />
        <Button variant="success" className={fileToCheck ? "" : "invisible-check-button"} onClick={checkFile}>Check
            <span className="run-icon">
            {testRunning ? (<div className="spinner-border spinner-border-sm text-light"></div>) : (<AiOutlinePlayCircle />)}
          </span>
        </Button>
      </Container>
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} fileCheckStatusClass={fileCheckStatusClass} testRunTime={testRunTime} protokollName={protokollName} pdf={pdf} />
    </div>
  );
}

export default Home;

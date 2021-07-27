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
  const [pdf, updatePdf] = usePDF({ document: ProtokollPdf });
  const [fileCheckStatus, setFileCheckStatus] = useState({ text: "", class: "", testRunTime: null, protokollName: "" });

  // Reset log on file change
  useEffect(() => {
    setLog([]);
  }, [fileToCheck, setLog])


  const checkFile = () => {
    setTestRunning(true);
    setFileCheckStatus({ text: "", class: "", testRunTime: null, fileName: "" })
    uploadFile(fileToCheck);
  }

  const uploadFile = (file) => {
    const formData = new FormData();
    formData.append(file.name, file);

    fetch(`api/upload?connectionId=${connection.connectionId}&fileName=${file.name}`, {
      method: 'POST',
      body: formData,
    })
      .then(res => {
        setTestRunning(false);
        if (res.status === 200) {
          setLog(log => [...log, `${file.name} successfully uploaded!`])
          setFileCheckStatus({
            text: "Datei enthält keine Fehler!",
            class: "valid",
            testRunTime: new Date().toLocaleString(),
            fileName: fileToCheck.name,
          })
        }
        else {
          setFileCheckStatus({
            text: "Datei enthält Fehler!",
            class: "errors",
            testRunTime: new Date().toLocaleString(),
            fileName: fileToCheck.name,
          })
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
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} pdf={pdf} />
    </div>
  );
}

export default Home;

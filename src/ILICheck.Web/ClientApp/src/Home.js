import './App.css';
import React, { useState, useEffect } from 'react';
import { Button, Container } from 'react-bootstrap';
import { AiOutlinePlayCircle } from 'react-icons/ai';
import { FileDropzone } from './Dropzone';
import Protokoll from './Protokoll';

export const Home = props => {
  const { connection, closedConnectionId, log, setLog } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [fileCheckStatus, setFileCheckStatus] = useState({ text: "", class: "", testRunTime: null, protokollName: "" });
  const [abortController, setAbortController] = useState(null)

  // Reset log and abort upload on file change
  useEffect(() => {
    setLog([]);
    setTestRunning(false);
    abortController && abortController.abort();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fileToCheck, setLog])


  const checkFile = () => {
    setLog([]);
    setTestRunning(true);
    setFileCheckStatus({ text: "", class: "", testRunTime: null, fileName: "" })
    uploadFile(fileToCheck);
  }

  const uploadFile = (file) => {
    const formData = new FormData();
    formData.append(file.name, file);

    const controller = new AbortController()
    const signal = controller.signal
    setAbortController(controller);
    fetch(`api/upload?connectionId=${connection.connectionId}&fileName=${file.name}`, {
      method: 'POST',
      signal: signal,
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
        <FileDropzone setFileToCheck={setFileToCheck} abortController={abortController}/>
        <Button variant="success" className={fileToCheck ? "" : "invisible-check-button"} onClick={checkFile}>Check
          <span className="run-icon">
            {testRunning ? (<div className="spinner-border spinner-border-sm text-light"></div>) : (<AiOutlinePlayCircle />)}
          </span>
        </Button>
      </Container>
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} closedConnectionId={closedConnectionId} connection={connection} />
    </div>
  );
}

export default Home;

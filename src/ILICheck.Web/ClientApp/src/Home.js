import './App.css';
import React, { useState, useEffect } from 'react';
import { Button, Container } from 'react-bootstrap';
import { AiOutlinePlayCircle } from 'react-icons/ai';
import { FileDropzone } from './Dropzone';
import Protokoll from './Protokoll';

export const Home = props => {
  const { connection, closedConnectionId, log, updateLog, resetLog } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [fileCheckStatus, setFileCheckStatus] = useState({ text: "", class: "", testRunTime: null, fileName: "", fileDownloadAvailable: false });
  const [abortController, setAbortController] = useState(null)

  // Reset log and abort upload on file change
  useEffect(() => {
    resetLog();
    setTestRunning(false);
    abortController && abortController.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fileToCheck, resetLog])


  const checkFile = () => {
    resetLog();
    setTestRunning(true);
    setFileCheckStatus({ text: "", class: "", testRunTime: null, fileName: "", fileDownloadAvailable: false })
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
        res.text().then(content => {
          let className;
          let text;
          let downloadAvailable = false;
          if (content) {
            className = "errors"
            text = "Fehler!"
            updateLog(content);
          }
          else {
            className = "valid"
            text = "Keine Fehler!"
            updateLog(`${file.name} validiert!`);
          }
          if (res.status === 200) {
            downloadAvailable = true;
          }
          setFileCheckStatus({
            text: text,
            class: className,
            testRunTime: new Date(),
            fileName: fileToCheck.name,
            fileDownloadAvailable: downloadAvailable
          })
        });
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
        <FileDropzone setFileToCheck={setFileToCheck} abortController={abortController} />
        <Button variant="success" className={fileToCheck ? "" : "invisible-check-button"} onClick={checkFile}>Check
          <span className="run-icon">
            {testRunning ? (<div className="spinner-border spinner-border-sm text-light"></div>) : (<AiOutlinePlayCircle />)}
          </span>
        </Button>
      </Container>
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} closedConnectionId={closedConnectionId} connection={connection} />
      <footer className="app-footer">
        <div>{process.env.REACT_APP_VERSION ? 'v' + process.env.REACT_APP_VERSION + '+' : ''}{process.env.REACT_APP_REVISION ?? process.env.NODE_ENV}</div>
      </footer>
    </div>
  );
}

export default Home;

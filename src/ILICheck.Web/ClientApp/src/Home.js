import './App.css';
import './CI.css';
import React, { useState, useEffect } from 'react';
import { Button, Container } from 'react-bootstrap';
import { AiOutlinePlayCircle } from 'react-icons/ai';
import { FileDropzone } from './Dropzone';
import Protokoll from './Protokoll';

export const Home = props => {
  const { connection, closedConnectionId, log, updateLog, resetLog, setUploadLogsInterval, setUploadLogsEnabled, validationResult, setValidationResult } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [fileCheckStatus, setFileCheckStatus] = useState({ text: "", class: "", testRunTime: null, fileName: "", fileDownloadAvailable: false });

  const logUploadLogMessages = () => updateLog(`${fileToCheck.name} wird hochgeladen...`, { disableUploadLogs: false });
  const setIntervalImmediately = (func, interval) => { func(); return setInterval(func, interval); }

  // Reset log and abort upload on file change
  useEffect(() => {
    resetLog();
    setTestRunning(false);
    if (connection && connection.connectionId) {
      connection.stop();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fileToCheck, resetLog])


    useEffect(() => {
      if(validationResult !== "none")
      {
      let className;
      let text;
      let downloadAvailable = false;
      setTestRunning(false);
        if(validationResult === "ok"){
          downloadAvailable =true;
          className = "valid"
          text = "Keine Fehler!"
        }
        if(validationResult === "error"|| validationResult === "aborted"){
          className = "errors"
          text = "Fehler!"
          if(validationResult === "error"){
            downloadAvailable =true;
          }
        }

        setFileCheckStatus({
          text: text,
          class: className,
          testRunTime: new Date(),
          fileName: fileToCheck? fileToCheck.name : "",
          fileDownloadAvailable: downloadAvailable
        })
        setValidationResult("none")
        }
    }, [validationResult, fileToCheck, setValidationResult,updateLog])

  const checkFile = () => {
    resetLog();
    setTestRunning(true);
    setFileCheckStatus({ text: "", class: "", testRunTime: null, fileName: "", fileDownloadAvailable: false })
    setUploadLogsInterval(setIntervalImmediately(logUploadLogMessages, 2000));
    uploadFile(fileToCheck);
  }

  const uploadFile = (file) => {
    const formData = new FormData();
    formData.append(file.name, file);

    const controller = new AbortController()
    const signal = controller.signal
    fetch(`api/upload?connectionId=${connection.connectionId}&fileName=${file.name}`, {
      method: 'POST',
      signal: signal,
      body: formData,
    })
      .then(res => {
        if(res.status === 200)
        {
          console.log("Datei erfolgreich hochgeladen.")
        }
        else 
        {
          updateLog("Fehler beim Hochladen der Datei.")
          console.log("Fehler beim Hochladen der Datei.")
        }
      })
      .catch(err => console.error(err));
  }

  return (
    <div>
      <Container>
        <div className="title">
        INTERLIS Web-Check-Service
        </div>
        <div className="dropzone-wrapper">
          <FileDropzone setUploadLogsEnabled= {setUploadLogsEnabled} setFileToCheck={setFileToCheck} connection={connection} />
          <Button variant="success" className={fileToCheck ? "check-button" : "invisible-check-button"} onClick={checkFile}>Check
            <span className="run-icon">
              {testRunning ? (<div className="spinner-border spinner-border-sm text-light"></div>) : (<AiOutlinePlayCircle />)}
            </span>
        </Button>
        </div>
      </Container>
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} closedConnectionId={closedConnectionId} connection={connection} />
    </div>
  );
}

export default Home;

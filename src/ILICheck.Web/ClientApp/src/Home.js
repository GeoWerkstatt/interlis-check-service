import './App.css';
import './CI_geow.css';
import React, { useState, useEffect } from 'react';
import { Button, Container } from 'react-bootstrap';
import { FileDropzone } from './Dropzone';
import Protokoll from './Protokoll';
import InfoCarousel from './InfoCarousel';
import ili_cop_logo from './img/ILI_cop.png'

export const Home = props => {
  const { settings, connection, closedConnectionId, log, updateLog, resetLog, setUploadLogsInterval } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [fileCheckStatus, setFileCheckStatus] = useState({ text: "", class: "", testRunTime: null, fileName: "", fileDownloadAvailable: false });
  const [abortController, setAbortController] = useState(null)

  const logUploadLogMessages = () => updateLog(`${fileToCheck.name} wird hochgeladen...`, { disableUploadLogs: false });
  const setIntervalImmediately = (func, interval) => { func(); return setInterval(func, interval); }

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
    setUploadLogsInterval(setIntervalImmediately(logUploadLogMessages, 2000));
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
            updateLog("Alles nach Vorschrift, der ILICOP hat nichts zu beanstanden!");
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
    <div>
      <Container>
        <img src={ili_cop_logo} width="200" alt="ILICop_Logo" />
        <div className="title">
          {settings? settings.title : "INTERLIS WEB CHECK SERVICE"}
        </div>
        <InfoCarousel />
        <div className="dropzone-wrapper">
          <FileDropzone setFileToCheck={setFileToCheck} abortController={abortController} />
          <Button className={fileToCheck ? "check-button btn-color" : "invisible-check-button"} onClick={checkFile}>
            <span className="run-icon">
              {testRunning ? (<span className="spinner-border spinner-border-sm text-light"></span>) : ("Los!")}
            </span>
          </Button>
        </div>
      </Container>
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} closedConnectionId={closedConnectionId} connection={connection} />
    </div>
  );
}

export default Home;

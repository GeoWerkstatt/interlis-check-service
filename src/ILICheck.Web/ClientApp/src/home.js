import './app.css';
import './custom.css';
import React, { useState, useEffect } from 'react';
import { Button, Container } from 'react-bootstrap';
import { FileDropzone } from './dropzone';
import Protokoll from './protokoll';
import InfoCarousel from './infoCarousel';

export const Home = props => {
  const { connection, closedConnectionId, clientSettings, nutzungsbestimmungenAvailable, showNutzungsbestimmungen, quickStartContent, log, updateLog, resetLog, setUploadLogsInterval, setUploadLogsEnabled, validationResult, setValidationResult } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [fileCheckStatus, setFileCheckStatus] = useState({ text: "", class: "", testRunTime: null, fileName: "", fileDownloadAvailable: false });
  const [customAppLogoPresent, setCustomAppLogoPresent] = useState(true);
  const [checkedNutzungsbestimmungen, setCheckedNutzungsbestimmungen] = useState(false);

  const logUploadLogMessages = () => updateLog(`${fileToCheck.name} wird hochgeladen...`, { disableUploadLogs: false });
  const setIntervalImmediately = (func, interval) => { func(); return setInterval(func, interval); }

  // Reset log and abort upload on file change
  useEffect(() => {
    resetLog();
    setTestRunning(false);
    if (connection?.connectionId) {
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
          updateLog(`Alles nach Vorschrift, der ${clientSettings?.applicationName} hat nichts zu beanstanden!`);
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
    }, [validationResult, fileToCheck, setValidationResult, updateLog, clientSettings])

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
        <img className="app-logo" src="/app.png" alt="App Logo" onError={e => {setCustomAppLogoPresent(false); e.target.style.display='none'}} />
        {!customAppLogoPresent && <div className="app-title">{clientSettings?.applicationName}</div>}
        {quickStartContent && <InfoCarousel content={quickStartContent} />}
        <div className="dropzone-wrapper">
          <FileDropzone setUploadLogsEnabled= {setUploadLogsEnabled} setFileToCheck={setFileToCheck} connection={connection} />
          <Button className={fileToCheck ? "check-button btn-color" : "invisible-check-button"} onClick={checkFile}
            disabled={(nutzungsbestimmungenAvailable && !checkedNutzungsbestimmungen) || testRunning}>
            <span className="run-icon">
              {testRunning ? (<span className="spinner-border spinner-border-sm text-light"></span>) : ("Los!")}
            </span>
          </Button>
        </div>
        {fileToCheck && nutzungsbestimmungenAvailable &&
          <div className="terms-of-use">
            <label>
              <input type="checkbox"
                defaultChecked={checkedNutzungsbestimmungen}
                onChange={() => setCheckedNutzungsbestimmungen(!checkedNutzungsbestimmungen)}
              />
            Ich akzeptiere die <Button variant="link" cssClass="terms-of-use link" onClick={() => showNutzungsbestimmungen()}>Nutzungsbestimmungen</Button>.
          </label>
        </div>}
      </Container>
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} closedConnectionId={closedConnectionId} connection={connection} />
    </div>
  );
}

export default Home;

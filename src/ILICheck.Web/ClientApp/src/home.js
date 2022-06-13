import "./app.css";
import React, { useState, useEffect } from "react";
import { Container } from "react-bootstrap";
import { FileDropzone } from "./dropzone";
import Protokoll from "./protokoll";
import InfoCarousel from "./infoCarousel";

export const Home = (props) => {
  const {
    clientSettings,
    nutzungsbestimmungenAvailable,
    showNutzungsbestimmungen,
    quickStartContent,
    log,
    updateLog,
    resetLog,
    setUploadLogsInterval,
    setUploadLogsEnabled,
    validationResult,
    setValidationResult,
    setShowBannerContent,
  } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [fileCheckStatus, setFileCheckStatus] = useState({
    text: "",
    class: "",
    testRunTime: null,
    fileName: "",
    fileDownloadAvailable: false,
  });
  const [customAppLogoPresent, setCustomAppLogoPresent] = useState(false);
  const [checkedNutzungsbestimmungen, setCheckedNutzungsbestimmungen] = useState(false);
  const [isFirstValidation, setIsFirstValidation] = useState(true);

  const logUploadLogMessages = () => updateLog(`${fileToCheck.name} hochladen...`, { disableUploadLogs: false });
  const setIntervalImmediately = (func, interval) => {
    func();
    return setInterval(func, interval);
  };

  // Reset log and abort upload on file change
  useEffect(() => {
    resetLog();
    setTestRunning(false);
    setUploadLogsEnabled(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fileToCheck, resetLog]);

  useEffect(() => {
    if (testRunning && isFirstValidation) {
      setTimeout(() => {
        setShowBannerContent(true);
        setIsFirstValidation(false);
      }, 2000);
    }
  }, [testRunning, isFirstValidation, setShowBannerContent, setIsFirstValidation]);

  useEffect(() => {
    if (validationResult !== "none") {
      let className;
      let text;
      let downloadAvailable = false;
      setTestRunning(false);

      if (validationResult === "ok") {
        downloadAvailable = true;
        className = "valid";
        text = "Keine Fehler!";
        updateLog("Die Daten sind modellkonform!");
      }

      if (validationResult === "error") {
        downloadAvailable = true;
        className = "errors";
        text = "Fehler!";
        updateLog("Die Daten sind nicht modellkonform! Für Fehlermeldungen siehe XTF-Log-Datei.");
      }

      if (validationResult === "aborted") {
        className = "errors";
        text = "Fehler!";
        updateLog("Die Validierung wurde abgebrochen.");
      }

      setFileCheckStatus({
        text: text,
        class: className,
        testRunTime: new Date(),
        fileName: fileToCheck ? fileToCheck.name : "",
        fileDownloadAvailable: downloadAvailable,
      });

      setValidationResult("none");
    }
  }, [validationResult, fileToCheck, setValidationResult, updateLog, clientSettings]);

  const checkFile = (e) => {
    e.stopPropagation();
    resetLog();
    setTestRunning(true);
    setFileCheckStatus({ text: "", class: "", testRunTime: null, fileName: "", fileDownloadAvailable: false });
    setUploadLogsInterval(setIntervalImmediately(logUploadLogMessages, 2000));
    uploadFile(fileToCheck);
  };

  async function uploadFile(file) {
    const formData = new FormData();
    formData.append("file", file, file.name);
    const response = await fetch(`api/v1/upload`, {
      method: "POST",
      body: formData,
    });
    const data = await response.json();
    var interval = setInterval(async () => {
      const status = await fetch(data.statusUrl, {
        method: "GET",
      });
      const statusData = await status.json();
      console.log(statusData);
      updateLog(statusData.statusMessage);
      if (statusData.status === "completed" || statusData.status === "completedWithErrors") {
        clearInterval(interval);
        setTestRunning(false);
        console.log("clear");
      }
      console.log(statusData);
    }, 1000);
    if (response.ok) {
      console.log("Datei erfolgreich hochgeladen.");
    } else {
      updateLog("Fehler beim Hochladen der Datei.");
      console.log("Fehler beim Hochladen der Datei.");
    }
  }

  return (
    <div>
      <Container className="main-container">
        <div className="title-wrapper">
          <div className="app-subtitle">Online Validierung von INTERLIS Daten</div>
          <div>
            <img
              className="app-logo"
              src="/app.png"
              alt="App Logo"
              onLoad={() => setCustomAppLogoPresent(true)}
              onError={(e) => (e.target.style.display = "none")}
            />
          </div>
          {!customAppLogoPresent && <div className="app-title">{clientSettings?.applicationName}</div>}
          {quickStartContent && <InfoCarousel content={quickStartContent} />}
        </div>
        <div className="dropzone-wrapper">
          <FileDropzone
            setUploadLogsEnabled={setUploadLogsEnabled}
            setFileToCheck={setFileToCheck}
            fileToCheck={fileToCheck}
            nutzungsbestimmungenAvailable={nutzungsbestimmungenAvailable}
            checkedNutzungsbestimmungen={checkedNutzungsbestimmungen}
            checkFile={checkFile}
            testRunning={testRunning}
            setCheckedNutzungsbestimmungen={setCheckedNutzungsbestimmungen}
            showNutzungsbestimmungen={showNutzungsbestimmungen}
            acceptedFileTypes={clientSettings?.acceptedFileTypes}
          />
        </div>
      </Container>
      <Protokoll log={log} fileCheckStatus={fileCheckStatus} testRunning={testRunning} />
    </div>
  );
};

export default Home;

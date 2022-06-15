import React, { useState, useEffect } from "react";
import { Container } from "react-bootstrap";
import { FileDropzone } from "./dropzone";
import Protokoll from "./protokoll";
import InfoCarousel from "./infoCarousel";
import "./app.css";

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
    setShowBannerContent,
  } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [validationRunning, setValidationRunning] = useState(false);
  const [statusInterval, setStatusInterval] = useState(null);
  const [statusData, setStatusData] = useState(null);
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
    setStatusData(null);
    setValidationRunning(false);
    setUploadLogsEnabled(false);
    if (statusInterval) clearInterval(statusInterval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fileToCheck, resetLog]);

  useEffect(() => {
    if (validationRunning && isFirstValidation) {
      setTimeout(() => {
        setShowBannerContent(true);
        setIsFirstValidation(false);
      }, 2000);
    }
  }, [validationRunning, isFirstValidation, setShowBannerContent, setIsFirstValidation]);

  const checkFile = (e) => {
    e.stopPropagation();
    resetLog();
    setStatusData(null);
    setValidationRunning(true);
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
    if (response.ok) {
      const data = await response.json();
      const getStatusData = async (data) => {
        const status = await fetch(data.statusUrl, {
          method: "GET",
        });
        const statusData = await status.json();
        return statusData;
      };

      const interval = setIntervalImmediately(async () => {
        const statusData = await getStatusData(data);
        updateLog(statusData.statusMessage);
        if (
          statusData.status === "completed" ||
          statusData.status === "completedWithErrors" ||
          statusData.status === "failed"
        ) {
          clearInterval(interval);
          setValidationRunning(false);
          setStatusData(statusData);
        }
      }, 2000);
      setStatusInterval(interval);
    } else {
      console.log("Error while uploading file: " + response.json());
      updateLog("Der Upload war nicht erfolgreich. Die Validierung wurde abgebrochen.");
      setValidationRunning(false);
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
            validationRunning={validationRunning}
            setCheckedNutzungsbestimmungen={setCheckedNutzungsbestimmungen}
            showNutzungsbestimmungen={showNutzungsbestimmungen}
            acceptedFileTypes={clientSettings?.acceptedFileTypes}
          />
        </div>
      </Container>
      <Protokoll
        log={log}
        statusData={statusData}
        fileName={fileToCheck ? fileToCheck.name : ""}
        validationRunning={validationRunning}
      />
    </div>
  );
};

export default Home;

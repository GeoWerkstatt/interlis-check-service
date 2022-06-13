/* eslint-disable react-hooks/exhaustive-deps */
import React, { useCallback, useState, useEffect } from "react";
import { useDropzone } from "react-dropzone";
import { MdCancel, MdFileUpload } from "react-icons/md";
import { Button, Spinner } from "react-bootstrap";
import styled from "styled-components";

const getColor = (props) => {
  if (props.isDragActive) {
    return "#2196f3";
  } else {
    return "#d1d6d991";
  }
};

const Container = styled.div`
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  min-height: 15vh;
  max-width: 95vw;
  font-size: 20px;
  border-width: 2px;
  border-radius: 5px;
  border-color: ${(props) => getColor(props)};
  border-style: dashed;
  background-color: #d1d6d991;
  color: #9f9f9f;
  outline: none;
  transition: border 0.24s ease-in-out;
`;

export const FileDropzone = ({
  setFileToCheck,
  connection,
  setUploadLogsEnabled,
  fileToCheck,
  nutzungsbestimmungenAvailable,
  checkedNutzungsbestimmungen,
  checkFile,
  testRunning,
  setCheckedNutzungsbestimmungen,
  showNutzungsbestimmungen,
  acceptedFileTypes,
}) => {
  const [fileAvailable, setFileAvailable] = useState(false);
  const [dropZoneDefaultText, setDropZoneDefaultText] = useState();
  const [dropZoneText, setDropZoneText] = useState(dropZoneDefaultText);
  const [dropZoneTextClass, setDropZoneTextClass] = useState("dropzone dropzone-text-disabled");

  useEffect(
    () =>
      setDropZoneDefaultText(
        `Datei (${acceptedFileTypes}) hier ablegen oder klicken um vom lokalen Dateisystem auszuwählen.`
      ),
    [acceptedFileTypes]
  );
  useEffect(() => setDropZoneText(dropZoneDefaultText), [dropZoneDefaultText]);

  const updateDropZoneClass = () => {
    if (!checkFile || (nutzungsbestimmungenAvailable && !checkedNutzungsbestimmungen)) {
      setDropZoneTextClass("dropzone dropzone-text-disabled");
    } else {
      setDropZoneTextClass("dropzone dropzone-text-file");
    }
  };

  const onDropAccepted = useCallback(
    (acceptedFiles) => {
      updateDropZoneClass();
      if (acceptedFiles.length === 1) {
        setDropZoneText(acceptedFiles[0].name);
        updateDropZoneClass();
        setFileToCheck(acceptedFiles[0]);
        setFileAvailable(true);
      }
    },
    [setFileToCheck]
  );

  const onDropRejected = useCallback(
    (fileRejections) => {
      setDropZoneTextClass("dropzone dropzone-text-error");
      const errorCode = fileRejections[0].errors[0].code;

      switch (errorCode) {
        case "file-invalid-type":
          setDropZoneText(`Bitte wähle eine Datei (max. 200MB) mit folgender Dateiendung: ${acceptedFileTypes}`);
          break;
        case "too-many-files":
          setDropZoneText("Es kann nur eine Datei aufs Mal geprüft werden.");
          break;
        case "file-too-large":
          setDropZoneText(
            "Die ausgewählte Datei ist über 200MB gross. Bitte wähle eine kleinere Datei oder erstelle eine ZIP-Datei."
          );
          break;
        default:
          setDropZoneText(`Bitte wähle eine Datei (max. 200MB) mit folgender Dateiendung: ${acceptedFileTypes}`);
      }
      setFileToCheck(null);
      setFileAvailable(false);
    },
    [setFileToCheck]
  );

  const removeFile = (e) => {
    e.stopPropagation();
    connection.stop();
    setUploadLogsEnabled(false);
    setFileToCheck(null);
    setFileAvailable(false);
    setDropZoneText(dropZoneDefaultText);
    setDropZoneTextClass("dropzone dropzone-text-disabled");
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDropAccepted,
    onDropRejected,
    maxFiles: 1,
    maxSize: 209715200,
    accept: acceptedFileTypes,
  });

  return (
    <Container className={dropZoneTextClass} {...getRootProps({ isDragActive })}>
      <input {...getInputProps()} />
      <div className={dropZoneTextClass}>
        {fileAvailable && (
          <span onClick={removeFile}>
            <MdCancel className="dropzone-icon" />
          </span>
        )}
        {dropZoneText}
        {!fileAvailable && (
          <p className="drop-icon">
            <MdFileUpload />
          </p>
        )}
        {fileToCheck && nutzungsbestimmungenAvailable && (
          <div onClick={(e) => e.stopPropagation()} className="terms-of-use">
            <label>
              <input
                type="checkbox"
                defaultChecked={checkedNutzungsbestimmungen}
                onChange={() => setCheckedNutzungsbestimmungen(!checkedNutzungsbestimmungen)}
              />
              <span className="nutzungsbestimmungen-input">
                Ich akzeptiere die{" "}
                <Button
                  variant="link"
                  className="terms-of-use link"
                  onClick={() => {
                    showNutzungsbestimmungen();
                  }}
                >
                  Nutzungsbestimmungen
                </Button>
                .
              </span>
            </label>
          </div>
        )}
        {testRunning && (
          <p>
            <Spinner className="spinner" animation="border" />
          </p>
        )}
        {fileAvailable && (
          <p className={!nutzungsbestimmungenAvailable && "added-margin"}>
            <Button
              className={fileToCheck && !testRunning ? "check-button" : "invisible-check-button"}
              onClick={checkFile}
              disabled={(nutzungsbestimmungenAvailable && !checkedNutzungsbestimmungen) || testRunning}
            >
              Validieren
            </Button>
          </p>
        )}
      </div>
    </Container>
  );
};

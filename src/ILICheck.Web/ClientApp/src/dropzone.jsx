import React, { useCallback, useState, useEffect } from "react";
import { useDropzone } from "react-dropzone";
import { MdCancel, MdFileUpload } from "react-icons/md";
import { Button, Spinner } from "react-bootstrap";
import styled from "styled-components";

const getColor = (props) => {
  if (props.isDragActive) {
    return "#124A4F";
  } else {
    return "#124A4F99";
  }
};

const Container = styled.div`
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  min-height: 15vh;
  max-width: 95vw;
  font-size: 2rem;
  border-width: 2px;
  border-radius: 5px;
  border-color: ${(props) => getColor(props)};
  border-style: dashed;
  background-color: #124a4f0d;
  outline: none;
  transition: border 0.24s ease-in-out;
`;

export const FileDropzone = ({
  setFileToCheck,
  setUploadLogsEnabled,
  fileToCheck,
  nutzungsbestimmungenAvailable,
  checkedNutzungsbestimmungen,
  checkFile,
  validationRunning,
  setCheckedNutzungsbestimmungen,
  showNutzungsbestimmungen,
  acceptedFileTypes,
  fileToCheckRef,
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

  const onDropAccepted = useCallback(
    (acceptedFiles) => {
      const updateDropZoneClass = () => {
        if (!checkFile || (nutzungsbestimmungenAvailable && !checkedNutzungsbestimmungen)) {
          setDropZoneTextClass("dropzone dropzone-text-disabled");
        } else {
          setDropZoneTextClass("dropzone dropzone-text-file");
        }
      };
      updateDropZoneClass();
      if (acceptedFiles.length === 1) {
        setDropZoneText(acceptedFiles[0].name);
        updateDropZoneClass();
        setFileToCheck(acceptedFiles[0]);
        fileToCheckRef.current = acceptedFiles[0];
        setFileAvailable(true);
      }
    },
    [checkFile, checkedNutzungsbestimmungen, fileToCheckRef, nutzungsbestimmungenAvailable, setFileToCheck]
  );

  const resetFileToCheck = useCallback(() => {
    setFileToCheck(null);
    fileToCheckRef.current = null;
  }, [fileToCheckRef, setFileToCheck]);

  const onDropRejected = useCallback(
    (fileRejections) => {
      setDropZoneTextClass("dropzone dropzone-text-error");
      const errorCode = fileRejections[0].errors[0].code;

      switch (errorCode) {
        case "file-invalid-type":
          setDropZoneText(
            `Der Dateityp wird nicht unterstützt. Bitte wähle eine Datei (max. 200MB) mit einer der folgenden Dateiendungen: ${acceptedFileTypes}`
          );
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
          setDropZoneText(
            `Bitte wähle eine Datei (max. 200MB) mit einer der folgenden Dateiendungen: ${acceptedFileTypes}`
          );
      }
      resetFileToCheck();
      setFileAvailable(false);
    },
    [resetFileToCheck, acceptedFileTypes]
  );

  const removeFile = (e) => {
    e.stopPropagation();
    setUploadLogsEnabled(false);
    resetFileToCheck();
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
    <div className="dropzone-wrapper">
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
          {validationRunning && (
            <div>
              <Spinner className="spinner" animation="border" />
            </div>
          )}
          {fileAvailable && (
            <p className={!nutzungsbestimmungenAvailable && "added-margin"}>
              <Button
                className={fileToCheck && !validationRunning ? "check-button" : "invisible-check-button"}
                onClick={checkFile}
                disabled={(nutzungsbestimmungenAvailable && !checkedNutzungsbestimmungen) || validationRunning}
              >
                Validieren
              </Button>
            </p>
          )}
        </div>
      </Container>
    </div>
  );
};

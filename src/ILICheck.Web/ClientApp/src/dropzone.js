/* eslint-disable react-hooks/exhaustive-deps */
import React, { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { MdCancel, MdFileDownload } from 'react-icons/md';
import { Button, Spinner } from 'react-bootstrap';
import styled from 'styled-components';

const getColor = (props) => {
    if (props.isDragActive) {
        return '#2196f3';
    }
    else {
        return '#d1d6d991';
    }
}

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
  border-color: ${props => getColor(props)};
  border-style: dashed;
  background-color: #d1d6d991;
  color:#9f9f9f;
  outline: none;
  transition: border .24s ease-in-out;
`;

export const FileDropzone = ({ setFileToCheck, connection, setUploadLogsEnabled, fileToCheck, nutzungsbestimmungenAvailable, checkedNutzungsbestimmungen, checkFile, testRunning, setCheckedNutzungsbestimmungen, showNutzungsbestimmungen }) => {
    const [fileAvailable, setFileAvailable] = useState(false);
    const [dropZoneText, setDropZoneText] = useState("Datei hier ablegen oder klicken um vom lokalen Dateisystem auszuwählen.");
    const [dropZoneTextClass, setDropZoneTextClass] = useState("dropzone dropzone-text-disabled");

    const updateDropZoneClass = () => {
        if (!checkFile || (nutzungsbestimmungenAvailable && !checkedNutzungsbestimmungen)) {
            setDropZoneTextClass("dropzone dropzone-text-disabled")
        } else {
            setDropZoneTextClass("dropzone dropzone-text-file")
        }
    }

    const onDropAccepted = useCallback(acceptedFiles => {
        updateDropZoneClass();
        if (acceptedFiles.length === 1) {
            setDropZoneText(acceptedFiles[0].name);
            updateDropZoneClass();
            setFileToCheck(acceptedFiles[0])
            setFileAvailable(true);
        }
    }, [setFileToCheck])

    const onDropRejected = useCallback(fileRejections => {
        setDropZoneTextClass("dropzone dropzone-text-error");
        const errorCode = fileRejections[0].errors[0].code;

        switch (errorCode) {
            case "file-invalid-type":
                setDropZoneText("Fehler: Nicht unterstütze Datei. Bitte wähle eine .xtf Datei aus.");
                break;
            case "too-many-files":
                setDropZoneText("Fehler: Es kann nur eine Datei aufs mal geprüft werden.");
                break;
            case "file-too-large":
                setDropZoneText("Fehler: Die ausgewählte Datei ist über 200MB gross. Bitte wähle eine kleinere Datei oder erstelle eine .zip Datei.");
                break;
            default:
                setDropZoneText("Fehler: Bitte wähle eine Datei des Typs .xtf oder .zip mit maximal 200MB aus.");
        }
        setFileToCheck(null)
        setFileAvailable(false);
    }, [setFileToCheck])

    const removeFile = () => {
        connection.stop();
        setUploadLogsEnabled(false);
        setFileToCheck(null);
        setFileAvailable(false);
        setDropZoneText("Datei hier ablegen oder klicken um vom lokalen Dateisystem auszuwählen.");
        setDropZoneTextClass("dropzone dropzone-text-disabled");
    }

    const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDropAccepted, onDropRejected, maxFiles: 1, maxSize: 209715200, accept: ".xtf, .xml, .zip" })

    return (
        <Container className={dropZoneTextClass} {...getRootProps({ isDragActive })}>
            <input {...getInputProps()} />
            <div className={dropZoneTextClass} onClick={(e) => e.stopPropagation()}>
                {fileAvailable && <span onClick={removeFile}><MdCancel className='dropzone-icon' /></span>}
                {dropZoneText}
                {fileAvailable &&
                    <Button className={fileToCheck && !testRunning ? "check-button" : "invisible-check-button"} onClick={checkFile}
                        disabled={(nutzungsbestimmungenAvailable && !checkedNutzungsbestimmungen) || testRunning}>
                        Validieren
                    </Button>}
                {!fileAvailable && <p className='drop-icon'><MdFileDownload/></p>}
                {testRunning && <Spinner className="spinner" animation="border" />}
                {fileToCheck && nutzungsbestimmungenAvailable &&
                    <div className="terms-of-use">
                        <label>
                            <input type="checkbox"
                                defaultChecked={checkedNutzungsbestimmungen}
                                onChange={() => setCheckedNutzungsbestimmungen(!checkedNutzungsbestimmungen)}
                            />
                            <span className="nutzungsbestimmungen-input">Ich akzeptiere die <Button variant="link" className="terms-of-use link" onClick={() => { showNutzungsbestimmungen() }}>Nutzungsbestimmungen</Button>.</span>
                        </label>
                    </div>}
            </div>
        </Container>
    )
}
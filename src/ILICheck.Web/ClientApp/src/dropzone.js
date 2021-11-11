import React, { useCallback, useState } from 'react'
import { useDropzone } from 'react-dropzone'
import { IoIosRemoveCircleOutline } from 'react-icons/io'
import styled from 'styled-components';

const getColor = (props) => {
    if (props.isDragActive) {
        return '#2196f3';
    }
}

const Container = styled.div`
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 15px;
  font-size: 20px;
  border-width: 2px;
  border-radius: 5px;
  border-color: ${props => getColor(props)};
  border-style: dashed;
  background-color: #fafafa;
  outline: none;
  transition: border .24s ease-in-out;
`;

export const FileDropzone = ({ setFileToCheck, connection, setUploadLogsEnabled }) => {
    const [fileAvailable, setFileAvailable] = useState(false);
    const [dropZoneText, setDropZoneText] = useState("Datei hier ablegen oder klicken um vom lokalen Dateisystem auszuwählen.");
    const [dropZoneTextClass, setDropZoneTextClass] = useState("dropzone-text");

    const onDropAccepted = useCallback(acceptedFiles => {
        if (acceptedFiles.length === 1) {
            setDropZoneText(acceptedFiles[0].name);
            setDropZoneTextClass("dropzone-text-file");
            setFileToCheck(acceptedFiles[0])
            setFileAvailable(true);
        }
    }, [setFileToCheck])

    const onDropRejected = useCallback(fileRejections => {
        setDropZoneTextClass("dropzone-text-error");
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

    const removeFile = (event) => {
        event.stopPropagation();
        connection.stop();
        setUploadLogsEnabled(false);
        setFileToCheck(null);
        setFileAvailable(false);
        setDropZoneText("Datei hier ablegen oder klicken um vom lokalen Dateisystem auszuwählen.");
        setDropZoneTextClass("dropzone-text");
    }

    const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDropAccepted, onDropRejected, maxFiles: 1, maxSize: 209715200, accept: ".xtf, .xml, .zip" })

    return (
        <Container className={dropZoneTextClass} {...getRootProps({ isDragActive })}>
            <input {...getInputProps()} />
            <div>{dropZoneText} {fileAvailable && <span onClick={removeFile}><IoIosRemoveCircleOutline /></span>}</div>
        </Container>
    )
}
import React, { useCallback, useState } from 'react'
import { useDropzone } from 'react-dropzone'
import { AiOutlineCloseCircle } from 'react-icons/ai'
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

export const FileDropzone = ({ setFileToCheck }) => {
    const [fileAvailable, setFileAvailable] = useState(false);
    const [dropZoneText, setDropZoneText] = useState(".xtf oder .xml file hier ablegen, oder klicken um auf dem Filesystem auszuwählen.");
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
                setDropZoneText("Fehler: Nicht unterstütze Datei. Bitte wählen Sie eine Datei des Typs .xtf oder .xml aus.");
                break;
            case "too-many-files":
                setDropZoneText("Fehler: Es kann nur eine Datei aufs mal geprüft werden.");
                break;
            case "file-too-large":
                setDropZoneText("Fehler: Die ausgewählte Datei ist über 5MB gross. Bitte wählen Sie eine kleinere Datei.");
                break;
            default:
                setDropZoneText("Fehler: Bitte wählen Sie eine Datei des Typs .xtf oder .xml mit maximal 5MB aus.");
        }
        setFileToCheck(null)
        setFileAvailable(false);
    }, [setFileToCheck])

    const removeFile = (event) => {
        event.stopPropagation();
        setFileToCheck(null);
        setFileAvailable(false);
        setDropZoneText(".xtf oder .xml file hier ablegen, oder klicken um auf dem Filesystem auszuwählen.");
        setDropZoneTextClass("dropzone-text");
    }

    const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDropAccepted, onDropRejected, maxFiles: 1, maxSize: 5000000, accept: ".xtf, .xml" })

    return (
        <Container className={dropZoneTextClass} {...getRootProps({ isDragActive })}>
            <input {...getInputProps()} />
            <p>{dropZoneText} {fileAvailable && <span className="remove-icon" onClick={removeFile}><AiOutlineCloseCircle /></span>}</p>
        </Container>
    )
}
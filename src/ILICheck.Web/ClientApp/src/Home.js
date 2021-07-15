import './App.css';
import React, { useState, useEffect} from 'react';
import { usePDF } from '@react-pdf/renderer';
import { Button, Container } from 'react-bootstrap';
import { AiOutlinePlayCircle } from 'react-icons/ai';
import { FileDropzone } from './Dropzone';
import { ProtokollPdf } from './ProtokollPdf';
import Protokoll from './Protokoll';

export const Home = props => {
  const { connection, logMessage } = props;
  const [fileToCheck, setFileToCheck] = useState(null);
  const [testRunning, setTestRunning] = useState(false);
  const [testRunTime, setTestRunTime] = useState(null);
  const [pdf, updatePdf] = usePDF({ document: ProtokollPdf });
  const [protokollName, setProtokollName] = useState("");
  const [fileCheckStatusClass, setFileCheckStatusClass] = useState("");
  const [fileCheckStatus, setFileCheckStatus] = useState("");
  const [checkRun, setCheckRun] = useState(1); // used to simulate status styling
  const [log, setLog] = useState([])


  // Reset log on file change
  useEffect(() => {
    setLog([]);
  }, [fileToCheck])

  useEffect(() => {
    if(logMessage) {
        setLog(log => [...log, logMessage] );
    }
  }, [logMessage])

  const checkFile = () => {
    connection.invoke("StartValidation", connection.connectionId, fileToCheck.name);
    setTestRunning(true);

      setTestRunTime(new Date().toLocaleString());
      setProtokollName("Check_result_" + fileToCheck.name + "-" + testRunTime);
      setCheckRun(checkRun + 1);// used to simulate status styling
      console.log(checkRun);// used to simulate status styling
      if (checkRun === 1) {
        setFileCheckStatusClass("valid")// used to simulate status styling
        setFileCheckStatus("Datei enthält keine Fehler!")
      }
      if (checkRun === 2) {
        setFileCheckStatusClass("warnings")// used to simulate status styling
        setFileCheckStatus("Datei enthält Warnungen!")
      }
      if (checkRun === 3) {
        setFileCheckStatusClass("errors")// used to simulate status styling
        setFileCheckStatus("Datei enthält Fehler!")
      }
      setTestRunning(false);
  }

  return (
      <div className="app">
        <header className="app-header">
          <p>
            INTERLIS Web-Check-Service
          </p>
        </header>
        <Container>
          <FileDropzone setFileToCheck={setFileToCheck} />
          <Button variant="success" className={fileToCheck ? "" : "invisible-check-button"} onClick={checkFile}>Check
            <span className="run-icon">
              {testRunning ? (<div className="spinner-border spinner-border-sm text-light"></div>) : (<AiOutlinePlayCircle />)}
            </span>
          </Button>
        </Container>
        <Protokoll fileCheckStatus={fileCheckStatus} fileCheckStatusClass={fileCheckStatusClass} log={log} testRunTime={testRunTime} protokollName ={protokollName} pdf={ pdf}/>
      </div>
  );
}

export default Home;

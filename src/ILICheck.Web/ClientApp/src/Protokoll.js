import './App.css';
import React from 'react';
import { Card, Container } from 'react-bootstrap';
import { GoFile, GoFileCode} from 'react-icons/go';

export const Protokoll = props => {
  const { log, fileCheckStatus, connection, closedConnectionId } = props;
  const protokollNameLog = "Check_result_" + fileCheckStatus.fileName + "-" + fileCheckStatus.testRunTime + ".log";
  const protokollNameXtf = "Check_result_" + fileCheckStatus.fileName + "-" + fileCheckStatus.testRunTime + ".xtf";
  let downloadLogUrl;
  let downloadXTFUrl;
  if (connection && fileCheckStatus.class === "valid") {
    downloadLogUrl = `api/download?connectionId=${closedConnectionId}&fileExtension=.log`
    downloadXTFUrl = `api/download?connectionId=${closedConnectionId}&fileExtension=.xtf`
  }

  return (
    <Container>
      {log.length > 0 && <Card className="protokoll-card">
        <Card.Body>
          <Card.Title className={fileCheckStatus.class}>{fileCheckStatus.text} Testausführung: {fileCheckStatus.testRunTime}
            {downloadLogUrl && downloadXTFUrl &&
              <span>
                <span title="Log-Datei herunterladen.">
                  <a download={protokollNameLog} className="download-icon" href={downloadLogUrl}><GoFile /></a>
                </span>
                <span title="XTF-Log-Datei herunterladen.">
                  <a download={protokollNameXtf} className="download-icon" href={downloadXTFUrl}><GoFileCode /></a>
                </span>
              </span>
            }
          </Card.Title>
          <div className="protokoll">
            {log.map((logEntry, index) => (
              <div key={index}>{logEntry}</div>
            ))}
          </div>
        </Card.Body>
      </Card>}
    </Container>
  );
}

export default Protokoll;

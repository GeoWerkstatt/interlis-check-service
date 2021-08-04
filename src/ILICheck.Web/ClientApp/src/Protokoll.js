import './App.css';
import React from 'react';
import { Card, Container } from 'react-bootstrap';
import { AiOutlineDownload } from 'react-icons/ai';

export const Protokoll = props => {
  const { log, fileCheckStatus, connection } = props;
  const protokollName = "Check_result_" + fileCheckStatus.fileName + "-" + fileCheckStatus.testRunTime + ".xtf";
  let downloadUrl;
  if (connection && fileCheckStatus.class === "valid") {
    downloadUrl = `api/download?connectionId=${connection.connectionId}`
  }

  return (
    <Container>
      {log.length > 0 && <Card className="protokoll-card">
        <Card.Body>
          <Card.Title className={fileCheckStatus.class}>{fileCheckStatus.text} Testausführung: {fileCheckStatus.testRunTime}
            {downloadUrl &&
              <span title="Protokolldatei herunterladen.">
                <a download={protokollName} className="download-icon" href={downloadUrl}><AiOutlineDownload /></a>
              </span>
            }
          </Card.Title>
          <Card.Text className="protokoll">
            {log.map(logEntry => (
              <p key={log.indexOf(logEntry)} >{logEntry}</p>
            ))}
          </Card.Text>
        </Card.Body>
      </Card>}
    </Container>
  );
}

export default Protokoll;

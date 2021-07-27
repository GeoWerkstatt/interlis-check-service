import './App.css';
import React from 'react';
import { Card, Container } from 'react-bootstrap';
import { AiOutlineFileText, AiOutlineFilePdf } from 'react-icons/ai';

export const Protokoll = props => {
  const { log, fileCheckStatus, pdf } = props;
  const protokollName = "Check_result_" + fileCheckStatus.fileName + "-" + fileCheckStatus.testRunTime;


  const downloadTxtFile = () => {
    const element = document.createElement("a");
    const file = new Blob([log], { type: 'text/plain' });
    element.href = URL.createObjectURL(file);
    element.download = protokollName + ".txt";
    element.click();
  }

  return (
    <Container>
      {log.length > 0 && <Card className="protokoll-card">
        <Card.Body>
          <Card.Title className={fileCheckStatus.class}>{fileCheckStatus.text} Testausführung: {fileCheckStatus.testRunTime}
            {fileCheckStatus.text &&
              <span title="Textfile herunterladen.">
                <span className="download-icon" onClick={downloadTxtFile}><AiOutlineFileText /></span>
                <a href={pdf.url} download={protokollName + ".pdf"} target="_blank" rel="noreferrer" title="PDF herunterladen.">
                  <span className="download-icon"><AiOutlineFilePdf /></span>
                </a>
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

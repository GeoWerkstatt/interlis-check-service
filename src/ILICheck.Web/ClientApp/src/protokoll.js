import './app.css';
import React, { useState, useRef, useEffect } from 'react';
import DayJS from 'dayjs'
import { Card, Container } from 'react-bootstrap';
import { GoFile, GoFileCode } from 'react-icons/go';
import { BsLink45Deg } from 'react-icons/bs';

export const Protokoll = props => {
  const { log, fileCheckStatus, connection, closedConnectionId, testRunning } = props;
  const copyToClipboardTooltipDefaultText = "XTF-Log-Datei Link in die Zwischenablage kopieren";
  const [copyToClipboardTooltipText, setCopyToClipboardTooltipText] = useState(copyToClipboardTooltipDefaultText);
  const [indicateWaiting, setIndicateWaiting] = useState(false);
  const protokollTimestamp = DayJS(fileCheckStatus.testRunTime).format('YYYYMMDDHHmm');
  const protokollFileName = "Ilivalidator_output_" + fileCheckStatus.fileName + "-" + protokollTimestamp;
  const xtfLogFileExtension = ".xtf";
  const logFileExtension = ".log";
  const downloadAvailable = connection && fileCheckStatus.fileDownloadAvailable;
  const downloadUrl = `api/v1/download?connectionId=${closedConnectionId}&fileExtension=`;

  // Copy to clipboard
  const resetToDefaultText = () => setCopyToClipboardTooltipText(copyToClipboardTooltipDefaultText);
  const copyToClipboard = () => {
    navigator.clipboard.writeText(window.location + downloadUrl + xtfLogFileExtension);
    setCopyToClipboardTooltipText("Link wurde kopiert");
  }

  // Autoscroll protokoll log
  const logEndRef = useRef(null);
  useEffect(() => logEndRef.current?.scrollIntoView({ behavior: "smooth" }), [log]);
  
  // Show flash dot to indicate waiting
  useEffect(() =>  setTimeout(() => { if(testRunning === true) {setIndicateWaiting(!indicateWaiting)} else {setIndicateWaiting(false)}}, 500))

  return (
    <Container>
      {log.length > 0 && <Card className="protokoll-card">
        <Card.Body>
          <div className="protokoll">
            {log.map((logEntry, index) => <div key={index}>{logEntry}{indicateWaiting && index ===log.length -1 &&  "."}</div>)}
            <div ref={logEndRef} />
          </div>
          <Card.Title className={fileCheckStatus.class}>{fileCheckStatus.text}
            {downloadAvailable &&
              <span>
                <span className="icon-tooltip">
                  <a download={protokollFileName + logFileExtension} className={fileCheckStatus.class + " download-icon"} href={downloadUrl + logFileExtension}><GoFile /></a>
                  <span className="icon-tooltip-text">Log-Datei herunterladen</span>
                </span>
                <span className="icon-tooltip">
                  <a download={protokollFileName + xtfLogFileExtension} className={fileCheckStatus.class + " download-icon"} href={downloadUrl + xtfLogFileExtension}><GoFileCode /></a>
                  <span className="icon-tooltip-text">XTF-Log-Datei herunterladen</span>
                </span>
                <span className="icon-tooltip">
                  <div className={fileCheckStatus.class + "btn-sm download-icon"} onClick={copyToClipboard} onMouseLeave={resetToDefaultText}><BsLink45Deg />
                    <span className="icon-tooltip-text">{copyToClipboardTooltipText}</span>
                  </div>
                </span>
              </span>
            }
          </Card.Title>
        </Card.Body>
      </Card>}
    </Container>
  );
}

export default Protokoll;

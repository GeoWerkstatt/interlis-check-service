import './app.css';
import './custom.css';
import React, { useState, useRef, useEffect } from 'react';
import DayJS from 'dayjs'
import { Button, Card, Container } from 'react-bootstrap';
import { GoFile, GoFileCode } from 'react-icons/go';
import { BsLink45Deg } from 'react-icons/bs';

export const Protokoll = props => {
  const { log, fileCheckStatus, connection, closedConnectionId } = props;
  const copyToClipboardTooltipDefaultText = "XTF-Log-Datei Link in die Zwischenablage kopieren";
  const [copyToClipboardTooltipText, setCopyToClipboardTooltipText] = useState(copyToClipboardTooltipDefaultText);

  const protokollTimestamp = DayJS(fileCheckStatus.testRunTime).format('YYYYMMDDHHmm');
  const protokollFileName = "Ilivalidator_output_" + fileCheckStatus.fileName + "-" + protokollTimestamp;

  const xtfLogFileExtension = ".xtf";
  const logFileExtension = ".log";
  const downloadAvailable = connection && fileCheckStatus.fileDownloadAvailable;
  const downloadUrl = `api/download?connectionId=${closedConnectionId}&fileExtension=`;

  // Copy to clipboard
  const resetToDefaultText = () => setCopyToClipboardTooltipText(copyToClipboardTooltipDefaultText);
  const copyToClipboard = () => {
    navigator.clipboard.writeText(window.location + downloadUrl + xtfLogFileExtension);
    setCopyToClipboardTooltipText("Link wurde kopiert");
  }

  // Autoscroll protokoll log
  const logEndRef = useRef(null);
  const scrollToBottom = () => logEndRef.current?.scrollIntoView({ behavior: "smooth" });
  useEffect(() => scrollToBottom(), [log]);

  return (
    <Container>
      {log.length > 0 && <Card className="protokoll-card">
        <Card.Body>
          <Card.Title className={fileCheckStatus.class}>{fileCheckStatus.text} Testausführung: {fileCheckStatus.testRunTime?.toLocaleString()}
            {downloadAvailable &&
              <span>
                <span title="Log-Datei herunterladen.">
                  <a download={protokollFileName + logFileExtension} className={fileCheckStatus.class + " download-icon"} href={downloadUrl + logFileExtension}><GoFile /></a>
                </span>
                <span title="XTF-Log-Datei herunterladen.">
                  <a download={protokollFileName + xtfLogFileExtension} className={fileCheckStatus.class + " download-icon"} href={downloadUrl + xtfLogFileExtension}><GoFileCode /></a>
                </span>
                <span className="copy-tooltip">
                  <Button variant="secondary" className="btn-sm btn-copy-to-clipboard" onClick={copyToClipboard} onMouseLeave={resetToDefaultText}>
                    <BsLink45Deg />
                    <span className="copy-icon">
                      <span className="copy-tooltip-text" id="copy-tooltip">{copyToClipboardTooltipText}</span>
                    </span>
                    Link kopieren
                  </Button>
                </span>
              </span>
            }
          </Card.Title>
          <div className="protokoll">
            {log.map((logEntry, index) => <div key={index}>{logEntry}</div>)}
            <div ref={logEndRef} />
          </div>
        </Card.Body>
      </Card>}
    </Container>
  );
}

export default Protokoll;

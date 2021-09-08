import './App.css';
import React from 'react';
import { Button, Card, Container } from 'react-bootstrap';
import { GoFile, GoFileCode } from 'react-icons/go';
import { BsLink45Deg } from 'react-icons/bs';

export const Protokoll = props => {
  const { log, fileCheckStatus, connection, closedConnectionId } = props;
  const protokollNameLog = "Ilivalidator_output_" + fileCheckStatus.fileName + "-" + fileCheckStatus.testRunTime + ".log";
  const protokollNameXtf = "Ilivalidator_output_" + fileCheckStatus.fileName + "-" + fileCheckStatus.testRunTime + ".xtf";
  let downloadLogUrl;
  let downloadXTFUrl;
  if (connection && fileCheckStatus.fileDownloadAvailable) {
    downloadLogUrl = `api/download?connectionId=${closedConnectionId}&fileExtension=.log`
    downloadXTFUrl = `api/download?connectionId=${closedConnectionId}&fileExtension=.xtf`
  }

  // Copy to clipboard
  let copyToClipboardTooltipRef = React.createRef();
  let copyToClipboardTooltipDefaultText = "XTF-Log-Datei Link in die Zwischenablage kopieren";
  const resetToDefaultText = () => copyToClipboardTooltipRef.current.innerHTML = copyToClipboardTooltipDefaultText
  const copyToClipboard = () => {
    navigator.clipboard.writeText(window.location + downloadXTFUrl);
    copyToClipboardTooltipRef.current.innerHTML = "Link wurde kopiert";
  }

  return (
    <Container>
      {log.length > 0 && <Card className="protokoll-card">
        <Card.Body>
          <Card.Title className={fileCheckStatus.class}>{fileCheckStatus.text} Testausführung: {fileCheckStatus.testRunTime}
            {downloadLogUrl && downloadXTFUrl &&
              <span>
                <span title="Log-Datei herunterladen.">
                  <a download={protokollNameLog} className={fileCheckStatus.class + " download-icon"} href={downloadLogUrl}><GoFile /></a>
                </span>
                <span title="XTF-Log-Datei herunterladen.">
                  <a download={protokollNameXtf} className={fileCheckStatus.class + " download-icon"} href={downloadXTFUrl}><GoFileCode /></a>
                </span>
                <span class="copy-tooltip">
                  <Button variant="secondary" className="btn-sm btn-copy-to-clipboard" onClick={copyToClipboard} onMouseLeave={resetToDefaultText}>
                    <BsLink45Deg />
                    <span class="copy-icon">
                      <span class="copy-tooltip-text" ref={copyToClipboardTooltipRef} id="copy-tooltip">{copyToClipboardTooltipDefaultText}</span>
                    </span>
                    Link kopieren
                  </Button>
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

import './app.css';
import React, { useState, useEffect } from 'react';
import githubLogo from './img/github.png'
import qgisLogo from './img/qgis.png'
import Home from './home';
import ImpressumModal from './impressum';
import DatenschutzModal from './datenschutz';
import HilfeModal from './hilfe';
import { Button } from 'react-bootstrap';

export const Layout = props => {
  const { connection, closedConnectionId, log, updateLog, resetLog, setUploadLogsInterval, validationResult, setValidationResult, setUploadLogsEnabled } = props;
  const [showImpressum, setShowImpressum] = useState(false);
  const [showDatenschutz, setShowDatenschutz] = useState(false);
  const [showHilfe, setShowHilfe] = useState(false);

  const [clientSettings, setClientSettings] = useState(null);
  const [datenschutzContent, setDatenschutzContent] = useState(null);
  const [impressumContent, setImpressumContent] = useState(null);
  const [infoHilfeContent, setInfoHilfeContent] = useState(null);
  const [quickStartContent, setQuickStartContent] = useState(null);

  // Update HTML title property
  useEffect(() => document.title = clientSettings?.applicationName, [clientSettings])

  // Fetch client settings
  useEffect(() => {
    fetch('api/settings')
      .then(res => res.headers.get('content-type')?.includes('application/json') && res.json())
      .then(json => setClientSettings(json));
  }, []);

  // Fetch optional custom content
  useEffect(() => {
    fetch('impressum.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setImpressumContent(text));

    fetch('datenschutz.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setDatenschutzContent(text));

    fetch('info-hilfe.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setInfoHilfeContent(text));

    fetch('quickstart.txt')
      .then(res => res.headers.get('content-type')?.includes('text/plain') && res.text())
      .then(text => setQuickStartContent(text));
  }, []);

  return (
    <div className="app">
      <header className="header-style">
        <div className="icon">
          <a href={clientSettings?.vendorLink} target="_blank" rel="noreferrer">
            <img className="vendor-logo" src="/vendor.png" alt="Vendor Logo" onError={(e) => { e.target.style.display='none'}} />
          </a>
        </div>
        <div className="subtitle">{clientSettings?.applicationName} - online&nbsp;
          <a href="https://www.interlis.ch/downloads/ilivalidator" title="Zum ilivalidator" target="_blank" rel="noreferrer">livalidator</a>
        </div>
      </header>
      <main>
        <Home connection={connection}
              closedConnectionId={closedConnectionId}
              validationResult ={validationResult}
              setValidationResult ={setValidationResult}
              clientSettings={clientSettings}
              quickStartContent={quickStartContent}
              log={log}
              updateLog={updateLog}
              resetLog={resetLog}
              setUploadLogsInterval={setUploadLogsInterval}
              setUploadLogsEnabled={setUploadLogsEnabled} />
      </main>
      <footer className="footer-style">
        {impressumContent && <Button variant="link" className="flex-item footer-button" onClick={() => setShowImpressum(true)}>
          IMPRESSUM
        </Button>}
        {datenschutzContent && <Button variant="link" className="flex-item footer-button no-outline-on-focus" onClick={() => setShowDatenschutz(true)}>
          DATENSCHUTZ
        </Button>}
        {infoHilfeContent && <Button variant="link" className="flex-item footer-button" onClick={() => setShowHilfe(true)}>
          INFO & HILFE
        </Button>}
        <div className="flex-item version-info">
          <span className="version-tag">{clientSettings?.applicationName} ({process.env.REACT_APP_VERSION ? 'v' + process.env.REACT_APP_VERSION + '+' : ''}{process.env.REACT_APP_REVISION ?? process.env.NODE_ENV}), ilivalidator ({clientSettings?.ilivalidatorVersion})</span>
        </div>
        <div className="flex-icons">
          <a href="https://github.com/GeoWerkstatt/interlis-check-service" title="Link zum github reporsitory" target="_blank" rel="noreferrer">
            <img className="icon" src={githubLogo} color="#c1c1c1" width="40" alt="GitHub Logo" />
          </a>
          <a href="https://plugins.qgis.org/plugins/xtflog_checker/" title="Link zum QGIS Plugin XTFLog Checker" target="_blank" rel="noreferrer">
            <img className="icon" src={qgisLogo} color="#c1c1c1" width="40" alt="QGIS Logo" />
          </a>
        </div>
      </footer>
      < ImpressumModal
        className="modal"
        show={showImpressum}
        content={impressumContent}
        onHide={() => setShowImpressum(false)}
      />
      < DatenschutzModal
        className="modal"
        show={showDatenschutz}
        content={datenschutzContent}
        onHide={() => setShowDatenschutz(false)}
      />
      < HilfeModal
        className="modal"
        show={showHilfe}
        content={infoHilfeContent}
        onHide={() => setShowHilfe(false)}
      />
    </div>
  );
}

export default Layout;

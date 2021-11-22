import './app.css';
import React, { useState } from 'react';
import vendorLogo from './img/vendor.png'
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

  return (
    <div className="app">
      <header className="header-style">
        <div className="icon">
          <a href="https://www.example.com" title="www.example.com" target="_blank" rel="noreferrer">
            <img src={vendorLogo} width="150" alt="Vendor Logo" />
          </a>
        </div>
        <div className="subtitle">INTERLIS Web-Check-Service - online <a href="https://www.interlis.ch/downloads/ilivalidator" title="Zum ilivalidator" target="_blank" rel="noreferrer">Ilivalidator</a></div>
      </header>
      <main>
        <Home connection={connection}
              closedConnectionId={closedConnectionId}
              validationResult ={validationResult}
              setValidationResult ={setValidationResult}
              log={log}
              updateLog={updateLog}
              resetLog={resetLog}
              setUploadLogsInterval={setUploadLogsInterval}
              setUploadLogsEnabled={setUploadLogsEnabled} />
      </main>
      <footer className="footer-style">
        <Button variant="link" className="flex-item footer-button" onClick={() => setShowImpressum(true)}>
          IMPRESSUM
        </Button>
        <Button variant="link" className="flex-item footer-button" onClick={() => setShowDatenschutz(true)}>
          DATENSCHUTZ
        </Button>
        <Button variant="link" className="flex-item footer-button" onClick={() => setShowHilfe(true)}>
          INFO & HILFE
        </Button>
        <div className="flex-item version-info">
          <span className="version-tag">INTERLIS Web-Check-Service ({process.env.REACT_APP_VERSION ? 'v' + process.env.REACT_APP_VERSION + '+' : ''}{process.env.REACT_APP_REVISION ?? process.env.NODE_ENV}), ilivalidator (1.11.12), ili2gpkg (4.6.0)</span>
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
        onHide={() => setShowImpressum(false)}
      />
      < DatenschutzModal
        className="modal"
        show={showDatenschutz}
        onHide={() => setShowDatenschutz(false)}
      />
      < HilfeModal
        className="modal"
        show={showHilfe}
        onHide={() => setShowHilfe(false)}
      />
    </div>
  );
}

export default Layout;

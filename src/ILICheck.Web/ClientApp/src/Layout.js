import './App.css';
import React from 'react';
import geow_logo from './img/GeoW.png'
import github_logo from './img/GitHub-Mark-Light-64px.png'
import qgis_logo from './img/qgis_transparent.png'
import Home from './Home';

export const Layout = props => {
  const { connection, closedConnectionId, log, updateLog, resetLog, setUploadLogsInterval, validationResult, setValidationResult, setUploadLogsEnabled } = props;

  return (
    <div className="app">
      <header className="header-style">
        <div className="icon">
          <a href="https://www.geowerkstatt.ch" title="www.geowerkstatt.ch" target="_blank" rel="noreferrer">
            <img src={geow_logo} width="150" alt="GeoWerkstatt_Logo" />
          </a>
        </div>
        <div className="subtitle">ilicop - online <a href="https://www.interlis.ch/downloads/ilivalidator" title="Zum ilivalidator" target="_blank" rel="noreferrer">Ilivalidator</a></div>
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
        <div className="flex-item"></div>
        <div className="flex-item"></div>
        <div className="flex-item"></div>
        <div className="flex-icons">
          <a href="https://github.com/GeoWerkstatt/interlis-check-service" title="Link zum github reporsitory" target="_blank" rel="noreferrer">
            <img className="icon" src={github_logo} color="#c1c1c1" width="40" alt="Github_Logo" />
          </a>
          <a href="https://plugins.qgis.org/plugins/xtflog_checker/" title="Link zum QGIS Plugin XTFLog Checker" target="_blank" rel="noreferrer">
            <img className="icon" src={qgis_logo} color="#c1c1c1" width="40" alt="QGIS_Logo" />
          </a>
          <span className="version-tag">{process.env.REACT_APP_VERSION ? 'v' + process.env.REACT_APP_VERSION + '+' : ''}{process.env.REACT_APP_REVISION ?? process.env.NODE_ENV}</span>
        </div>
      </footer>
    </div>
  );
}

export default Layout;

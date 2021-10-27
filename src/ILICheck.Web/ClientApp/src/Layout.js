import './App.css';
import React from 'react';
import Home from './Home';

export const Layout = props => {
  const { connection, closedConnectionId, log, updateLog, resetLog, setUploadLogsInterval } = props;

  return (
    <div className="app">
      <header className="header-style">
      </header>
      <main>
        <Home connection={connection} closedConnectionId={closedConnectionId} log={log} updateLog={updateLog} resetLog={resetLog} setUploadLogsInterval={setUploadLogsInterval} />
      </main>
      <footer className="footer-style">
          <span className="version-tag">{process.env.REACT_APP_VERSION ? 'v' + process.env.REACT_APP_VERSION + '+' : ''}{process.env.REACT_APP_REVISION ?? process.env.NODE_ENV}</span>
      </footer>
    </div>
  );
}

export default Layout;

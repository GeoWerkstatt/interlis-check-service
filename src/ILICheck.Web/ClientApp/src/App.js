import './App.css';
import './CI_geow.css';
import React, { useState, useEffect, useCallback } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import Layout from './Layout';
import { Settings } from './Settings';

function App() {
  const [connection, setConnection] = useState(null);
  const [log, setLog] = useState([]);
  const [closedConnectionId, setClosedConnectionId] = useState("");
  const [uploadLogsInterval, setUploadLogsInterval] = useState(-1);
  const [uploadLogsEnabled, setUploadLogsEnabled] = useState(false);

  const resetLog = useCallback(() => setLog([]), [ setLog ]);
  const updateLog = useCallback((message, { disableUploadLogs = true } = {}) => {
    if (disableUploadLogs) setUploadLogsEnabled(false);
    setLog(log => [...log, message]);
  }, []);

  useEffect(() => setUploadLogsEnabled(true), [uploadLogsInterval]);
  useEffect(() => !uploadLogsEnabled && clearInterval(uploadLogsInterval), [uploadLogsEnabled, uploadLogsInterval]);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("/hub")
      .build();

    connection.on('confirmConnection', (message) => {
      console.log('Message:', message);
    });

    connection.on('updateLog', (message) => updateLog(message));

    connection.on('stopConnection', () => {
      setClosedConnectionId(connection.connectionId)
      connection.stop();
    });

    connection.start().then(a => {
      if (connection.connectionId) {
        connection.invoke('SendConnectionId', connection.connectionId);
      }
    }).catch((e) => console.log('Error: ', e));

    setConnection(connection)
  }, [closedConnectionId, updateLog])

  return (
      <Layout connection={connection} closedConnectionId={closedConnectionId} settings={JSON.parse(Settings)} log={log} updateLog={updateLog} resetLog={resetLog} setUploadLogsInterval={setUploadLogsInterval} />
  );
}

export default App;

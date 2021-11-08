import './App.css';
import React, { useState, useEffect, useCallback } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import Layout from './Layout';

function App() {
  const [connection, setConnection] = useState(null);
  const [log, setLog] = useState([]);
  const [closedConnectionId, setClosedConnectionId] = useState("");
  const [uploadLogsInterval, setUploadLogsInterval] = useState(0);
  const [uploadLogsEnabled, setUploadLogsEnabled] = useState(false);
  const [validationResult, setValidationResult] = useState(false);

  const resetLog = useCallback(() => setLog([]), [setLog]);
  const updateLog = useCallback((message, { disableUploadLogs = true } = {}) => {
    if (disableUploadLogs) setUploadLogsEnabled(false);
    setLog(log => [...log, message]);
  }, []);

  useEffect(() => uploadLogsInterval && setUploadLogsEnabled(true), [uploadLogsInterval]);
  useEffect(() => !uploadLogsEnabled && clearInterval(uploadLogsInterval), [uploadLogsEnabled]);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("/hub")
      .build();

    async function start() {
      try {
        await connection.start().then(a => {
          if (connection.connectionId) {
            connection.invoke('SendConnectionId', connection.connectionId);
          }
        }).catch((e) => console.log('Error: ', e));
      } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
      }
    };

    connection.on('confirmConnection', (message) => {
      console.log('Message:', message);
    });

    connection.on('updateLog', (message) => updateLog(message));

    connection.on('stopConnection', () => {
      setClosedConnectionId(connection.connectionId)
      connection.stop();
    });

    connection.on('validatedWithErrors', (message) => {
      updateLog(message)
      setValidationResult("error")
    });

    connection.on('validatedWithoutErrors', (message) => {
      updateLog(message)
      setValidationResult("ok")
    });

    connection.on('validationAborted', (message) => {
      updateLog(message)
      setValidationResult("aborted")
    });

    connection.onclose(async () => {
      await start();
    });

    start();

    setConnection(connection)
  }, [updateLog])

  return (
    <Layout connection={connection}
      closedConnectionId={closedConnectionId}
      log={log} updateLog={updateLog}
      resetLog={resetLog}
      validationResult={validationResult}
      setValidationResult={setValidationResult}
      setUploadLogsInterval={setUploadLogsInterval}
      setUploadLogsEnabled={setUploadLogsEnabled} />
  );
}

export default App;

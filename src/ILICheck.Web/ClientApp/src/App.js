import './App.css';
import React, { useState, useEffect } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import Home from './Home';

function App() {
  const [connection, setConnection] = useState(null);
  const [log, setLog] = useState([]);
  const [closedConnectionId, setClosedConnectionId] = useState("");

  const updateLog = (message) => {
    setLog(log => [...log, message]);
  }

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("/hub")
      .build();

    connection.on('confirmConnection', (message) => {
      console.log('Message:', message);
    });

    connection.on('uploadStarted', (message) => {
      updateLog(message)
    });

    connection.on('fileUploading', (message) => {
      updateLog(message)
    });

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
  }, [closedConnectionId])

  return (
    <div>
      <Home connection={connection} closedConnectionId={closedConnectionId} log={log} setLog={setLog} />
    </div>
  );
}

export default App;

import './App.css';
import React, { useState, useEffect, useCallback } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import Home from './Home';

function App() {
  const [connection, setConnection] = useState(null);
  const [log, setLog] = useState([]);
  const [closedConnectionId, setClosedConnectionId] = useState("");

  const updateLog = useCallback((message) => setLog(log => [...log, message]), [ setLog ]);
  const resetLog = useCallback(() => setLog([]), [ setLog ]);

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
    <div>
      <Home connection={connection} closedConnectionId={closedConnectionId} log={log} updateLog={updateLog} resetLog={resetLog} />
    </div>
  );
}

export default App;

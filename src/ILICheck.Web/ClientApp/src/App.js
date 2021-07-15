import './App.css';
import React, { useState, useEffect } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import Home from './Home';

function App() {
  const [connection, setConnection] = useState(null); 
  const [logMessage, setLogMessage] = useState(null);
  
  const updateLog=(message)=>{
    setLogMessage(message);
    console.log('SignalR Message:', message);
  }

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("/hub")
      .build();
    
      connection.on('confirmConnection', (message) => {
        console.log('SignalR Message:', message);
      });

      connection.on('validationStarted', (message) => {
        updateLog(message)
      });

      connection.on('secondValidationPass', (message) => {
        updateLog(message)
      });

      connection.on('firstValidationPass', (message) => {
        updateLog(message)
      });

      connection.on('validationDone', (message) => {
        updateLog(message)

      });

    connection.start().then(a => {
      if (connection.connectionId) {
        connection.invoke("SendConnectionId", connection.connectionId);
      }
    }).catch((e) => console.log("Error SignalR: ", e));

    setConnection(connection)
  }, [])

  return (
    <div>
      <Home connection={connection} logMessage={logMessage} />
    </div>
  );
}

export default App;

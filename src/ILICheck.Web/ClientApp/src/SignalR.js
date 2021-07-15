import { HubConnectionBuilder } from '@microsoft/signalr';

export function MakeSignalRConnection (){
  console.log("Start signalR use effect")
  const connection = new HubConnectionBuilder()
    .withUrl("/hub")
    .build();

  connection.on('validationStarted', (message) => {
    console.log('SignalR Message:', message);
  });

  connection.on('confirmConnection', (message) => {
    console.log('SignalR Message:', message);
  });

  connection.on('secondValidationPass', (message) => {
    console.log('SignalR Message:', message);
  });

  connection.on('firstValidationPass', (message) => {
    console.log('SignalR Message:', message);
  });

  connection.on('validationDone', (message) => {
    console.log('SignalR Message:', message);
  });


  connection.start().then(a => {
    if (connection.connectionId) {
      connection.invoke("SendConnectionId", connection.connectionId);
    }
  }).catch((e) => console.log("Error SignalR: ", e));

  return connection;
}

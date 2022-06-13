import "./app.css";
import React, { useState, useEffect, useCallback } from "react";
import Layout from "./layout";

function App() {
  const [log, setLog] = useState([]);
  const [uploadLogsInterval, setUploadLogsInterval] = useState(0);
  const [uploadLogsEnabled, setUploadLogsEnabled] = useState(false);

  const resetLog = useCallback(() => setLog([]), [setLog]);
  const updateLog = useCallback((message, { disableUploadLogs = true } = {}) => {
    if (disableUploadLogs) setUploadLogsEnabled(false);
    setLog((log) => {
      if (message === log[log.length - 1]) return log;
      else return [...log, message];
    });
  }, []);

  useEffect(() => uploadLogsInterval && setUploadLogsEnabled(true), [uploadLogsInterval]);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => !uploadLogsEnabled && clearInterval(uploadLogsInterval), [uploadLogsEnabled]);

  return (
    <Layout
      log={log}
      updateLog={updateLog}
      resetLog={resetLog}
      setUploadLogsInterval={setUploadLogsInterval}
      setUploadLogsEnabled={setUploadLogsEnabled}
    />
  );
}

export default App;

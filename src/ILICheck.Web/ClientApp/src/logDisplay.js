import { useEffect, useState } from "react";
import { Card } from "react-bootstrap";
import LogDisplayEntry from "./logDisplayEntry";
import { createLogHierarchy } from "./logHierarchy";

export const LogDisplay = ({ statusData }) => {
  const jsonLogUrl = statusData?.jsonLogUrl;
  const [logData, setLogData] = useState(null);

  useEffect(() => {
    (async () => {
      if (jsonLogUrl) {
        const response = await fetch(jsonLogUrl);
        if (response.ok) {
          const data = await response.json();
          setLogData(createLogHierarchy(data));
        }
      }
    })();
  }, [jsonLogUrl]);

  return (
    logData &&
    logData.length > 0 && (
      <Card className="log-card">
        <Card.Body>
          {logData.map((log) => (
            <LogDisplayEntry key={log.message} {...log} />
          ))}
        </Card.Body>
      </Card>
    )
  );
};

export default LogDisplay;

import { useEffect, useState } from "react";
import { Card } from "react-bootstrap";
import LogDisplayEntry from "./logDisplayEntry";
import { createLogHierarchy } from "./logHierarchy";

/**
 * Displays the log entries of a validation in a hierarchical structure.
 */
export const LogDisplay = ({ statusData }) => {
  const jsonLogUrl = statusData?.jsonLogUrl;
  const [logData, setLogData] = useState(null);

  // Fetch validator log and convert it to a hierarchical structure
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
          {logData.map((logEntry) => (
            <LogDisplayEntry key={logEntry.message} {...logEntry} />
          ))}
        </Card.Body>
      </Card>
    )
  );
};

export default LogDisplay;

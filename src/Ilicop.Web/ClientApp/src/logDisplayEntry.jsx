import { useState } from "react";
import { Collapse } from "react-bootstrap";
import { BsCheckLg, BsChevronRight, BsExclamationTriangleFill, BsXLg } from "react-icons/bs";

/**
 * Displays a log entry with (optional) collapsable child log entries.
 */
export const LogDisplayEntry = ({ message, type, values }) => {
  const [showChildren, setShowChildren] = useState(false);

  function getIcon() {
    switch (type) {
      case "Error":
        return <BsXLg style={{ color: "red" }} />;
      case "Warning":
        return <BsExclamationTriangleFill style={{ color: "orange" }} />;
      default:
        return <BsCheckLg style={{ color: "green" }} />;
    }
  }

  return (
    <>
      <div onClick={() => setShowChildren(!showChildren)} className={"log-entry" + (values ? " expandable" : "")}>
        <span className={"icon chevron" + (showChildren ? " open" : "")}>{values && <BsChevronRight />}</span>
        <span className="icon">{getIcon()}</span>
        <span className="title">{message}</span>
      </div>
      {values && (
        <Collapse in={showChildren}>
          <div className="log-entry-container">
            {values.map((logEntry) => (
              <LogDisplayEntry key={logEntry.message} {...logEntry} />
            ))}
          </div>
        </Collapse>
      )}
    </>
  );
};

export default LogDisplayEntry;

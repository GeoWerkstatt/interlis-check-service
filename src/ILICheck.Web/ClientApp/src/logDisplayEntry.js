import { useState } from "react";
import { Collapse } from "react-bootstrap";
import { BsCheckLg, BsChevronRight, BsExclamationTriangleFill, BsXLg } from "react-icons/bs";

export const LogDisplayEntry = ({ message, type, values }) => {
  const [open, setOpen] = useState(false);

  function getIcon() {
    switch (type) {
      case "Warning":
        return <BsExclamationTriangleFill style={{ color: "orange" }} />;
      case "Error":
        return <BsXLg style={{ color: "red" }} />;
      default:
        return <BsCheckLg style={{ color: "green" }} />;
    }
  }

  return (
    <>
      <div onClick={() => setOpen(!open)} className={"log-entry" + (values ? " expandable" : "")}>
        <span className={"icon chevron" + (open ? " open" : "")}>{values && <BsChevronRight />}</span>
        <span className="icon">{getIcon()}</span>
        <span className="title">{message}</span>
      </div>
      {values && (
        <Collapse in={open}>
          <div className="log-entry-container">
            {values.map((value) => (
              <LogDisplayEntry key={value.message} {...value} />
            ))}
          </div>
        </Collapse>
      )}
    </>
  );
};

export default LogDisplayEntry;

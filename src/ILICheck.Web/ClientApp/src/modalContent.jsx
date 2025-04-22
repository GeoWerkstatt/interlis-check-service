import ReactMarkdown from "react-markdown";
import { Modal, Button } from "react-bootstrap";

export const ModalContent = (props) => {
  const { content, type } = props;

  return (
    <Modal {...props} size="lg" aria-labelledby="contained-modal-title-vcenter" centered>
      <div style={{ maxHeight: "calc(100vh - 58px)", display: "flex", flexDirection: "column" }}>
        <Modal.Body style={{ flex: 1, overflowY: "auto" }}>
          {type === "markdown" && <ReactMarkdown linkTarget="_blank" children={content || ""} />}
          {type === "raw" && content}
        </Modal.Body>
        <Modal.Footer style={{ flex: "0 0 auto" }}>
          <Button variant="outline-dark" onClick={props.onHide}>
            Schliessen
          </Button>
        </Modal.Footer>
      </div>
    </Modal>
  );
};

export default ModalContent;

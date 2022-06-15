import ReactMarkdown from "react-markdown";
import { Modal, Button } from "react-bootstrap";
import "./app.css";

export const ModalContent = (props) => {
  const { content, type } = props;

  return (
    <Modal {...props} size="lg" aria-labelledby="contained-modal-title-vcenter" centered>
      <Modal.Body>
        {type === "markdown" && <ReactMarkdown linkTarget="_blank" children={content || ""} />}
        {type === "raw" && content}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="outline-dark" onClick={props.onHide}>
          Schliessen
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export default ModalContent;

import { Modal, Button } from 'react-bootstrap';
import ReactMarkdown from "react-markdown";

export const ModalContent = props => {
  const { content } = props;

  return (
    <Modal {...props} size="lg" aria-labelledby="contained-modal-title-vcenter" centered>
      <Modal.Body>
        <ReactMarkdown children={content || ''} />
      </Modal.Body>
      <Modal.Footer>
        <Button variant="outline-dark" onClick={props.onHide}>Schliessen</Button>
      </Modal.Footer>
    </Modal>
  );
}

export default ModalContent;

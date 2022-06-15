import ReactMarkdown from "react-markdown";
import { Alert } from "react-bootstrap";
import { IoClose } from "react-icons/io5";
import "./app.css";

export const BannerContent = (props) => {
  const { content } = props;

  return (
    <Alert className="banner" variant="primary">
      <ReactMarkdown linkTarget="_blank" children={content || ""} />
      <span className="close-icon">
        <IoClose onClick={props.onHide} />
      </span>
    </Alert>
  );
};

export default BannerContent;

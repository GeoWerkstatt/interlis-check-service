import "./app.css";
import React from "react";
import { Carousel } from "react-bootstrap";

export const InfoCarousel = (props) => {
  const { content } = props;

  return (
    <Carousel interval={null} nextLabel="" prevLabel="">
      {content?.split("\n").map((item) => (
        <Carousel.Item key={item}>
          <div>{item}</div>
        </Carousel.Item>
      ))}
    </Carousel>
  );
};

export default InfoCarousel;

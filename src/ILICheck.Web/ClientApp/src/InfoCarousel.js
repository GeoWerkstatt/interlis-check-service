import './App.css';
import './CI_geow.css';
import React from 'react';
import { Carousel } from 'react-bootstrap';

export const InfoCarousel = () => {
  return (
    <Carousel interval={null} wrap="false" nextLabel="" prevLabel="" prevIcon="">
      <Carousel.Item>
        <p>Der Ilicop überprüft deine XTF-Dateien für dich.</p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Er nimmt es ganz genau und lässt nur passieren, was INTERLIS-konform ist.</p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Er kennt sich aus und findet das passende INTERLIS-Modell zu deinem Datensatz automatisch.</p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Statt dir eine Busse auszustellen, schreibt er dir ein genaues Feedback zu den Fehlern. </p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Das Feedback kannst du als Textdatei oder XTFLog-Datei herunterladen. </p>
      </Carousel.Item>
      <Carousel.Item>
        <p>So kannst du beim nächsten Mal konforme Daten hochladen und den Ilicop zufrieden stellen. </p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Bei grossen Dateien braucht er seine Zeit, er prüft schliesslich sehr gewissenhaft.</p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Auch der <a href="https://www.Datenschutz.ch" title="Datenschutzbestimmungen" target="_blank" rel="noreferrer">
          Datenschutz
        </a> ist ihm sehr wichtig, bitte übergebe ihm keine vertraulichen Daten.</p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Am liebsten arbeitet er mit seinem Kollgen dem <a href="https://plugins.qgis.org/plugins/xtflog_checker/" title="QGIS Plugin" target="_blank" rel="noreferrer">
          QGIS XTFLog Checker
        </a> zusammen.</p>
      </Carousel.Item>
      <Carousel.Item>
        <p>Alles klar? Der Ilicop nimmt jetzt gern dein (gezipptes) XTF-File entgegen!</p>
      </Carousel.Item>
    </Carousel>
  );
}

export default InfoCarousel;

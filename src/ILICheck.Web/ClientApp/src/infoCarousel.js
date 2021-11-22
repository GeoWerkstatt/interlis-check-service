import './app.css';
import './custom.css';
import React from 'react';
import { Carousel } from 'react-bootstrap';

export const InfoCarousel = () => {
  return (
    <Carousel interval={null} wrap="false" nextLabel="" prevLabel="" prevIcon="">
      <Carousel.Item>
        <div>Der INTERLIS Web-Check-Service überprüft deine XTF-Dateien für dich.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Er nimmt es ganz genau und lässt nur passieren, was INTERLIS-konform ist.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Er kennt sich aus und findet das passende INTERLIS-Modell zu deinem Datensatz automatisch.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Statt dir eine Busse auszustellen, schreibt er dir ein genaues Feedback zu den Fehlern.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Das Feedback kannst du als Textdatei oder XTFLog-Datei herunterladen.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>So kannst du beim nächsten Mal konforme Daten hochladen und den INTERLIS Web-Check-Service zufrieden stellen.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Bei grossen Dateien braucht er seine Zeit, er prüft schliesslich sehr gewissenhaft.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Auch der <a href="https://www.Datenschutz.ch" title="Datenschutzbestimmungen" target="_blank" rel="noreferrer">
          Datenschutz
        </a> ist ihm sehr wichtig, bitte übergebe ihm keine vertraulichen Daten.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Am liebsten arbeitet er mit seinem Kollgen dem <a href="https://plugins.qgis.org/plugins/xtflog_checker/" title="QGIS Plugin" target="_blank" rel="noreferrer">
          QGIS XTFLog Checker
        </a> zusammen.</div>
      </Carousel.Item>
      <Carousel.Item>
        <div>Alles klar? Der INTERLIS Web-Check-Service nimmt jetzt gern dein (gezipptes) XTF-File entgegen!</div>
      </Carousel.Item>
    </Carousel>
  );
}

export default InfoCarousel;

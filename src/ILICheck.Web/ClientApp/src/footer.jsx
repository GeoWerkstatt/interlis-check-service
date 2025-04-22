import "./app.css";
import React from "react";
import About from "./about";
import { Button } from "react-bootstrap";

export const Footer = (props) => {
  const {
    openModalContent,
    infoHilfeContent,
    nutzungsbestimmungenContent,
    datenschutzContent,
    impressumContent,
    clientSettings,
    licenseInfoCustom,
    licenseInfo,
  } = props;

  return (
    <footer className="footer-style">
      {infoHilfeContent && (
        <Button
          variant="link"
          className="footer-button no-outline-on-focus"
          onClick={() => openModalContent(infoHilfeContent, "markdown")}
        >
          INFO & HILFE
        </Button>
      )}
      {nutzungsbestimmungenContent && (
        <Button
          variant="link"
          className="footer-button no-outline-on-focus"
          onClick={() => openModalContent(nutzungsbestimmungenContent, "markdown")}
        >
          NUTZUNGSBESTIMMUNGEN
        </Button>
      )}
      {datenschutzContent && (
        <Button
          variant="link"
          className="footer-button no-outline-on-focus"
          onClick={() => openModalContent(datenschutzContent, "markdown")}
        >
          DATENSCHUTZ
        </Button>
      )}
      {impressumContent && (
        <Button
          variant="link"
          className="footer-button no-outline-on-focus"
          onClick={() => openModalContent(impressumContent, "markdown")}
        >
          IMPRESSUM
        </Button>
      )}
      <Button
        variant="link"
        className="footer-button no-outline-on-focus"
        onClick={() =>
          openModalContent(
            <About clientSettings={clientSettings} licenseInfo={{ ...licenseInfoCustom, ...licenseInfo }} />,
            "raw"
          )
        }
      >
        ABOUT
      </Button>
    </footer>
  );
};

export default Footer;

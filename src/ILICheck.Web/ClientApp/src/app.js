import React, { useState, useEffect } from "react";
import BannerContent from "./bannerContent";
import Home from "./home";
import ModalContent from "./modalContent";
import Footer from "./footer";
import Header from "./header";
import "./app.css";

export const App = () => {
  const [modalContent, setModalContent] = useState(false);
  const [modalContentType, setModalContentType] = useState(null);
  const [showModalContent, setShowModalContent] = useState(false);
  const [showBannerContent, setShowBannerContent] = useState(false);
  const [clientSettings, setClientSettings] = useState(null);
  const [datenschutzContent, setDatenschutzContent] = useState(null);
  const [impressumContent, setImpressumContent] = useState(null);
  const [infoHilfeContent, setInfoHilfeContent] = useState(null);
  const [bannerContent, setBannerContent] = useState(null);
  const [nutzungsbestimmungenContent, setNutzungsbestimmungenContent] = useState(null);
  const [quickStartContent, setQuickStartContent] = useState(null);
  const [licenseInfo, setLicenseInfo] = useState(null);
  const [licenseInfoCustom, setLicenseInfoCustom] = useState(null);

  // Update HTML title property
  useEffect(() => (document.title = clientSettings?.applicationName), [clientSettings]);

  // Fetch client settings
  useEffect(() => {
    fetch("api/v1/settings")
      .then((res) => res.headers.get("content-type")?.includes("application/json") && res.json())
      .then((json) => setClientSettings(json));
  }, []);

  // Fetch optional custom content
  useEffect(() => {
    fetch("impressum.md")
      .then((res) => res.headers.get("content-type")?.includes("ext/markdown") && res.text())
      .then((text) => setImpressumContent(text));

    fetch("datenschutz.md")
      .then((res) => res.headers.get("content-type")?.includes("ext/markdown") && res.text())
      .then((text) => setDatenschutzContent(text));

    fetch("info-hilfe.md")
      .then((res) => res.headers.get("content-type")?.includes("ext/markdown") && res.text())
      .then((text) => setInfoHilfeContent(text));

    fetch("nutzungsbestimmungen.md")
      .then((res) => res.headers.get("content-type")?.includes("ext/markdown") && res.text())
      .then((text) => setNutzungsbestimmungenContent(text));

    fetch("banner.md")
      .then((res) => res.headers.get("content-type")?.includes("ext/markdown") && res.text())
      .then((text) => setBannerContent(text));

    fetch("quickstart.txt")
      .then((res) => res.headers.get("content-type")?.includes("text/plain") && res.text())
      .then((text) => setQuickStartContent(text));

    fetch("license.json")
      .then((res) => res.headers.get("content-type")?.includes("application/json") && res.json())
      .then((json) => setLicenseInfo(json));

    fetch("license.custom.json")
      .then((res) => res.headers.get("content-type")?.includes("application/json") && res.json())
      .then((json) => setLicenseInfoCustom(json));
  }, []);

  const openModalContent = (content, type) =>
    setModalContent(content) & setModalContentType(type) & setShowModalContent(true);

  return (
    <div className="app">
      <Header clientSettings={clientSettings}></Header>
      <Home
        clientSettings={clientSettings}
        nutzungsbestimmungenAvailable={nutzungsbestimmungenContent ? true : false}
        showNutzungsbestimmungen={() => openModalContent(nutzungsbestimmungenContent, "markdown")}
        quickStartContent={quickStartContent}
        setShowBannerContent={setShowBannerContent}
      />
      <Footer
        openModalContent={openModalContent}
        infoHilfeContent={infoHilfeContent}
        nutzungsbestimmungenContent={nutzungsbestimmungenContent}
        datenschutzContent={datenschutzContent}
        impressumContent={impressumContent}
        clientSettings={clientSettings}
        licenseInfoCustom={licenseInfoCustom}
        licenseInfo={licenseInfo}
      ></Footer>
      <ModalContent
        className="modal"
        show={showModalContent}
        content={modalContent}
        type={modalContentType}
        onHide={() => setShowModalContent(false)}
      />
      {bannerContent && showBannerContent && (
        <BannerContent className="banner" content={bannerContent} onHide={() => setShowBannerContent(false)} />
      )}
    </div>
  );
};

export default App;

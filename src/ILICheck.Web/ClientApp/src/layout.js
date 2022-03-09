import './app.css';
import React, { useState, useEffect } from 'react';
import swissMadeSwissHosted from './img/sms-sh.png';
import qgisLogo from './img/qgis.png';
import interlisLogo from './img/interlis.svg'
import Home from './home';
import ModalContent from './modalContent';
import BannerContent from './bannerContent';
import { Button } from 'react-bootstrap';

export const Layout = props => {
  const { connection, closedConnectionId, log, updateLog, resetLog, setUploadLogsInterval, validationResult, setValidationResult, setUploadLogsEnabled } = props;
  const [modalContent, setModalContent] = useState(false);
  const [showModalContent, setShowModalContent] = useState(false);
  const [showBannerContent, setShowBannerContent] = useState(false);
  const [clientSettings, setClientSettings] = useState(null);
  const [datenschutzContent, setDatenschutzContent] = useState(null);
  const [impressumContent, setImpressumContent] = useState(null);
  const [infoHilfeContent, setInfoHilfeContent] = useState(null);
  const [bannerContent, setBannerContent] = useState(null);
  const [nutzungsbestimmungenContent, setNutzungsbestimmungenContent] = useState(null);
  const [quickStartContent, setQuickStartContent] = useState(null);

  // Update HTML title property
  useEffect(() => document.title = clientSettings?.applicationName, [clientSettings])

  // Fetch client settings
  useEffect(() => {
    fetch('api/settings')
      .then(res => res.headers.get('content-type')?.includes('application/json') && res.json())
      .then(json => setClientSettings(json));
  }, []);

  // Fetch optional custom content
  useEffect(() => {
    fetch('impressum.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setImpressumContent(text));

    fetch('datenschutz.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setDatenschutzContent(text));

    fetch('info-hilfe.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setInfoHilfeContent(text));

    fetch('nutzungsbestimmungen.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setNutzungsbestimmungenContent(text));

      fetch('banner.md')
      .then(res => res.headers.get('content-type')?.includes('ext/markdown') && res.text())
      .then(text => setBannerContent(text));

    fetch('quickstart.txt')
      .then(res => res.headers.get('content-type')?.includes('text/plain') && res.text())
      .then(text => setQuickStartContent(text));
  }, []);

  const openModalContent = content => setModalContent(content) & setShowModalContent(true);

  return (
    <div className="app">
      <div className="version-info">
        <p>{clientSettings?.applicationName}{process.env.REACT_APP_VERSION ? 'v' + process.env.REACT_APP_VERSION + '+' : ''}{process.env.REACT_APP_REVISION ?? process.env.NODE_ENV}</p>
        <p>ilivalidator{clientSettings?.ilivalidatorVersion}</p>
        <p>ili2gpkg{clientSettings?.ili2gpkgVersion}</p>
      </div>
      <header>
        <a href={clientSettings?.vendorLink} target="_blank" rel="noreferrer">
          <img className="vendor-logo" src="/vendor.png" alt="Vendor Logo" onError={(e) => { e.target.style.display = 'none' }} />
        </a>
      </header>
      <main>
        <Home connection={connection}
          closedConnectionId={closedConnectionId}
          validationResult={validationResult}
          setValidationResult={setValidationResult}
          clientSettings={clientSettings}
          nutzungsbestimmungenAvailable={nutzungsbestimmungenContent ? true : false}
          showNutzungsbestimmungen={() => openModalContent(nutzungsbestimmungenContent)}
          quickStartContent={quickStartContent}
          log={log}
          updateLog={updateLog}
          resetLog={resetLog}
          setUploadLogsInterval={setUploadLogsInterval}
          setUploadLogsEnabled={setUploadLogsEnabled}
          setShowBannerContent={setShowBannerContent}/>
      </main>
      <footer className="footer-style">
        <div className='footer-links'>
          {infoHilfeContent && <Button variant="link" className="footer-button" onClick={() => openModalContent(infoHilfeContent)}>
            INFO & HILFE
          </Button>}
          {nutzungsbestimmungenContent && <Button variant="link" className="footer-button no-outline-on-focus" onClick={() => openModalContent(nutzungsbestimmungenContent)}>
            NUTZUNGSBESTIMMUNGEN
          </Button>}
          {datenschutzContent && <Button variant="link" className="footer-button no-outline-on-focus" onClick={() => openModalContent(datenschutzContent)}>
            DATENSCHUTZ
          </Button>}
          {impressumContent && <Button variant="link" className="footer-button" onClick={() => openModalContent(impressumContent)}>
            IMPRESSUM
          </Button>}
        </div>
        <div className='footer-icons'>
          <a href="https://interlis.ch/" title="Link zu interlis" target="_blank" rel="noreferrer">
            <img className="footer-icon" src={interlisLogo} alt="Interlis Logo" />
          </a>
          <a href="https://plugins.qgis.org/plugins/xtflog_checker/" title="Link zum QGIS Plugin XTFLog Checker" target="_blank" rel="noreferrer">
            <img className="footer-icon" src={qgisLogo} alt="QGIS Logo" />
          </a>
          <a href="https://www.swissmadesoftware.org/en/home/swiss-hosting.html" title="Link zu Swiss Hosting" target="_blank" rel="noreferrer">
            <img className="footer-icon" src={swissMadeSwissHosted} alt="Swiss Hosting Logo" />
          </a>
        </div>
      </footer>
      < ModalContent
        className="modal"
        show={showModalContent}
        content={modalContent}
        onHide={() => setShowModalContent(false)}
      />
      {bannerContent && showBannerContent && <BannerContent
        className="banner"
        content={bannerContent}
        onHide={() => setShowBannerContent(false)}
      />}
    </div>
  );
}

export default Layout;

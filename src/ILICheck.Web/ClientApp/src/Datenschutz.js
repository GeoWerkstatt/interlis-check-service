import { Modal, Button } from 'react-bootstrap';
export const DatenschutzModal = props => {
  return (
    <Modal
      {...props}
      size="lg"
      aria-labelledby="contained-modal-title-vcenter"
      centered
    >
      <Modal.Header>
        <Modal.Title id="contained-modal-title-vcenter">
          Datenschutzerklärung
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <p>
          <p>
            Verantwortliche Stelle im Sinne der Datenschutzgesetze, insbesondere der EU-Datenschutzgrundverordnung (DSGVO), ist:
          </p>
          <p>
            GeoWerkstatt GmbH
            Bleichemattstrasse 2
            CH-5000 Aarau
            Schweiz
          </p>
          <p>
            Stefan Kiener, Geschäftsführer GeoWerkstatt
            Telefon: +41 (0)62 832 63 81
            office@geowerkstatt.ch
            http://www.geowerkstatt.ch/
          </p>
          <h4>Allgemeiner Hinweis</h4>
          <p>
            Gestützt auf Artikel 13 der schweizerischen Bundesverfassung und die datenschutzrechtlichen Bestimmungen des Bundes (Datenschutzgesetz, DSG) hat jede Person Anspruch auf Schutz ihrer Privatsphäre sowie auf Schutz vor Missbrauch ihrer persönlichen Daten. Die Betreiber dieser Seiten nehmen den Schutz Ihrer persönlichen Daten sehr ernst. Wir behandeln Ihre personenbezogenen Daten vertraulich und entsprechend der gesetzlichen Datenschutzvorschriften sowie dieser Datenschutzerklärung.
            In Zusammenarbeit mit unseren Hosting-Providern bemühen wir uns, die Datenbanken so gut wie möglich vor fremden Zugriffen, Verlusten, Missbrauch oder vor Fälschung zu schützen.
            Wir weisen darauf hin, dass die Datenübertragung im Internet (z.B. bei der Kommunikation per E-Mail) Sicherheitslücken aufweisen kann. Ein lückenloser Schutz der Daten vor dem Zugriff durch Dritte ist nicht möglich.Durch die Nutzung dieser Website erklären Sie sich mit der Erhebung, Verarbeitung und Nutzung von Daten gemäss der nachfolgenden Beschreibung einverstanden. Diese Website kann grundsätzlich ohne Registrierung besucht werden. Dabei werden Daten wie beispielsweise aufgerufene Seiten bzw. Namen der abgerufenen Datei, Datum und Uhrzeit zu statistischen Zwecken auf dem Server gespeichert, ohne dass diese Daten unmittelbar auf Ihre Person bezogen werden. Personenbezogene Daten, insbesondere Name, Adresse oder E-Mail-Adresse werden soweit möglich auf freiwilliger Basis erhoben. Ohne Ihre Einwilligung erfolgt keine Weitergabe der Daten an Dritte.
          </p>
          <h4>Datenschutzerklärung für Uploads</h4>
          <p>
            ... zu ergänzen.

            Hochgeladene files und geschriebene Logfiles werden für maximal 24h gespeichert und anschliessend automatisch gelöscht. Der INTERLIS Web-Check-Service darf ausschliesslich mit nicht zugangsbeschränkten Daten genutzt werden.
            Nur Geobasisdaten der Zugangsberechtigungsstufe A gemäss <span><a style={{ color: 'black' }} href="https://fedlex.data.admin.ch/filestore/fedlex.data.admin.ch/eli/cc/2008/389/20190601/de/pdf-a/fedlex-data-admin-ch-eli-cc-2008-389-20190601-de-pdf-a.pdf">GEOIV</a></span> dürfen hochgeladen werden
          </p>
          <h4>Datenschutzerklärung für Cookies</h4>
          <p>
            ... zu ergänzen.
          </p>
          <h4>Datenschutzerklärung für SSL-/TLS-Verschlüsselung</h4>
          <p>
            Diese Website nutzt aus Gründen der Sicherheit und zum Schutz der Übertragung vertraulicher Inhalte, wie zum Beispiel der Anfragen, die Sie an uns als Seitenbetreiber senden, eine SSL-/TLS-Verschlüsselung. Eine verschlüsselte Verbindung erkennen Sie daran, dass die Adresszeile des Browsers von "http://" auf "https://" wechselt und an dem Schloss-Symbol in Ihrer Browserzeile.
            Wenn die SSL bzw. TLS Verschlüsselung aktiviert ist, können die Daten, die Sie an uns übermitteln, nicht von Dritten mitgelesen werden.
          </p>
          <h4>Datenschutzerklärung für die Nutzung von Google Web Fonts</h4>
          <p>
            Diese Website nutzt zur einheitlichen Darstellung von Schriftarten so genannte Web Fonts, die von Google bereitgestellt werden. Beim Aufruf einer Seite lädt Ihr Browser die benötigten Web Fonts in ihren Browsercache, um Texte und Schriftarten korrekt anzuzeigen. Wenn Ihr Browser Web Fonts nicht unterstützt, wird eine Standardschrift von Ihrem Computer genutzt.
            Weitere Informationen zu Google Web Fonts finden Sie unter https://developers.google.com/fonts/faq und in der Datenschutzerklärung von Google: https://www.google.com/policies/privacy/
          </p>
          <h4>Urheberrechte</h4>
          <p>
            Die Urheber- und alle anderen Rechte an Inhalten, Bildern, Fotos oder anderen Dateien auf der Website, gehören ausschliesslich dem Betreiber dieser Website oder den speziell genannten Rechteinhabern. Für die Reproduktion von sämtlichen Dateien, ist die schriftliche Zustimmung des Urheberrechtsträgers im Voraus einzuholen.
            Wer ohne Einwilligung des jeweiligen Rechteinhabers eine Urheberrechtsverletzung begeht, kann sich strafbar und allenfalls schadenersatzpflichtig machen.
          </p>
          <h4>Allgemeiner Haftungsausschluss</h4>
          <p>
            Alle Angaben unseres Internetangebotes wurden sorgfältig geprüft. Wir bemühen uns, unser Informationsangebot aktuell, inhaltlich richtig und vollständig anzubieten. Trotzdem kann das Auftreten von Fehlern nicht völlig ausgeschlossen werden, womit wir keine Garantie für Vollständigkeit, Richtigkeit und Aktualität von Informationen auch journalistisch-redaktioneller Art übernehmen können. Haftungsansprüche aus Schäden materieller oder ideeller Art, die durch die Nutzung der angebotenen Informationen verursacht wurden, sind ausgeschlossen, sofern kein nachweislich vorsätzliches oder grob fahrlässiges Verschulden vorliegt.
            Der Herausgeber kann nach eigenem Ermessen und ohne Ankündigung Texte verändern oder löschen und ist nicht verpflichtet, Inhalte dieser Website zu aktualisieren. Die Benutzung bzw. der Zugang zu dieser Website geschieht auf eigene Gefahr des Besuchers. Der Herausgeber, seine Auftraggeber oder Partner sind nicht verantwortlich für Schäden, wie direkte, indirekte, zufällige, vorab konkret zu bestimmende oder Folgeschäden, die angeblich durch den Besuch dieser Website entstanden sind und übernehmen hierfür folglich keine Haftung.
            Der Herausgeber übernimmt ebenfalls keine Verantwortung und Haftung für die Inhalte und die Verfügbarkeit von Webseiten Dritter, die über externe Links dieser Webseite erreichbar sind. Für den Inhalt der verlinkten Seiten sind ausschliesslich deren Betreiber verantwortlich. Der Herausgeber distanziert sich damit ausdrücklich von allen Inhalten Dritter, die möglicherweise straf- oder haftungsrechtlich relevant sind oder gegen die guten Sitten verstossen.
          </p>
          <h4>Änderungen</h4>
          <p>
            Wir können diese Datenschutzerklärung jederzeit ohne Vorankündigung anpassen. Es gilt die jeweils aktuelle, auf unserer Website publizierte Fassung. Soweit die Datenschutzerklärung Teil einer Vereinbarung mit Ihnen ist, werden wir Sie im Falle einer Aktualisierung über die Änderung per E-Mail oder auf andere geeignete Weise informieren.
          </p>
          <h4>Fragen an den Datenschutzbeauftragten</h4>
          <p>
            Wenn Sie Fragen zum Datenschutz haben, schreiben Sie uns bitte eine E-Mail oder wenden Sie sich direkt an die für den Datenschutz zu Beginn der Datenschutzerklärung aufgeführten, verantwortlichen Person in unserer Organisation.                    </p>
          <p>
            Aarau, 21.01.2020 Quelle: SwissAnwalt
          </p>
        </p>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="outline-dark" onClick={props.onHide}>Schliessen</Button>
      </Modal.Footer>
    </Modal>
  );
}

export default DatenschutzModal;

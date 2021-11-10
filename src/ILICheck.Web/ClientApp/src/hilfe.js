import { Modal, Button } from 'react-bootstrap';
export const HilfeModal = props => {
    return (
      <Modal
        {...props}
        size="lg"
        aria-labelledby="contained-modal-title-vcenter"
        centered
      >
        <Modal.Header>
          <Modal.Title id="contained-modal-title-vcenter">
            Informationen zum INTERLIS Web-Check-Service
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
        <h4>Anleitung & Funktionsweise</h4>
          <p>
            Der INTERLIS Web-Check-Service basiert auf dem <a href="https://www.interlis.ch/downloads/ilivalidator" title="Zum ilivalidator" target="_blank" rel="noreferrer">Ilivalidator</a> von Eisenhut informatik.
            Bei updates des Ilivaliators greift auch der INTERLIS Web-Check-Service automatisch auf die jeweils aktuellste Version zu.
            Dateien, die mit dem INTERLIS Web-Check-Service überprüft werden, werden auf einen Server der GeoWerstatt Gmbh (...mehr Details) hochgeladen und dort für den Validierungsprozess zwischengespeichert.
            Nur Dateien des Typs .xtf und .zip werden akzeptiert. Enthält ein .zip file kein .xtf file oder mehr als eine Datei wird der Prozess abgebrochen.
            </p>
            <p>
            Sobald eine .xtf Datei hochgeladen wurde wird deren Stuktur validiert. Diese muss einer gültigen xml Defionition entsprechen, bei fehlenden Tag oder ählichem wird der Prozess abgebochen und Feedback
            in die Konsole geschrieben. Wurde eine stukturell gültige xml Datei hochgeladen wird diese vom Server an den ilivalidator weitergegeben. Der Server nimmt die INTERLIS Validierung vor und schreibt Logdateien.
            Einerseits schreibt er ein klassisches Logfile im Textformat und andererseits ein XTFLog file, welches der INTERLIS-Klasse IliVErrors entspricht. Das XTFLog file enthält auch räumliche Informationen zu den Fehlern
            und kann beispielsweise in einem GIS Programm visualisiert werden.
            </p>
            <p>
            Für die einfache Visualisierung in QGIS steht das von GeoWerkstatt Gmbh entwickelte QGIS Plugin
            <a href="https://plugins.qgis.org/plugins/xtflog_checker/" title="QGIS Plugin" target="_blank" rel="noreferrer"> XTFLog-Checker </a> zur Verfügung. Damit kann das XTFLog file einfach in QGIS geladen uns visualisiert werden.
            Eine Checkliste vereinfacht das "Abarbeiten" von Fehlern. Im INTERLIS Web-Check-Service steht auch die Möglichkeit "Link in Zwischenablage kopieren" zur Verfügung. Dieser Link kann kopiert werden und direkt im QGIS Plugin angegeben
            werden. Das XTFLog-Checker Plugin kümmert sich dann um das herunterladen der XTFLog Datei vom Server und visualisiert die Fehler unmittelbar.
            </p>
            <p>
            Ist der Validierungsprozess abgeschlossen wird die hochgeladene Datei unmittelbar gelöscht. Die Logdateien werden für maximal 24h gespeichert um einen Zugriff über den Link zu gewährleistern.
          </p>
          <h4>Entwicklung und Bugtracking</h4>
          <p>
            Der INTERLIS Web-Check-Service wurde von der GeoWerkstatt Gmbh in Zusammenarbeit mit der GIS Fachstelle des Kanton Solothurn als Open Source Projekt entwickelt. Der Code steht unter der Linzent (.... zu ERgänzen) im
            <a href="https://github.com/GeoWerkstatt/interlis-check-service" title="Link zum github reporsitory" target="_blank" rel="noreferrer"> GitHub Repositoy </a>
           zur Verfügung. Falls Ihnen Bugs begegnen, können Sie dort ein <a href="https://github.com/GeoWerkstatt/interlis-check-service/issues" title="Link zum github reporsitory" target="_blank" rel="noreferrer"> Issue </a> eröffnen.
          </p>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-dark" onClick={props.onHide}>Schliessen</Button>
        </Modal.Footer>
      </Modal>
    );
  }

  export default HilfeModal;



import { Modal, Button } from 'react-bootstrap';
export const ImpressumModal = props => {
    return (
        <Modal
            {...props}
            size="lg"
            aria-labelledby="contained-modal-title-vcenter"
            centered
        >
            <Modal.Header>
                <Modal.Title id="contained-modal-title-vcenter">
                    IMPRESSUM
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <p>
                    <h4>Kontaktadresse</h4>
                    <p>
                        GeoWerkstatt GmbH
                        Bleichemattstrasse 2
                        CH-5000 Aarau
                        Schweiz
                    </p>
                    <h4>Vertretungsberechtigte Person</h4>
                    <p>
                        Stefan Kiener, Geschäftsführer GeoWerkstatt
                        Telefon: +41 (0)62 832 63 81
                        office@geowerkstatt.ch
                    </p>
                    <h4>Handelsregister-Eintrag</h4>
                    <p>
                        Eingetragener Firmenname: GEOWERKSTATT GmbH Handelsregister Nr: CHE-111.988.475
                    </p>
                    <h4>Mehrwertsteuer-Nummer</h4>
                    <p>
                        CHE-111.988.475
                        Mit der Nutzung dieser Webseite erklären Sie sich mit den nachfolgenden Bedingungen einverstanden.
                    </p>
                    <h4>Haftungsausschluss</h4>
                    <p>
                        Der Autor übernimmt keinerlei Gewähr hinsichtlich der inhaltlichen Richtigkeit, Genauigkeit, Aktualität, Zuverlässigkeit und Vollständigkeit der Informationen. Haftungsansprüche gegen den Autor wegen Schäden materieller oder immaterieller Art, welche aus dem Zugriff oder der Nutzung bzw. Nichtnutzung der veröffentlichten Informationen, durch Missbrauch der Verbindung oder durch technische Störungen entstanden sind, werden ausgeschlossen. Alle Angebote sind unverbindlich. Der Autor behält es sich ausdrücklich vor, Teile der Seiten oder das gesamte Angebot ohne besondere Ankündigung zu verändern, zu ergänzen, zu löschen oder die Veröffentlichung zeitweise oder endgültig einzustellen.
                    </p>
                    <h4>Haftungsausschluss für Links</h4>
                    <p>
                        Verweise und Links auf Webseiten Dritter liegen ausserhalb unseres Verantwortungsbereichs. Es wird jegliche Verantwortung für solche Webseiten abgelehnt. Der Zugriff und die Nutzung solcher Webseiten erfolgen auf eigene Gefahr des jeweiligen Nutzers.
                    </p>
                    <h4>Urheberrechte</h4>
                    <p>
                        Die Urheber- und alle anderen Rechte an Inhalten, Bildern, Fotos oder anderen Dateien auf dieser Website, gehören ausschliesslich der Firma GEOWERKSTATT GmbH oder den speziell genannten Rechteinhabern. Für die Reproduktion jeglicher Elemente ist die schriftliche Zustimmung des Urheberrechtsträgers im Voraus einzuholen.
                    </p>
                    <h4>Datenschutz</h4>
                    <p>
                        Die Firma GeoWerkstatt GmbH nimmt den Schutz Ihrer persönlichen Daten sehr ernst. Wir behandeln Ihre personenbezogenen Daten vertraulich und entsprechend der gesetzlichen Datenschutzvorschriften. Weitere Informationen zum Datenschutz entnehmen Sie unserer Datenschutzerklärung.
                    </p>
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

export default ImpressumModal;
# INTERLIS Web-Check-Service (ILICHECK) <!-- omit in toc -->

[![CI](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/ci.yml/badge.svg)](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/ci.yml)
[![Release](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/release.yml/badge.svg)](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/release.yml)

Webbasierter Checkservice für INTERLIS Datenabgaben

![INTERLIS Web-Check-Service](./assets/ilicheck_app_screenshot.png)

## Inhaltsverzeichnis <!-- omit in toc -->

- [Quick Start](#quick-start)
- [Individuelle Anpassung](#individuelle-anpassung)
  - [ilivalidator](#ilivalidator)
  - [Web-Check-Service](#web-check-service)
- [REST API](#rest-api)
- [Health Check API](#health-check-api)
- [Einrichten der Entwicklungsumgebung](#einrichten-der-entwicklungsumgebung)
- [Neue Version erstellen](#neue-version-erstellen)
- [Lizenz](#lizenz)

## Quick Start

Mit [Docker](https://www.docker.com/) kann der *INTERLIS Web-Check-Service* in einer isolierten Umgebung mit Docker Containern betrieben werden. Eine Beispiel-Konfiguration (`docker-compose.yml`) befindet sich im nächsten Abschnitt. Mit `docker-compose up` wird die Umgebung hochgefahren.

Um einen ersten Augenschein der Applikation zu nehmen, kann der Container in der Kommandozeile wie folgt gestartet werden:

```bash
docker run -it --rm -p 8080:80 ghcr.io/geowerkstatt/interlis-check-service:latest
```

`docker-compose.yml`

```yaml
version: '3'
services:
  web:
    # Docker image (NAME:TAG)
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:v3
    #     Stable tag for a specific major version
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:v3.2.1
    #     Stable tag for a specific version
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:v3-beta
    #     Reflects the last commit on branches matching 'releases/beta/**'
    #     May be used to preview upcoming releases
    #     Contains breaking changes and incompatibilities
    #     NOT RECOMMENDED!
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:latest
    #     Points to the latest stable tag, no matter what the current major version is
    #     May contain breaking changes and incompatibilities
    #     NOT RECOMMENDED!
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:edge
    #     Reflects the last commit on the default branch (main)
    #     May contain breaking changes and incompatibilities
    #     NOT RECOMMENDED!
    image: ghcr.io/geowerkstatt/interlis-check-service:v3
    # Docker container restart behavior
    restart: unless-stopped
    # Mount paths as volumes
    #
    # volumes:
    #   - /path/to/logs:/logs
    #     Application and error logs
    #
    #   - /path/to/uploads:/uploads
    #     Transfer files, ilivalidator and session logs
    #
    #   - /path/to/config:/config
    #     Config folder with TOML files to control validation
    #
    #   - /path/to/web-assets:/web-assets
    #     Folder containing optional custom web assets
    #     examples: - favicon.ico
    #               - app.png (max-height: 200px, max-width: 650px)
    #               - vendor.png (max-height: 70px, max-width: 200px)
    #               - impressum.md (imprint as Markdown-formatted document)
    #               - datenschutz.md (privacy statement as Markdown-formatted document)
    #               - info-hilfe.md (operating instructions as Markdown-formatted document)
    #               - banner.md (info banner appearing on first validation, as Markdown-formatted document)
    #               - nutzungsbestimmungen.md (terms of use as Markdown-formatted document)
    #                 Adding this document means the user must agree to the terms prior validation
    #               - quickstart.txt (line-separated brief instructions as plain text document)
    volumes:
      - ./logs:/logs
      - ./uploads:/uploads
      - ./config:/config
      - ./web-assets:/web-assets
    # Add environment variables
    #
    # environment:
    #   - PUID=1000
    #     Optional, Default user id 941
    #     Using PUID and PGID allows to map the container's internal user to a user on the
    #     host machine which prevents permisson issues when writing files to the mounted volume
    #
    #   - PGID=1000
    #     Optional, Default group id 941
    #     Using PUID and PGID allows to map the container's internal user to a user on the
    #     host machine which prevents permisson issues when writing files to the mounted volume
    #
    #   - DELETE_TRANSFER_FILES=true
    #     Optional, If set to true, transfer files get deleted right after ilivalidator
    #     has completed validation
    #     Default false
    #
    #   - TRANSFER_AND_LOG_DATA_RETENTION=15 minutes
    #     Optional, If set, transfer files and ilivalidator log files older than the
    #     specified value get deleted
    #     Keep in mind, a validation may last for several minutes. In order to prevent files
    #     from getting deleted during a long running validation choose at least '15 minutes'
    #     Default unset (preserves logs and transfer files forever)
    #     examples: - 30 minutes
    #               - 10 hours
    #               - 5 days
    #               - 3 weeks
    #               - 6 months
    #               - 1 year
    #
    #   - ILIVALIDATOR_VERSION=1.11.10
    #     Optional, Default latest version available from https://interlis.ch/downloads/ilivalidator
    #
    #   - ILIVALIDATOR_CONFIG_NAME=Beispiel1.TOML
    #     Optional TOML config file name in mounted /config volume
    #
    #   - ILIVALIDATOR_ENABLE_TRACE=true
    #     Optional, Enable validation trace messages, Default false
    #
    #   - ILIVALIDATOR_MODEL_DIR=https://models.example.com;http://models.interlis.ch/
    #     Optional semicolon-separated list of repositories with ili-files
    #     Default http://models.interlis.ch/
    #
    #   - ENABLE_GPKG_VALIDATION=true
    #     Optional, Default false
    #
    #   - ILI2GPKG_VERSION=4.7.0
    #     Optional, Default latest version available from https://interlis.ch/downloads/ili2db
    #     The ili2gpkg version is only taken into account if ENABLE_GPKG_VALIDATION is set to true
    #
    #   - PROXY=http://USER:PASSWORD@example.com:8080
    #     Optional, Configuring proxy settings for all apps in the container
    #     Protocol (e.g. http://) and port (e.g. 8080) is mandatory in order do be able
    #     to parse values for ilivalidator properly
    #     examples: - http://example.com:8080
    #               - https://host.example.com:443
    #               - http://10.10.5.68:5698
    #               - https://USER:PASSWORD@10.10.5.68:8443
    #
    #   - NO_PROXY=host.example.com,10.1.0.0/16
    #     Optional, Specifies URLs that should be excluded from proxying
    #
    #   - CUSTOM_APP_NAME=ilicop
    #     Optional custom application name
    #     Default INTERLIS Web-Check-Service
    #
    #   - CUSTOM_VENDOR_LINK=https://www.example.com
    #     Optional link to the vendors webpage
    #     The link is only taken into account if there is a corresponding vendor.png
    environment:
      - PUID=1000
      - PGID=1000
    # Expose ports (HOST:CONTAINER)
    #
    # ports:
    #   - 3080:80
    #     Map port 80 in the container to any desired port on the Docker host
    #     INTERLIS Web-Check-Service web app runs on port 80 inside the container
    ports:
      - 3080:80
```

## Individuelle Anpassung

Der INTERLIS Web-Check-Service kann in folgenden Bereichen individuell an eigene Bedürfnisse angepasst werden. Dies erfolgt entweder über das Setzen von Umgebungsvariablen oder über zusätzliche Dateien beim Starten des Docker-Containers. Eine ausführliche Beschreibung befindet sich in der oben aufgeführten [docker-compose.yml](#docker-composeyml) Beispielkonfiguration.

### ilivalidator

- Einzelne Prüfungen ein oder ausschalten
- Eigene Fehlermeldungen inkl. Attributwerte definieren
- Prüfung gegen weitere Interlis-Modelle
- Konfiguration der verwendeten ilivalidator Version

### Web-Check-Service

- Konfiguration der Aufbewahrungszeit der Transferdateien und ilivalidator Log-Dateien auf dem Server
- Eigenes Favicon-Icon
- Eigenes Anbieter-Logo mit Verlinkung zu eigener Webseite
- Eigenes Applikations-Logo
- Eigener Applikationsname
- Einbinden eines Impressums
- Einbinden von Datenschutzbestimmungen
- Einbinden eines Banners
- Einbinden von Nutzungsbestimmungen, die vor der Prüfung vom Benutzer akzeptiert werden müssen
- Einbinden eines Benutzerhandbuchs
- Einbinden eines Quick-Start-Guides

![Beispiel eines individuell angepassten INTERLIS Web-Check-Service](./assets/ilicheck_app_screenshot_customized.png)

## REST API

Der INTERLIS Web-Check-Service ist seit Version 3 vollständig über eine REST API steuerbar. Damit lassen sich Validierungen von INTERLIS Transferdateien in beliebige bestehende Datenfreigabe-Prozesse integrieren. Eine komplette Schnittstellenbeschreibung inkl. Code-Beispielen für verschiedene Client-Technolgien, sowie der Möglichkeit die REST Schnittstelle direkt im Browser zu testen steht unter `https://<host>:<port>/api` oder unter  [ilicop.ch/api](https://ilicop.ch/api) zur Verfügung.

![REST API Schnittstellenbeschreibung](./assets/ilicheck_rest_api.png)

## Health Check API

Für das Monitoring im produktiven Betrieb steht unter `https://<host>:<port>/health` eine Health Check API zur verfügung. Anhand der Antwort *Healthy* (HTTP Status Code 200), resp. *Unhealthy* (HTTP Status Code 503) kann der Status der Applikation bspw. mit cURL abgefragt werden.

```bash
curl -f https://<host>:<port>/health || exit 1;
```

Der Health Check ist auch im Docker Container integriert und kann ebenfalls über eine Shell abgefragt werden.

```bash
docker inspect --format='{{json .State.Health.Status}}' container_name
```

## Einrichten der Entwicklungsumgebung

Folgenden Komponenten müssen auf dem Entwicklungsrechner installiert sein:

- Git
- Docker
- Visual Studio 2022 oder Visual Studio Code
- Node.js 16 LTS

1. Git Repository klonen:  
   Öffne Git Shell und navigiere in den lokalen Projekt Root  
   `git clone https://github.com/GeoWerkstatt/interlis-check-service.git`

1. Web-App (React Client und .NET Core Backend) starten:  
   `IIS Express` Launch-Profil im Visual Studio resp. Visual Studio Code mit F5 starten  
   _Hinweis: Damit in der lokalen Entwicklungsumgebung der ilivalidator-Aufruf aus dem .NET Backend erfolgen kann, muss vorgängig das [docker-compose.yml](./docker-compose.yml) mit `docker-compose up` gestartet werden._

## Neue Version erstellen

Ein neuer GitHub *Pre-release* wird bei jeder Änderung auf [main](https://github.com/GeoWerkstatt/interlis-check-service) [automatisch](./.github/workflows/pre-release.yml) erstellt. In diesem Kontext wird auch ein neues Docker Image mit dem Tag *:edge* erstellt und in die [GitHub Container Registry (ghcr.io)](https://github.com/geowerkstatt/interlis-check-service/pkgs/container/interlis-check-service) gepusht. Der definitve Release erfolgt, indem die Checkbox *This is a pre-release* eines beliebigen Pre-releases entfernt wird. In der Folge wird das entsprechende Docker Image in der ghcr.io Registry mit den Tags (bspw.: *:v1*, *:v1.2.3* und *:latest*) [ergänzt](./.github/workflows/release.yml).

## Lizenz

Dieses Projekt ist unter der [GNU General Public License Version 3 (GPLv3)](https://www.gnu.org/licenses/gpl-3.0.html) lizensiert. Eine Kopie der Lizenz ist [hier](./LICENSE) abgelegt.

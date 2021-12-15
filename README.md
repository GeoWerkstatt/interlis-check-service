# INTERLIS Web-Check-Service (ILICHECK)

[![CI](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/ci.yml/badge.svg)](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/ci.yml)
[![Release](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/release.yml/badge.svg)](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/release.yml)

Webbasierter Checkservice für INTERLIS Datenabgaben

![INTERLIS Web-Check-Service](./assets/ilicheck_app_screenshot.png)

## Quick Start

Mit [Docker](https://www.docker.com/) kann der *INTERLIS Web-Check-Service* in einer isolierten Umgebung mit Docker Containern betrieben werden. Eine Beispiel-Konfiguration (`docker-compose.yml`) befindet sich im nächsten Abschnitt. Mit `docker-compose up` wird die Umgebung hochgefahren.

Um einen ersten Augenschein der Applikation zu nehmen, kann der Container in der Kommandozeile wie folgt gestartet werden:

```bash
docker run -it --rm -p 8080:80 ghcr.io/geowerkstatt/interlis-check-service
```

### docker-compose.yml

```yaml
version: '3'
services:
  web:
    # Docker image (NAME:TAG)
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:v1
    #     Stable tag for a specific major version
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:v1.2.3
    #     Stable tag for a specific version
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:v2-beta
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
    image: ghcr.io/geowerkstatt/interlis-check-service:v1
    # Docker container restart behavior
    restart: unless-stopped
    # Mount paths as volumes
    #
    # volumes:
    #   - /path/to/logs:/logs
    #     Application and error logs
    #
    #   - /path/to/uploads:/uploads
    #     XTF transfer files, ilivalidator and session logs
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
    #     Optional, If set to true, the XTF transfer file gets deleted right after ilivalidator
    #     has completed validation
    #     Default false
    #
    #   - TRANSFER_AND_LOG_DATA_RETENTION=15 minutes
    #     Optional, If set, XTF transfer files and ilivalidator log files older than the
    #     specified value get deleted
    #     Keep in mind, a validation may last for several minutes. In order to prevent files
    #     from getting deleted during a long running validation choose at least '15 minutes'
    #     Default unset (preserves logs and XTF transfer files forever)
    #     examples: - 30 minutes
    #               - 10 hours
    #               - 5 days
    #               - 3 weeks
    #               - 6 months
    #               - 1 year
    #
    #   - ILIVALIDATOR_VERSION=1.11.10
    #     Optional, Default latest version available from
    #     https://www.interlis.ch/downloads/ilivalidator
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

## Einrichten der Entwicklungsumgebung

Folgenden Komponenten müssen auf dem Entwicklungsrechner installiert sein:

* Git
* Docker
* Visual Studio 2019 oder Visual Studio Code
* Node.js 14 LTS

1. Git Repository klonen:  
   Öffne Git Shell und navigiere in den lokalen Projekt Root  
   `git clone https://github.com/GeoWerkstatt/interlis-check-service.git`

1. Web-App (React Client und .NET Core Backend) starten:  
   `IIS Express` Launch-Profil im Visual Studio resp. Visual Studio Code mit F5 starten  
   _Hinweis: Damit in der lokalen Entwicklungsumgebung der ilivalidator-Aufruf aus dem .NET Backend erfolgen kann, muss vorgängig das [docker-compose.yml](./docker-compose.yml) mit `docker-compose up` gestartet werden._

## Neue Version erstellen

Ein neuer GitHub _Pre-release_ wird bei jeder Änderung auf [main](https://github.com/GeoWerkstatt/interlis-check-service) [automatisch](./.github/workflows/pre-release.yml) erstellt. In diesem Kontext wird auch ein neues Docker Image mit dem Tag _:edge_ erstellt und in die [GitHub Container Registry (ghcr.io)](https://github.com/geowerkstatt/interlis-check-service/pkgs/container/interlis-check-service) gepusht. Der definitve Release erfolgt, indem die Checkbox _This is a pre-release_ eines beliebigen Pre-releases entfernt wird. In der Folge wird das entsprechende Docker Image in der ghcr.io Registry mit den Tags (bspw.: _:v1_, _:v1.2.3_ und _:latest_) [ergänzt](./.github/workflows/release.yml).

## Lizenz

Dieses Projekt ist unter der [GNU General Public License Version 3 (GPLv3)](https://www.gnu.org/licenses/gpl-3.0.html) lizensiert. Eine Kopie der Lizenz ist [hier](./LICENSE) abgelegt.

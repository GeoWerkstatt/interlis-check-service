# INTERLIS Check-Service (ILICHECK)

[![Release](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/release.yml/badge.svg)](https://github.com/GeoWerkstatt/interlis-check-service/actions/workflows/release.yml)

Webbasierter Checkservice für INTERLIS Datenabgaben

## Quick Start

Mit Docker-Compose kann der INTERLIS Check-Service in einer isolierten Umgebung mit Docker Containern betrieben werden. Eine Beispiel-Konfiguration (`docker-compose.yml`) befindet sich im nächsten Abschnitt. Mit `docker-compose up` wird die Umgebung hochgefahren.

### docker-compose.yml

```yaml
version: '3'
services:
  web:
    # Docker image (NAME:TAG)
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:v1
    #     Stable tag for a specific major version
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:latest
    #     Points to the latest stable tag, no matter what the current major version is
    #     May contain breaking changes and incompatibilities
    #     NOT RECOMMENDED!
    #
    #   - image: ghcr.io/geowerkstatt/interlis-check-service:edge
    #     Reflects the last commit of an active branch
    #     May contain breaking changes and incompatibilities
    #     NOT RECOMMENDED!
    image: ghcr.io/geowerkstatt/interlis-check-service:latest
    # Docker container restart behavior
    restart: unless-stopped
    # Mount paths as volumes
    #
    # volumes:
    #   - /path/to/logs:/logs
    #     Application and error logs
    #
    #   - /path/to/uploads:/uploads
    #     XTF transfer files and session logs
    #
    #   - /path/to/config:/config
    #     Config folder with TOML files to control validation
    volumes:
      - ./logs:/logs
      - ./uploads:/uploads
      - ./config:/config
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
    #               - https://hostexample.com:443
    #               - http://10.10.5.68:5698
    #               - https://USER:PASSWORD10.10.5.68:8443
    #
    #   - NO_PROXY=host.example.com,10.1.0.0/16
    #     Optional, Specifies URLs that should be excluded from proxying
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
* Visual Studio 2019
* Node.js 14 LTS

1. Git Repository klonen:  
   Öffne Git Shell und navigiere in den lokalen Projekt Root  
   `git clone https://github.com/GeoWerkstatt/interlis-check-service.git`

1. Web-App (React Client und .NET Core Backend) starten:  
   `IIS Express` Launch-Profil im Visual Studio mit F5 starten

## Neue Version erstellen

Ein neuer GitHub _Pre-release_ wird bei jeder Änderung auf [main](https://github.com/GeoWerkstatt/interlis-check-service) [automatisch](./.github/workflows/pre-release.yml) erstellt. In diesem Kontext wird auch auch ein neues Docker Image mit dem Tag _:edge_ erstellt und in die [GitHub Container Registry (ghcr.io)](https://github.com/geowerkstatt/interlis-check-service/pkgs/container/interlis-check-service) gepusht. Der definitve Release erfolgt, indem die Checkbox _This is a pre-release_ eines beliebigen Pre-releases entfernt wird. In der Folge wird das entsprechende Docker Image in der ghcr.io Registry mit den Tags (bspw.: _:v1_, _:1.1.23_ und _:latest_) [ergänzt](./.github/workflows/release.yml).

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
ARG VERSION
ARG REVISION

# Set default shell
SHELL ["/bin/bash", "-c"]

# Install Node.js
RUN apt-get update && \
    apt-get install -y gnupg && \
    mkdir -p /etc/apt/keyrings && \
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg && \
    NODE_MAJOR=20 && \
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list && \
    apt-get update && \
    apt-get install nodejs -y && \
    rm -rf /var/lib/apt/lists/*

# Restore dependencies and tools
COPY ["src/ILICheck.Web/ILICheck.Web.csproj", "ILICheck.Web/"]
RUN dotnet restore "ILICheck.Web/ILICheck.Web.csproj"

# Create optimized production build
COPY ["src/ILICheck.Web/", "ILICheck.Web/"]
ENV GENERATE_SOURCEMAP=false
ENV PUBLISH_DIR=/app/publish
RUN dotnet publish "ILICheck.Web/ILICheck.Web.csproj" \
  -c Release \
  -p:VersionPrefix=${VERSION} \
  -p:SourceRevisionId=${REVISION} \
  -o ${PUBLISH_DIR}

# Generate license and copyright notice for Node.js packages
WORKDIR /src/ILICheck.Web/ClientApp
COPY ["licenseCustomFormat.json", "/src/ILICheck.Web/ClientApp/"]
RUN npx license-checker --json --production \
  --customPath licenseCustomFormat.json \
  --out ${PUBLISH_DIR}/ClientApp/build/license.json

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
ARG VERSION
ARG REVISION
ENV HOME=/app
ENV TZ=Europe/Zurich
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ILICHECK_APP_VERSION=${VERSION}
ENV ILICHECK_APP_REVISION=${REVISION}
ENV ILICHECK_APP_HOME_DIR=/app
ENV ILICHECK_APP_LOG_DIR=/logs
ENV ILICHECK_UPLOADS_DIR=/uploads
ENV ILICHECK_WEB_ASSETS_DIR=/web-assets
ENV ILITOOLS_HOME_DIR=/ilitools
ENV ILITOOLS_CONFIG_DIR=/config
ENV ILITOOLS_MODELS_DIR=/models
ENV ILITOOLS_PLUGINS_DIR=/plugins
WORKDIR ${ILICHECK_APP_HOME_DIR}

# Install missing packages
RUN \
  DEBIAN_FRONTEND=noninteractive && \
  mkdir -p /usr/share/man/man1 /usr/share/man/man2 && \
  apt-get update && \
  apt-get install -y curl unzip default-jre-headless sudo vim htop cron libcap2-bin && \
  rm -rf /var/lib/apt/lists/*

# Add non-root user and create our folders
RUN \
 useradd --uid 941 --user-group --home $HOME --shell /bin/bash abc && \
 usermod --groups users abc && \
 mkdir -p \
   $ILICHECK_APP_HOME_DIR \
   $ILICHECK_APP_LOG_DIR \
   $ILICHECK_UPLOADS_DIR \
   $ILICHECK_WEB_ASSETS_DIR \
   $ILITOOLS_HOME_DIR \
   $ILITOOLS_CONFIG_DIR \
   $ILITOOLS_MODELS_DIR \
   $ILITOOLS_PLUGINS_DIR

EXPOSE 80
VOLUME $ILICHECK_APP_LOG_DIR
VOLUME $ILICHECK_UPLOADS_DIR
VOLUME $ILITOOLS_CONFIG_DIR
VOLUME $ILITOOLS_MODELS_DIR
VOLUME $ILITOOLS_PLUGINS_DIR

# Set default locale
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8

# Allow dotnet to bind to well known ports
RUN setcap CAP_NET_BIND_SERVICE=+eip /usr/share/dotnet/dotnet

COPY --from=build /app/publish $ILICHECK_APP_HOME_DIR
COPY docker-entrypoint.sh /entrypoint.sh
COPY ilivalidator-wrapper.sh /usr/local/bin/ilivalidator

HEALTHCHECK CMD curl --fail http://localhost/health || exit 1

ENTRYPOINT ["/entrypoint.sh"]

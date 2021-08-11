FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

# Install Node.js
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs

# Restore dependencies and tools
COPY ["src/ILICheck.Web/ILICheck.Web.csproj", "src/ILICheck.Web/"]
RUN dotnet restore "src/ILICheck.Web/ILICheck.Web.csproj"

# Create optimized production build
COPY ["src/ILICheck.Web/", "src/ILICheck.Web/"]
RUN dotnet publish "src/ILICheck.Web/ILICheck.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final

ENV HOME=/app
ENV TZ=Europe/Zurich
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ILICHECK_APP_HOME_DIR=/app
ENV ILICHECK_APP_LOG_DIR=/logs
ENV ILICHECK_UPLOADS_DIR=/uploads
ENV ILIVALIDATOR_HOME_DIR=/ilivalidator
ENV ILIVALIDATOR_CONFIG_DIR=/config
WORKDIR ${ILICHECK_APP_HOME_DIR}

# Install missing packages (curl unzip jre sudo vim htop)
RUN \
  DEBIAN_FRONTEND=noninteractive && \
  mkdir -p /usr/share/man/man1 /usr/share/man/man2 && \
  apt-get update && \
  apt-get install -y curl unzip default-jre-headless sudo vim htop && \
  rm -rf /var/lib/apt/lists/*

# Add non-root user and create our folders
RUN \
 useradd --uid 941 --user-group --home $HOME --shell /bin/bash abc && \
 usermod --groups users abc && \
 mkdir -p \
   $ILICHECK_APP_HOME_DIR \
   $ILICHECK_APP_LOG_DIR \
   $ILICHECK_UPLOADS_DIR \
   $ILIVALIDATOR_HOME_DIR \
   $ILIVALIDATOR_CONFIG_DIR

EXPOSE 80
VOLUME $ILICHECK_APP_LOG_DIR
VOLUME $ILICHECK_UPLOADS_DIR
VOLUME $ILIVALIDATOR_CONFIG_DIR

COPY --from=build /app/publish $ILICHECK_APP_HOME_DIR
COPY docker-entrypoint.sh /entrypoint.sh
COPY ilivalidator-wrapper.sh /usr/local/bin/ilivalidator

ENTRYPOINT ["/entrypoint.sh"]

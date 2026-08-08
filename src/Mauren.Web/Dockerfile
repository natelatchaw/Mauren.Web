# See https://aka.ms/customizecontainer to learn how to customize your debug container 

# -----------------------------------------------------------------------------
# 1. Downloader Helper Stage
# -----------------------------------------------------------------------------
FROM debian:bookworm-slim AS downloader
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates curl xz-utils unzip \
    && rm -rf /var/lib/apt/lists/*

RUN curl -L -o /tmp/ffmpeg.tar.xz https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz \
    && mkdir -p /tmp/ffmpeg \
    && tar -xJf /tmp/ffmpeg.tar.xz -C /tmp/ffmpeg --strip-components=1

RUN curl -L -o /tmp/libdave.zip https://github.com/discord/libdave/releases/download/v1.1.1%2Fcpp/libdave-Linux-X64-boringssl.zip \
    && mkdir -p /tmp/extracted /tmp/dave \
    && unzip /tmp/libdave.zip -d /tmp/extracted \
    && find /tmp/extracted -name "libdave.so*" -exec cp {} /tmp/dave/libdave.so \;
    
RUN curl -L -o /tmp/yt-dlp_linux.zip https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux.zip \
    && mkdir -p /tmp/yt-dlp \
    && unzip /tmp/yt-dlp_linux.zip -d /tmp/yt-dlp

# -----------------------------------------------------------------------------
# 2. Base Stage (Used by Visual Studio Fast Mode)
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Temporarily switch to root to install dependencies needed for BOTH debugging and production
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libopus0 \
    libopus-dev \
    libsodium23 \
    libsodium-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=downloader /tmp/ffmpeg/ffmpeg /usr/local/bin/ffmpeg
COPY --from=downloader /tmp/ffmpeg/ffprobe /usr/local/bin/ffprobe
COPY --from=downloader /tmp/dave/libdave.so /usr/local/lib/libdave.so
COPY --from=downloader /tmp/yt-dlp /opt/yt-dlp

RUN chmod +x /usr/local/bin/ffmpeg /usr/local/bin/ffprobe /opt/yt-dlp/yt-dlp_linux \
    && ln -s /opt/yt-dlp/yt-dlp_linux /usr/local/bin/yt-dlp \
    && ldconfig

# Switch back to the unprivileged user so Visual Studio attaches its debugger cleanly
USER $APP_UID
	
# -----------------------------------------------------------------------------
# 3. Build Stage
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

ARG GITHUB_USERNAME
ARG GITHUB_PASSWORD

RUN --mount=type=secret,id=token,required=false \
    if [ -f /run/secrets/token ]; then \
        NUGET_TOKEN=$(cat /run/secrets/token); \
    else \
        NUGET_TOKEN="${GITHUB_PASSWORD}"; \
    fi && \
    dotnet nuget add source "https://nuget.pkg.github.com/natelatchaw/index.json" \
    --name "natelatchaw" \
    --username "${GITHUB_USERNAME}" \
    --password "${NUGET_TOKEN}" \
    --store-password-in-clear-text

COPY ["Mauren.Web.slnx", "."]
COPY ["src/Mauren.Web/Mauren.Web.csproj", "src/Mauren.Web/"]
RUN dotnet restore "./src/Mauren.Web/Mauren.Web.csproj"

COPY ["src/", "src/"]
RUN dotnet build "./src/Mauren.Web/Mauren.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -----------------------------------------------------------------------------
# 4. Publish Stage
# -----------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./src/Mauren.Web/Mauren.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------------
# 5. Final Stage (Used by Unraid / Production)
# -----------------------------------------------------------------------------
FROM base AS final
WORKDIR /app

# Switch to root for volume setup and execution control
USER root

COPY --from=publish /app/publish .
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Setup configurations and directories for Unraid mounts
RUN mkdir -p Content \
    && cp appsettings*.json Content/ 2>/dev/null || true \
    && cp Content/appsettings*.json /app/ 2>/dev/null || true \
    && chown -R $APP_UID:$APP_UID /app /entrypoint.sh

# Container remains root to allow entrypoint.sh to map Unraid volumes before hand-off
ENTRYPOINT ["/entrypoint.sh"]
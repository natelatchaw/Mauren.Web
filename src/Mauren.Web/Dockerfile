# See https://aka.ms/customizecontainer to learn how to customize your debug container 

# -----------------------------------------------------------------------------
# Base Stage
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Temporarily switch to root to install dependencies needed for BOTH debugging and production
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    curl \
    unzip \
    xz-utils \
    libopus0 \
    libopus-dev \
    libsodium23 \
    libsodium-dev \
    && rm -rf /var/lib/apt/lists/*

# Switch back to the unprivileged user so Visual Studio attaches its debugger cleanly
USER $APP_UID
	
# -----------------------------------------------------------------------------
# Build Stage
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
# Publish Stage
# -----------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./src/Mauren.Web/Mauren.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------------
# Final Stage
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
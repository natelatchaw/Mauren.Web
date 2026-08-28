#!/bin/bash
# Check if the script is running as root (UID 0)
if [ "$(id -u)" = "0" ]; then
    # We are in Unraid/Production mode
    USER_ID=${PUID:-$APP_UID}
    USER_ID=${USER_ID:-1654} # Fallback to standard dotnet UID    
    
    echo "Initialize: Running as root. Setting up Unraid volumes for UID: $USER_ID..."
    
    # Ensure the Content directory exists
    mkdir -p /app/Content

    # Seed the Unraid volume with an empty JSON object if it doesn't exist
    if [ ! -f /app/Content/appsettings.json ]; then
        echo "Initialize: Seeding Unraid volume with an empty JSON object..."
        echo "{}" > /app/Content/appsettings.json
    fi

    # ----- yt-dlp Runtime Management -----
    # Define the yt-dlp directory
    YTDLP_DIR="/app/yt-dlp"
    # Make a directory for yt-dlp binaries/dependencies
    mkdir -p "$YTDLP_DIR"

    # If the binary does not exist in the yt-dlp directory
    if [ ! -f "$YTDLP_DIR/yt-dlp" ]; then
        echo "Initialize: Downloading yt-dlp..."
        # Download the latest yt-dlp binary
        curl -L -o "$YTDLP_DIR/yt-dlp" https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux
    # If the binary does exist in the yt-dlp directory
    else
        echo "Initialize: Updating yt-dlp..."
        # Update the existing yt-dlp binary
        "$YTDLP_DIR/yt-dlp" -U || true
    fi

    # Mark the yt-dlp binary as executable
    chmod +x "$YTDLP_DIR/yt-dlp"
    # Symlink the yt-dlp binary to the local user bin directory
    ln -sf "$YTDLP_DIR/yt-dlp" /usr/local/bin/yt-dlp
    # Apply permissions to the yt-dlp directory
    chown -R $USER_ID "$YTDLP_DIR"
    # -------------------------------------
    
    # Apply permissions ONLY to the Unraid mounted folder. Leave the root files alone.
    chown -R $USER_ID /app/Content
    
    # Hand off to the app using setpriv to drop privileges
    exec setpriv --reuid=$USER_ID --regid=$USER_ID --clear-groups dotnet Mauren.Web.dll
else
    # We are in Visual Studio F5 mode
    echo "Initialize: Running as non-root (UID: $(id -u)). Bypassing volume setup for Visual Studio..."

    # ----- yt-dlp Runtime Management -----
    # Ensure yt-dlp exists for local debugging if not already present
    if [ ! -f /opt/yt-dlp/yt-dlp ]; then
        # Make a directory for yt-dlp binaries/dependencies
        mkdir -p /opt/yt-dlp
        # Download the latest yt-dlp binary
        curl -L -o /opt/yt-dlp/yt-dlp https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux
        # Mark the yt-dlp binary as executable
        chmod +x /opt/yt-dlp/yt-dlp
    fi
    # Symlink the yt-dlp binary to the local user bin directory
    ln -sf /opt/yt-dlp/yt-dlp /usr/local/bin/yt-dlp
    # -------------------------------------
    
    # Execute the app directly since VS is already running as the target user
    exec dotnet Mauren.Web.dll
fi
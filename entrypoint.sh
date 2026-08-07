#!/bin/bash

# Check if the script is running as root (UID 0)
if [ "$(id -u)" = "0" ]; then
    # We are in Unraid/Production mode
    USER_ID=${PUID:-$APP_UID}
    USER_ID=${USER_ID:-1654} # Fallback to standard dotnet UID
    
    echo "Initialize: Running as root. Setting up Unraid volumes for UID: $USER_ID..."
    
    if [ ! -f /app/Content/appsettings.json ]; then
        echo "Initialize: Populating Unraid volume..."
        cp /app/Defaults/appsettings*.json /app/Content/ 2>/dev/null || true
        chown -R $USER_ID /app/Content
    fi
    
    ln -sf /app/Content/appsettings.json /app/appsettings.json
    
    if [ -f /app/Content/appsettings.Development.json ]; then
        ln -sf /app/Content/appsettings.Development.json /app/appsettings.Development.json
    fi
    
    chown -h $USER_ID /app/appsettings*.json
    
    # Hand off to the app using setpriv to drop privileges
    exec setpriv --reuid=$USER_ID --regid=$USER_ID --clear-groups dotnet Mauren.Web.dll

else
    # We are in Visual Studio F5 mode
    echo "Initialize: Running as non-root (UID: $(id -u)). Bypassing volume setup for Visual Studio..."
    
    # Execute the app directly since VS is already running as the target user
    exec dotnet Mauren.Web.dll
fi
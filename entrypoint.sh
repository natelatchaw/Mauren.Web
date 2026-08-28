#!/bin/bash
set -e # Abort the script immediately if any command fails

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

    ### -------------------------------------
    ### libDave Runtime Management
    ### -------------------------------------
    # Define the DAVE directory
    DAVE_DIR="/opt/dave"

    # If the DAVE binary does not exist in the DAVE directory
    if [ ! -f "$DAVE_DIR/libdave.so" ]; then
        echo "Initialize: Downloading libdave..."
        # Define the path to the archive
        DAVE_ARCHIVE="/tmp/libdave.zip"
        DAVE_TEMP_DIR="/tmp/dave"
        # Make a directory for DAVE binaries/dependencies and temp extraction location
        mkdir -p "$DAVE_DIR" "$DAVE_TEMP_DIR"
        # Download the latest DAVE binary
        curl -L --fail -o "$DAVE_ARCHIVE" https://github.com/discord/libdave/releases/download/v1.1.1%2Fcpp/libdave-Linux-X64-boringssl.zip
        # Extract the archive to the DAVE temp directory
        unzip -q "$DAVE_ARCHIVE" -d "$DAVE_TEMP_DIR"
        # Locate the libdave binary in the temp extraction directory and copy to DAVE directory
        find "$DAVE_TEMP_DIR" -name "libdave.so*" -exec cp {} "$DAVE_DIR/libdave.so" \;
        # Remove temporary files and directories
        rm -rf "$DAVE_TEMP_DIR" "$DAVE_ARCHIVE"
    else
        echo "Initialize: Found existing libdave install at $DAVE_DIR"
    fi


    # Symlink the DAVE binary
    ln -sf "$DAVE_DIR/libdave.so" /usr/local/lib/libdave.so
    ldconfig
    ### -------------------------------------
    
    ### -------------------------------------
    ### FFmpeg Runtime Management
    ### -------------------------------------
    # Define the FFmpeg directory
    FFMPEG_DIR="/opt/ffmpeg"

    # If the FFmpeg binary does not exist in the FFmpeg directory
    if [ ! -f "$FFMPEG_DIR/ffmpeg" ]; then
        echo "Initialize: Downloading FFmpeg..."
        # Define the path to the archive
        FFMPEG_ARCHIVE="/tmp/ffmpeg.tar.xz"
        # Make a directory for FFmpeg binaries/dependencies
        mkdir -p "$FFMPEG_DIR"
        # Download the latest FFmpeg binary
        curl -L --fail -o "$FFMPEG_ARCHIVE" https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-linux64-gpl.tar.xz
        # Extract the archive to the FFmpeg directory
        tar -xJf "$FFMPEG_ARCHIVE" -C "$FFMPEG_DIR" --strip-components=1
        # Move all files in /bin to the root FFmpeg directory
        mv "$FFMPEG_DIR/bin/"* "$FFMPEG_DIR/"
        # Remove temporary files and directories
        rm -rf "$FFMPEG_DIR/bin" "$FFMPEG_ARCHIVE"
    else
        echo "Initialize: Found existing FFmpeg install at $FFMPEG_DIR"
    fi

    # Mark the FFmpeg binaries as executable
    chmod +x "$FFMPEG_DIR/ffmpeg" "$FFMPEG_DIR/ffprobe"
    # Symlink the FFmpeg binaries
    ln -sf "$FFMPEG_DIR/ffmpeg" /usr/local/bin/ffmpeg
    ln -sf "$FFMPEG_DIR/ffprobe" /usr/local/bin/ffprobe
    # Apply permissions
    chown -R $USER_ID "$FFMPEG_DIR"
    ### -------------------------------------
    
    ### -------------------------------------
    ### Deno Runtime Management
    ### -------------------------------------
    # Define the Deno directory
    DENO_DIR="/opt/deno"

    # If the Deno binary does not exist in the Deno directory
    if [ ! -f "$DENO_DIR/deno" ]; then
        echo "Initialize: Downloading Deno..."
        # Define the path to the archive
        DENO_ARCHIVE="/tmp/deno.zip"
        # Make a directory for Deno binaries/dependencies
        mkdir -p "$DENO_DIR"
        # Download the latest Deno binary
        curl -L --fail -o "$DENO_ARCHIVE" https://github.com/denoland/deno/releases/latest/download/deno-x86_64-unknown-linux-gnu.zip
        # Extract the archive to the Deno directory
        unzip -q "$DENO_ARCHIVE" -d "$DENO_DIR"
        # Remove temporary files and directories
        rm -rf "$DENO_ARCHIVE"
    else
        echo "Initialize: Found existing deno install at $DENO_DIR"
    fi

    # Mark the Deno binaries as executable
    chmod +x "$DENO_DIR/deno"
    # Symlink the Deno binaries
    ln -sf "$DENO_DIR/deno" /usr/local/bin/deno
    # Apply permissions
    chown -R $USER_ID "$DENO_DIR"
    ### -------------------------------------

    ### -------------------------------------
    ### yt-dlp Runtime Management
    ### -------------------------------------
    # Define the yt-dlp directory
    YTDLP_DIR="/opt/yt-dlp"

    # If the binary does not exist in the yt-dlp directory
    if [ ! -f "$YTDLP_DIR/yt-dlp" ]; then
        echo "Initialize: Downloading yt-dlp..."
        # Make a directory for yt-dlp binaries/dependencies
        mkdir -p "$YTDLP_DIR"
        # Download the latest yt-dlp binary
        curl -L --fail -o "$YTDLP_DIR/yt-dlp" https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux
    # If the binary does exist in the yt-dlp directory
    else
        echo "Initialize: Updating yt-dlp..."
        # Update the existing yt-dlp binary
        "$YTDLP_DIR/yt-dlp" -U || true
    fi

    # Mark the yt-dlp binary as executable
    chmod +x "$YTDLP_DIR/yt-dlp"
    # Symlink the yt-dlp binary
    ln -sf "$YTDLP_DIR/yt-dlp" /usr/local/bin/yt-dlp
    # Apply permissions to the yt-dlp directory
    chown -R $USER_ID "$YTDLP_DIR"
    ### -------------------------------------
    
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
        curl -L --fail -o /opt/yt-dlp/yt-dlp https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux
        # Mark the yt-dlp binary as executable
        chmod +x /opt/yt-dlp/yt-dlp
    fi
    # Symlink the yt-dlp binary to the local user bin directory
    ln -sf /opt/yt-dlp/yt-dlp /usr/local/bin/yt-dlp
    # -------------------------------------
    
    # Execute the app directly since VS is already running as the target user
    exec dotnet Mauren.Web.dll
fi
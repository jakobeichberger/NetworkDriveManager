#!/bin/bash
# NetworkDriveManager Auto-Update Script (macOS)
# Downloads and installs the latest version from GitHub Releases.
# Usage: sudo bash update-macos.sh

set -e

APP_NAME="NetworkDriveManager"
INSTALL_DIR="/Applications/$APP_NAME"
REPO_OWNER="jakobeichberger"
REPO_NAME="NetworkDriveManager"

echo ""
echo "=== $APP_NAME Auto-Updater (macOS) ==="
echo ""

if [ "$(id -u)" -ne 0 ]; then
    echo "Error: This script must be run with sudo."
    echo "Usage: sudo bash update-macos.sh"
    exit 1
fi

# Determine architecture
ARCH=$(uname -m)
if [ "$ARCH" = "arm64" ]; then
    ASSET_PATTERN="NetworkDriveManager-macos-arm64.tar.gz"
else
    ASSET_PATTERN="NetworkDriveManager-macos-x64.tar.gz"
fi

# Get current version
CURRENT_VERSION="0.0.0"
if [ -f "$INSTALL_DIR/$APP_NAME" ]; then
    CURRENT_VERSION=$("$INSTALL_DIR/$APP_NAME" --version 2>/dev/null || echo "0.0.0")
fi
echo "Current version: $CURRENT_VERSION"

# Fetch latest release
echo "Checking for updates..."
RELEASE_JSON=$(curl -s -H "User-Agent: NetworkDriveManager-Updater" \
    "https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/releases/latest")

LATEST_VERSION=$(echo "$RELEASE_JSON" | grep '"tag_name"' | head -1 | sed 's/.*"v\?\([^"]*\)".*/\1/')

if [ -z "$LATEST_VERSION" ]; then
    echo "Error: Could not determine latest version."
    exit 1
fi
echo "Latest version:  $LATEST_VERSION"

# Simple version comparison
if [ "$CURRENT_VERSION" = "$LATEST_VERSION" ]; then
    echo ""
    echo "You are already running the latest version."
    exit 0
fi

echo ""
echo "New version available: $LATEST_VERSION"

# Find download URL
DOWNLOAD_URL=$(echo "$RELEASE_JSON" | grep "browser_download_url" | grep "$ASSET_PATTERN" | head -1 | sed 's/.*"\(https[^"]*\)".*/\1/')

if [ -z "$DOWNLOAD_URL" ]; then
    echo "Error: No compatible download found for $ASSET_PATTERN"
    exit 1
fi

# Download and extract
TEMP_DIR=$(mktemp -d)
echo "Downloading $ASSET_PATTERN..."
curl -L -o "$TEMP_DIR/update.tar.gz" "$DOWNLOAD_URL"

echo "Extracting update..."
tar -xzf "$TEMP_DIR/update.tar.gz" -C "$TEMP_DIR"

# Stop the running application
if pgrep -x "$APP_NAME" > /dev/null 2>&1; then
    echo "Stopping $APP_NAME..."
    pkill -x "$APP_NAME" 2>/dev/null || true
    sleep 2
fi

# Install the update
echo "Installing update to $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"
EXTRACTED_DIR=$(find "$TEMP_DIR" -maxdepth 1 -type d -name "NetworkDriveManager-*" | head -1)
if [ -n "$EXTRACTED_DIR" ]; then
    cp -R "$EXTRACTED_DIR"/* "$INSTALL_DIR/"
fi
chmod +x "$INSTALL_DIR/$APP_NAME"

# Cleanup
rm -rf "$TEMP_DIR"

echo ""
echo "Update to version $LATEST_VERSION complete!"
echo ""

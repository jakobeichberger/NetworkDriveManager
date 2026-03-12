#!/bin/bash
# NetworkDriveManager macOS Installer
# Usage: sudo bash install.sh

set -e

APP_NAME="NetworkDriveManager"
INSTALL_DIR="/Applications/$APP_NAME"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo ""
echo "=== $APP_NAME macOS Installer ==="
echo ""

if [ "$(id -u)" -ne 0 ]; then
    echo "Error: This script must be run with sudo."
    echo "Usage: sudo bash install.sh"
    exit 1
fi

if [ ! -f "$SCRIPT_DIR/$APP_NAME" ]; then
    echo "Error: $APP_NAME binary not found in $SCRIPT_DIR"
    echo "Please run this script from the extracted archive directory."
    exit 1
fi

# Create installation directory
echo "Installing to: $INSTALL_DIR"
mkdir -p "$INSTALL_DIR"

# Copy application files
echo "Copying files..."
cp -R "$SCRIPT_DIR"/* "$INSTALL_DIR/"
rm -f "$INSTALL_DIR/install.sh" "$INSTALL_DIR/uninstall.sh"

# Make the binary executable
chmod +x "$INSTALL_DIR/$APP_NAME"

# Create a symlink in /usr/local/bin for command-line access
if [ -d "/usr/local/bin" ]; then
    ln -sf "$INSTALL_DIR/$APP_NAME" /usr/local/bin/networkdrivemanager
    echo "Created symlink: /usr/local/bin/networkdrivemanager"
fi

echo ""
echo "Installation complete!"
echo "You can run $APP_NAME from: $INSTALL_DIR/$APP_NAME"
echo "Or from the command line: networkdrivemanager"
echo ""

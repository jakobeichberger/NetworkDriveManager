#!/bin/bash
# NetworkDriveManager macOS Uninstaller
# Usage: sudo bash uninstall.sh

set -e

APP_NAME="NetworkDriveManager"
INSTALL_DIR="/Applications/$APP_NAME"

echo ""
echo "=== $APP_NAME macOS Uninstaller ==="
echo ""

if [ "$(id -u)" -ne 0 ]; then
    echo "Error: This script must be run with sudo."
    echo "Usage: sudo bash uninstall.sh"
    exit 1
fi

# Remove symlink
if [ -L "/usr/local/bin/networkdrivemanager" ]; then
    rm -f /usr/local/bin/networkdrivemanager
    echo "Removed symlink: /usr/local/bin/networkdrivemanager"
fi

# Remove installation directory
if [ -d "$INSTALL_DIR" ]; then
    rm -rf "$INSTALL_DIR"
    echo "Removed installation directory: $INSTALL_DIR"
fi

echo ""
echo "Uninstallation complete."
echo ""

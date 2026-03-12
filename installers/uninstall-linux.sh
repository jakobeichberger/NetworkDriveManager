#!/bin/bash
# NetworkDriveManager Linux Uninstaller
# Usage: sudo bash uninstall-linux.sh

set -e

APP_NAME="NetworkDriveManager"
INSTALL_DIR="/opt/$APP_NAME"
DESKTOP_FILE="/usr/share/applications/networkdrivemanager.desktop"

echo ""
echo "=== $APP_NAME Linux Uninstaller ==="
echo ""

if [ "$(id -u)" -ne 0 ]; then
    echo "Error: This script must be run with sudo."
    echo "Usage: sudo bash uninstall-linux.sh"
    exit 1
fi

# Remove symlink
if [ -L "/usr/local/bin/networkdrivemanager" ]; then
    rm -f /usr/local/bin/networkdrivemanager
    echo "Removed symlink: /usr/local/bin/networkdrivemanager"
fi

# Remove .desktop file
if [ -f "$DESKTOP_FILE" ]; then
    rm -f "$DESKTOP_FILE"
    echo "Removed application menu entry."
    if command -v update-desktop-database &> /dev/null; then
        update-desktop-database /usr/share/applications/ 2>/dev/null || true
    fi
fi

# Remove installation directory
if [ -d "$INSTALL_DIR" ]; then
    rm -rf "$INSTALL_DIR"
    echo "Removed installation directory: $INSTALL_DIR"
fi

echo ""
echo "Uninstallation complete."
echo ""

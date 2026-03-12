#!/bin/bash
# NetworkDriveManager Linux Installer
# Usage: sudo bash install.sh
#
# This script installs the application and creates a .desktop file
# for easy launching from the application menu.

set -e

APP_NAME="NetworkDriveManager"
BINARY_NAME="NetworkDriveManager"
INSTALL_DIR="/opt/$APP_NAME"
DESKTOP_FILE="/usr/share/applications/networkdrivemanager.desktop"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo ""
echo "=== $APP_NAME Linux Installer ==="
echo ""

if [ "$(id -u)" -ne 0 ]; then
    echo "Error: This script must be run with sudo."
    echo "Usage: sudo bash install.sh"
    exit 1
fi

if [ ! -f "$SCRIPT_DIR/$BINARY_NAME" ]; then
    echo "Error: $BINARY_NAME binary not found in $SCRIPT_DIR"
    echo "Please run this script from the extracted archive directory."
    exit 1
fi

# Install required system packages for SMB/CIFS support
echo "Checking system dependencies..."
if command -v apt-get &> /dev/null; then
    echo "Detected apt-based system (Debian/Ubuntu)."
    apt-get update -qq
    apt-get install -y -qq cifs-utils libice6 libsm6 libx11-6 libfontconfig1 2>/dev/null || true
elif command -v dnf &> /dev/null; then
    echo "Detected dnf-based system (Fedora/RHEL)."
    dnf install -y -q cifs-utils libICE libSM libX11 fontconfig 2>/dev/null || true
elif command -v pacman &> /dev/null; then
    echo "Detected pacman-based system (Arch)."
    pacman -S --noconfirm --needed cifs-utils libice libsm libx11 fontconfig 2>/dev/null || true
elif command -v zypper &> /dev/null; then
    echo "Detected zypper-based system (openSUSE)."
    zypper install -y cifs-utils libICE6 libSM6 libX11-6 fontconfig 2>/dev/null || true
else
    echo "Warning: Could not detect package manager. Please ensure cifs-utils is installed manually."
fi

# Create installation directory
echo "Installing to: $INSTALL_DIR"
mkdir -p "$INSTALL_DIR"

# Copy application files
echo "Copying files..."
cp -R "$SCRIPT_DIR"/* "$INSTALL_DIR/"
rm -f "$INSTALL_DIR/install.sh" "$INSTALL_DIR/uninstall.sh"

# Make the binary executable
chmod +x "$INSTALL_DIR/$BINARY_NAME"

# Create symlink for command-line access
ln -sf "$INSTALL_DIR/$BINARY_NAME" /usr/local/bin/networkdrivemanager

# Create .desktop file for application menu integration
echo "Creating application menu entry..."
cat > "$DESKTOP_FILE" << EOF
[Desktop Entry]
Name=Network Drive Manager
Comment=Manage SMB/CIFS network drive connections
Exec=$INSTALL_DIR/$BINARY_NAME
Terminal=false
Type=Application
Categories=System;Network;FileManager;
StartupNotify=true
EOF

# Update desktop database
if command -v update-desktop-database &> /dev/null; then
    update-desktop-database /usr/share/applications/ 2>/dev/null || true
fi

echo ""
echo "Installation complete!"
echo "You can launch $APP_NAME from your application menu or run: networkdrivemanager"
echo ""

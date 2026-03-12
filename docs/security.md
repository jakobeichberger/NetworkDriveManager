---
layout: default
title: Security
---

# Security

Network Drive Manager is designed to handle credentials securely. This page describes how credentials are stored, encrypted, and used.

## Credential Encryption

Credentials (username and password) are encrypted using **AES-256-GCM** (Galois/Counter Mode) before being written to disk.

| Aspect | Detail |
|--------|--------|
| **Algorithm** | AES-256-GCM |
| **Key size** | 256 bits (32 bytes) |
| **Nonce** | Randomly generated per encryption operation |
| **Authentication** | GCM provides built-in authentication (integrity + confidentiality) |

### Encryption Flow

1. On first use, a random 256-bit key is generated and saved to `secret.key`.
2. When the user clicks **Save Credentials**, the username and password are serialised and encrypted with the key.
3. The encrypted payload (nonce + ciphertext + auth tag) is written to `credentials.enc`.
4. On subsequent launches, the application reads `secret.key`, decrypts `credentials.enc`, and populates the credential fields.

### File Permissions

On **Linux** and **macOS**, the application sets file permissions for sensitive files:

| File | Permissions |
|------|-------------|
| `secret.key` | `0600` (owner read/write only) |
| `credentials.enc` | `0600` (owner read/write only) |

This is enforced using `File.SetUnixFileMode()` in `CredentialService`.

## No Network Transmission

The application **never** transmits credentials over the network itself. Credentials are only passed to the operating system's mount command:

- **Windows** — passed as arguments to `net use`
- **macOS** — passed via the `PASSWD` environment variable to `mount_smbfs` (avoids credentials appearing in the process list)
- **Linux** — written to a temporary credentials file with `chmod 600`, used by `mount -t cifs`, then deleted

## Sensitive Files

The following files are created at runtime and contain sensitive data:

| File | Contains | Protected By |
|------|----------|-------------|
| `secret.key` | AES-256 encryption key | File permissions (0600 on Unix) |
| `credentials.enc` | Encrypted username and password | AES-256-GCM encryption |

Both files are listed in `.gitignore` and should **never** be committed to version control.

## Best Practices

- Keep `secret.key` and `credentials.enc` in a location with restricted access.
- Do not share or copy these files between machines — generate new credentials on each installation.
- The application stores credentials locally and never sends them to any remote service beyond the configured file servers.
- Use strong, unique passwords for your network shares.
- Ensure the directory containing the application is not world-readable.

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NetworkDriveManager.Services;

/// <summary>
/// Manages encrypted credential storage using AES-GCM.
/// Mirrors the Python version's Fernet-based encryption approach.
/// </summary>
public static class CredentialService
{
    private static readonly string _keyFile = Path.Combine(ConfigService.GetRuntimeDir(), "secret.key");
    private static readonly string _credFile = Path.Combine(ConfigService.GetRuntimeDir(), "credentials.enc");

    private const int NonceSize = 12;
    private const int TagSize = 16;

    private static byte[] GetOrCreateKey()
    {
        if (File.Exists(_keyFile))
        {
            var key = File.ReadAllBytes(_keyFile);
            LogService.Debug($"Loaded encryption key from {_keyFile}");
            return key;
        }

        // Generate a new 256-bit AES key
        var newKey = new byte[32];
        RandomNumberGenerator.Fill(newKey);
        File.WriteAllBytes(_keyFile, newKey);
        SetRestrictivePermissions(_keyFile);
        LogService.Info($"Generated new encryption key at {_keyFile}");
        return newKey;
    }

    /// <summary>
    /// Encrypt and persist credentials to disk.
    /// </summary>
    public static void SaveCredentials(string username, string password)
    {
        var key = GetOrCreateKey();
        var payload = JsonSerializer.Serialize(new { username, password });
        var plaintext = Encoding.UTF8.GetBytes(payload);

        // AES-GCM: nonce (12 bytes) + ciphertext + tag (16 bytes)
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        // Write: [NonceSize nonce][TagSize tag][ciphertext]
        using var fs = File.Create(_credFile);
        fs.Write(nonce);
        fs.Write(tag);
        fs.Write(ciphertext);
        fs.Close();
        SetRestrictivePermissions(_credFile);

        LogService.Info($"Credentials saved (encrypted) for user '{username}'");
    }

    /// <summary>
    /// Sets file permissions to owner-only (0600) on non-Windows platforms.
    /// </summary>
    private static void SetRestrictivePermissions(string filePath)
    {
        if (!PlatformService.IsWindows && File.Exists(filePath))
        {
            try
            {
#pragma warning disable CA1416 // Guarded by PlatformService.IsWindows check above
                File.SetUnixFileMode(filePath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite);
#pragma warning restore CA1416
            }
            catch (Exception ex)
            {
                LogService.Debug($"Could not set restrictive permissions on {filePath}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Load and decrypt stored credentials. Returns (username, password) or (null, null).
    /// </summary>
    public static (string? Username, string? Password) LoadCredentials()
    {
        if (!File.Exists(_credFile) || !File.Exists(_keyFile))
            return (null, null);

        try
        {
            var key = GetOrCreateKey();
            var data = File.ReadAllBytes(_credFile);

            if (data.Length < NonceSize + TagSize)
                return (null, null);

            var nonce = data[..NonceSize];
            var tag = data[NonceSize..(NonceSize + TagSize)];
            var ciphertext = data[(NonceSize + TagSize)..];
            var plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            var json = Encoding.UTF8.GetString(plaintext);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var username = root.GetProperty("username").GetString();
            var password = root.GetProperty("password").GetString();

            LogService.Info($"Loaded saved credentials for user '{username}'");
            return (username, password);
        }
        catch (Exception ex)
        {
            LogService.Warning($"Could not decrypt stored credentials: {ex.Message}");
            return (null, null);
        }
    }
}

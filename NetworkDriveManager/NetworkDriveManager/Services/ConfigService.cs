using System.IO;
using System.Text.Json;
using NetworkDriveManager.Models;

namespace NetworkDriveManager.Services;

/// <summary>
/// Manages loading and saving the drives_config.json configuration file.
/// </summary>
public static class ConfigService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Returns the directory used for mutable runtime files (config, credentials, logs).
    /// </summary>
    public static string GetRuntimeDir()
    {
        var exePath = Environment.ProcessPath;
        if (exePath != null)
            return Path.GetDirectoryName(exePath)!;
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public static string ConfigFilePath => Path.Combine(GetRuntimeDir(), "drives_config.json");
    public static string LogFilePath => Path.Combine(GetRuntimeDir(), "network_drive_manager.log");

    /// <summary>
    /// Load drive definitions from the JSON configuration file.
    /// </summary>
    public static List<DriveConfig> LoadConfig()
    {
        var path = ConfigFilePath;
        if (!File.Exists(path))
        {
            LogService.Warning($"Configuration file not found: {path}");
            return new List<DriveConfig>();
        }

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<DrivesConfigFile>(json, _jsonOptions);
            var drives = data?.Drives ?? new List<DriveConfig>();
            LogService.Info($"Loaded {drives.Count} drive(s) from {path}");
            return drives;
        }
        catch (JsonException ex)
        {
            LogService.Error($"Invalid JSON in configuration file: {ex.Message}");
            return new List<DriveConfig>();
        }
    }

    /// <summary>
    /// Persist drive definitions to the JSON configuration file.
    /// </summary>
    public static void SaveConfig(List<DriveConfig> drives)
    {
        var data = new DrivesConfigFile { Drives = drives };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(ConfigFilePath, json);
        LogService.Info($"Saved {drives.Count} drive(s) to {ConfigFilePath}");
    }
}

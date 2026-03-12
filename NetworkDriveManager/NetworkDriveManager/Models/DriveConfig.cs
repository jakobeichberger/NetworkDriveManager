namespace NetworkDriveManager.Models;

/// <summary>
/// Represents a single network drive configuration entry.
/// </summary>
public class DriveConfig
{
    public string Letter { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Share { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Hidden { get; set; }

    public string UncPath => $@"\\{Server}\{Share}";

    public DriveConfig Clone() => new()
    {
        Letter = Letter,
        Server = Server,
        Share = Share,
        Label = Label,
        Hidden = Hidden
    };
}

/// <summary>
/// Root object for drives_config.json.
/// </summary>
public class DrivesConfigFile
{
    public List<DriveConfig> Drives { get; set; } = new();
}

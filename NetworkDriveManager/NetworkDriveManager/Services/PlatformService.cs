using System.Runtime.InteropServices;

namespace NetworkDriveManager.Services;

/// <summary>
/// Provides platform detection and OS-specific path helpers.
/// </summary>
public static class PlatformService
{
    /// <summary>Whether the current OS is Windows.</summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>Whether the current OS is macOS.</summary>
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>Whether the current OS is Linux.</summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>Returns the name of the current operating system.</summary>
    public static string OsName =>
        IsWindows ? "Windows" :
        IsMacOS ? "macOS" :
        IsLinux ? "Linux" :
        "Unknown";

    /// <summary>
    /// Returns the default mount base directory for non-Windows platforms.
    /// On macOS: /Volumes, on Linux: /mnt
    /// </summary>
    public static string MountBaseDir =>
        IsMacOS ? "/Volumes" : "/mnt";
}

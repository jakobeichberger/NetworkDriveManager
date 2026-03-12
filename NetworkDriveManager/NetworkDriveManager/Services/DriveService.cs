using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace NetworkDriveManager.Services;

/// <summary>
/// Provides network drive operations (connect, disconnect, status check)
/// with cross-platform support for Windows, macOS, and Linux.
/// </summary>
public static class DriveService
{
    /// <summary>
    /// Returns the mount point path for a given drive letter on the current OS.
    /// Windows: "H:" — macOS/Linux: "/Volumes/H" or "/mnt/H"
    /// </summary>
    public static string GetMountPoint(string letter)
    {
        if (PlatformService.IsWindows)
            return $"{letter}:";
        return Path.Combine(PlatformService.MountBaseDir, letter);
    }

    /// <summary>
    /// Check whether a drive letter / mount point is currently mapped.
    /// </summary>
    public static bool IsDriveConnected(string letter)
    {
        try
        {
            if (PlatformService.IsWindows)
            {
                var result = RunCommand("net", $"use {letter}:");
                var connected = result.ExitCode == 0;
                LogService.Debug($"Drive {letter}: connected={connected}");
                return connected;
            }
            else
            {
                var mountPoint = GetMountPoint(letter);
                if (!Directory.Exists(mountPoint))
                    return false;
                var result = RunCommand("mount", "");
                var connected = result.Output.Contains(mountPoint);
                LogService.Debug($"Drive {letter} ({mountPoint}): connected={connected}");
                return connected;
            }
        }
        catch (Exception)
        {
            LogService.Debug($"Drive {letter}: check failed or timed out");
            return false;
        }
    }

    /// <summary>
    /// Return (connected, remoteUncPath) for a drive.
    /// </summary>
    public static (bool Connected, string? RemotePath) GetDriveInfo(string letter)
    {
        try
        {
            if (PlatformService.IsWindows)
            {
                var result = RunCommand("net", $"use {letter}:");
                if (result.ExitCode != 0)
                    return (false, null);

                var match = Regex.Match(result.Output, @"(\\\\[^\r\n]+)");
                var remote = match.Success ? match.Groups[1].Value.TrimEnd() : null;
                return (true, remote);
            }
            else
            {
                var mountPoint = GetMountPoint(letter);
                if (!Directory.Exists(mountPoint))
                    return (false, null);

                var result = RunCommand("mount", "");
                // macOS: //user@server/share on /Volumes/X (smbfs, ...)
                // Linux: //server/share on /mnt/X type cifs (...)
                var escapedMount = Regex.Escape(mountPoint);
                var match = Regex.Match(result.Output, $@"(//[^\s]+)\s+on\s+{escapedMount}\s");
                if (!match.Success)
                    return (false, null);

                var remotePath = match.Groups[1].Value;
                // Convert //server/share to \\server\share for consistency
                var uncPath = remotePath.Replace("/", @"\");
                // Remove user@ prefix if present (macOS format: //user@server/share)
                uncPath = Regex.Replace(uncPath, @"^\\\\[^@]+@", @"\\");
                return (true, uncPath);
            }
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    /// <summary>
    /// Map a network drive / mount a share.
    /// </summary>
    public static (bool Success, string Message) ConnectDrive(
        string letter, string server, string share, string username, string password)
    {
        LogService.Info($"Connecting drive {letter} to \\\\{server}\\{share} as user '{username}' on {PlatformService.OsName}");

        try
        {
            if (PlatformService.IsWindows)
            {
                var uncPath = $@"\\{server}\{share}";
                var result = RunCommand("net", $"use {letter}: {uncPath} /user:{username} {password}", timeout: 30);
                var msg = !string.IsNullOrWhiteSpace(result.Output) ? result.Output
                        : !string.IsNullOrWhiteSpace(result.Error) ? result.Error : string.Empty;

                if (result.ExitCode == 0)
                    LogService.Info($"Successfully connected drive {letter}:");
                else
                    LogService.Error($"Failed to connect drive {letter}: {msg}");

                return (result.ExitCode == 0, msg.Trim());
            }
            else if (PlatformService.IsMacOS)
            {
                var mountPoint = GetMountPoint(letter);
                Directory.CreateDirectory(mountPoint);

                var smbUrl = $"//{username}:{password}@{server}/{share}";
                var result = RunCommand("mount", $"-t smbfs {smbUrl} {mountPoint}", timeout: 30);
                var msg = !string.IsNullOrWhiteSpace(result.Output) ? result.Output
                        : !string.IsNullOrWhiteSpace(result.Error) ? result.Error : string.Empty;

                if (result.ExitCode == 0)
                    LogService.Info($"Successfully mounted {letter} at {mountPoint}");
                else
                    LogService.Error($"Failed to mount {letter}: {msg}");

                return (result.ExitCode == 0, msg.Trim());
            }
            else // Linux
            {
                var mountPoint = GetMountPoint(letter);
                Directory.CreateDirectory(mountPoint);

                var result = RunCommand("mount", $"-t cifs //{server}/{share} {mountPoint} -o username={username},password={password}", timeout: 30);
                var msg = !string.IsNullOrWhiteSpace(result.Output) ? result.Output
                        : !string.IsNullOrWhiteSpace(result.Error) ? result.Error : string.Empty;

                if (result.ExitCode == 0)
                    LogService.Info($"Successfully mounted {letter} at {mountPoint}");
                else
                    LogService.Error($"Failed to mount {letter}: {msg}");

                return (result.ExitCode == 0, msg.Trim());
            }
        }
        catch (TimeoutException)
        {
            LogService.Error($"Connection to {letter} timed out");
            return (false, "Connection timed out.");
        }
    }

    /// <summary>
    /// Disconnect / unmount a mapped network drive.
    /// </summary>
    public static (bool Success, string Message) DisconnectDrive(string letter)
    {
        LogService.Info($"Disconnecting drive {letter} on {PlatformService.OsName}");

        try
        {
            if (PlatformService.IsWindows)
            {
                var result = RunCommand("net", $"use {letter}: /delete /yes", timeout: 30);
                var msg = !string.IsNullOrWhiteSpace(result.Output) ? result.Output
                        : !string.IsNullOrWhiteSpace(result.Error) ? result.Error : string.Empty;

                if (result.ExitCode == 0)
                    LogService.Info($"Successfully disconnected drive {letter}:");
                else
                    LogService.Error($"Failed to disconnect drive {letter}: {msg}");

                return (result.ExitCode == 0, msg.Trim());
            }
            else
            {
                var mountPoint = GetMountPoint(letter);
                var result = RunCommand("umount", mountPoint, timeout: 30);
                var msg = !string.IsNullOrWhiteSpace(result.Output) ? result.Output
                        : !string.IsNullOrWhiteSpace(result.Error) ? result.Error : string.Empty;

                if (result.ExitCode == 0)
                {
                    LogService.Info($"Successfully unmounted {letter} from {mountPoint}");
                    // Clean up empty mount point directory
                    try { if (Directory.Exists(mountPoint)) Directory.Delete(mountPoint); } catch { }
                }
                else
                    LogService.Error($"Failed to unmount {letter}: {msg}");

                return (result.ExitCode == 0, msg.Trim());
            }
        }
        catch (TimeoutException)
        {
            LogService.Error($"Disconnect of {letter} timed out");
            return (false, "Disconnect timed out.");
        }
    }

    /// <summary>
    /// Parse 'net use' lines from a batch file and return (drives, skippedCount).
    /// </summary>
    public static (List<Models.DriveConfig> Drives, int Skipped) ParseBatDrives(string content)
    {
        var pattern = new Regex(
            @"^\s*net\s+use\s+([A-Za-z]):?\s+\\\\([^\\]+)\\(\S+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        var netUseLines = content.Split('\n')
            .Where(l => Regex.IsMatch(l.Trim(), @"^net\s+use\b", RegexOptions.IgnoreCase))
            .ToList();

        var drives = new List<Models.DriveConfig>();
        var seenLetters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in pattern.Matches(content))
        {
            var letter = match.Groups[1].Value.ToUpper();
            var server = match.Groups[2].Value.Trim();
            var share = match.Groups[3].Value.Trim();

            if (seenLetters.Contains(letter))
            {
                LogService.Warning($"Duplicate drive letter '{letter}' in import file — keeping first");
                continue;
            }

            seenLetters.Add(letter);
            var label = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(share.Replace("_", " ").Replace("-", " "));

            drives.Add(new Models.DriveConfig
            {
                Letter = letter,
                Server = server,
                Share = share,
                Label = label,
                Hidden = false,
            });
        }

        var skipped = Math.Max(0, netUseLines.Count - drives.Count);
        LogService.Info($"Parsed {drives.Count} drive(s) from import ({skipped} line(s) skipped)");
        return (drives, skipped);
    }

    private static (int ExitCode, string Output, string Error) RunCommand(string fileName, string arguments, int timeout = 10)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(timeout * 1000))
        {
            try { process.Kill(true); } catch { /* ignore */ }
            throw new TimeoutException($"Command '{fileName}' timed out after {timeout}s");
        }

        return (process.ExitCode, output, error);
    }
}

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NetworkDriveManager.Services;

/// <summary>
/// Provides network drive operations (connect, disconnect, status check)
/// via the Windows 'net use' command.
/// </summary>
public static class DriveService
{
    /// <summary>
    /// Check whether a drive letter is currently mapped.
    /// </summary>
    public static bool IsDriveConnected(string letter)
    {
        try
        {
            var result = RunNetUse($"{letter}:");
            var connected = result.ExitCode == 0;
            LogService.Debug($"Drive {letter}: connected={connected}");
            return connected;
        }
        catch (Exception)
        {
            LogService.Debug($"Drive {letter}: check failed or timed out");
            return false;
        }
    }

    /// <summary>
    /// Return (connected, remoteUncPath) in one 'net use' call.
    /// </summary>
    public static (bool Connected, string? RemotePath) GetDriveInfo(string letter)
    {
        try
        {
            var result = RunNetUse($"{letter}:");
            if (result.ExitCode != 0)
                return (false, null);

            var match = Regex.Match(result.Output, @"(\\\\[^\r\n]+)");
            var remote = match.Success ? match.Groups[1].Value.TrimEnd() : null;
            return (true, remote);
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    /// <summary>
    /// Map a network drive using 'net use'.
    /// </summary>
    public static (bool Success, string Message) ConnectDrive(
        string letter, string server, string share, string username, string password)
    {
        var uncPath = $@"\\{server}\{share}";
        LogService.Info($"Connecting drive {letter}: to {uncPath} as user '{username}'");

        try
        {
            var result = RunNetUse($"{letter}: {uncPath} /user:{username} {password}", timeout: 30);
            var msg = !string.IsNullOrWhiteSpace(result.Output) ? result.Output
                    : !string.IsNullOrWhiteSpace(result.Error) ? result.Error : string.Empty;

            if (result.ExitCode == 0)
                LogService.Info($"Successfully connected drive {letter}:");
            else
                LogService.Error($"Failed to connect drive {letter}: {msg}");

            return (result.ExitCode == 0, msg.Trim());
        }
        catch (TimeoutException)
        {
            LogService.Error($"Connection to {letter}: timed out");
            return (false, "Connection timed out.");
        }
    }

    /// <summary>
    /// Disconnect a mapped network drive.
    /// </summary>
    public static (bool Success, string Message) DisconnectDrive(string letter)
    {
        LogService.Info($"Disconnecting drive {letter}:");

        try
        {
            var result = RunNetUse($"{letter}: /delete /yes", timeout: 30);
            var msg = !string.IsNullOrWhiteSpace(result.Output) ? result.Output
                    : !string.IsNullOrWhiteSpace(result.Error) ? result.Error : string.Empty;

            if (result.ExitCode == 0)
                LogService.Info($"Successfully disconnected drive {letter}:");
            else
                LogService.Error($"Failed to disconnect drive {letter}: {msg}");

            return (result.ExitCode == 0, msg.Trim());
        }
        catch (TimeoutException)
        {
            LogService.Error($"Disconnect of {letter}: timed out");
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

    private static (int ExitCode, string Output, string Error) RunNetUse(string arguments, int timeout = 10)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "net",
            Arguments = $"use {arguments}",
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
            throw new TimeoutException($"net use command timed out after {timeout}s");
        }

        return (process.ExitCode, output, error);
    }
}

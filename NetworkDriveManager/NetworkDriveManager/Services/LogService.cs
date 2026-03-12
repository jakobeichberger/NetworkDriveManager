using System.Globalization;
using System.IO;

namespace NetworkDriveManager.Services;

/// <summary>
/// Simple file-based logging service mirroring the Python version's rotating log approach.
/// </summary>
public static class LogService
{
    private static readonly object _lock = new();
    private static readonly string _logFile = ConfigService.LogFilePath;
    private const long MaxFileSize = 1_000_000; // 1 MB
    private const int BackupCount = 3;

    /// <summary>Log a message at DEBUG level.</summary>
    public static void Debug(string message) => Write("DEBUG", message);
    /// <summary>Log a message at INFO level.</summary>
    public static void Info(string message) => Write("INFO", message);
    /// <summary>Log a message at WARNING level.</summary>
    public static void Warning(string message) => Write("WARNING", message);
    /// <summary>Log a message at ERROR level.</summary>
    public static void Error(string message) => Write("ERROR", message);

    /// <summary>
    /// Write a timestamped log entry to the log file, rotating if necessary.
    /// </summary>
    private static void Write(string level, string message)
    {
        lock (_lock)
        {
            try
            {
                RotateIfNeeded();
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                var line = $"{timestamp} [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFile, line);
            }
            catch
            {
                // Logging should never crash the app
            }
        }
    }

    /// <summary>
    /// Rotate the log file when it exceeds <see cref="MaxFileSize"/>, keeping up to
    /// <see cref="BackupCount"/> backup copies.
    /// </summary>
    private static void RotateIfNeeded()
    {
        try
        {
            if (!File.Exists(_logFile))
                return;

            var info = new FileInfo(_logFile);
            if (info.Length < MaxFileSize)
                return;

            // Rotate: delete oldest backup, shift others
            var oldest = $"{_logFile}.{BackupCount}";
            if (File.Exists(oldest))
                File.Delete(oldest);

            for (int i = BackupCount - 1; i >= 1; i--)
            {
                var src = $"{_logFile}.{i}";
                var dst = $"{_logFile}.{i + 1}";
                if (File.Exists(src))
                    File.Move(src, dst);
            }

            File.Move(_logFile, $"{_logFile}.1");
        }
        catch
        {
            // Rotation failure is non-critical
        }
    }

    /// <summary>
    /// Read ERROR and WARNING lines from all log files (backups + current).
    /// </summary>
    public static List<string> ReadErrorWarningEntries()
    {
        var lines = new List<string>();

        // Read backup files oldest-first
        for (int i = BackupCount; i >= 1; i--)
        {
            var path = $"{_logFile}.{i}";
            if (File.Exists(path))
                lines.AddRange(ReadFilteredLines(path));
        }

        // Current log
        if (File.Exists(_logFile))
            lines.AddRange(ReadFilteredLines(_logFile));

        return lines;
    }

    /// <summary>
    /// Read lines containing [ERROR] or [WARNING] from a single log file.
    /// </summary>
    private static IEnumerable<string> ReadFilteredLines(string path)
    {
        string[] fileLines;
        try
        {
            fileLines = File.ReadAllLines(path);
        }
        catch
        {
            yield break;
        }

        foreach (var line in fileLines)
        {
            if (line.Contains("[ERROR]") || line.Contains("[WARNING]"))
                yield return line;
        }
    }

    /// <summary>
    /// Clear all log files.
    /// </summary>
    public static void ClearLog()
    {
        lock (_lock)
        {
            for (int i = BackupCount; i >= 1; i--)
            {
                var path = $"{_logFile}.{i}";
                try { if (File.Exists(path)) File.Delete(path); } catch { }
            }

            try
            {
                File.WriteAllText(_logFile, string.Empty);
            }
            catch { }

            Info("Log file cleared by user");
        }
    }
}

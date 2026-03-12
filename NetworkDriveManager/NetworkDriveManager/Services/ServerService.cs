using System.Net.Sockets;

namespace NetworkDriveManager.Services;

/// <summary>
/// Provides server reachability checks via TCP socket probing (SMB port 445).
/// </summary>
public static class ServerService
{
    /// <summary>
    /// Check whether a server is reachable on the given port (default: SMB 445).
    /// </summary>
    public static bool IsServerReachable(string server, int port = 445, int timeoutMs = 3000)
    {
        try
        {
            using var client = new TcpClient();
            var result = client.BeginConnect(server, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(timeoutMs);

            if (success && client.Connected)
            {
                client.EndConnect(result);
                LogService.Debug($"Server {server}:{port} is reachable");
                return true;
            }

            LogService.Warning($"Server {server}:{port} is not reachable (timeout)");
            return false;
        }
        catch (Exception ex)
        {
            LogService.Warning($"Server {server}:{port} is not reachable: {ex.Message}");
            return false;
        }
    }
}

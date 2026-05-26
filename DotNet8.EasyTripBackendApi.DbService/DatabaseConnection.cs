using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Npgsql;

namespace DotNet8.EasyTripBackendApi.DbService;

/// <summary>
/// Resolves database hostnames for Npgsql. Supabase direct hosts are often IPv6-only;
/// on some Windows networks .NET DNS fails while nslookup still returns the address.
/// </summary>
public static class DatabaseConnection
{
    public static string Resolve(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var host = builder.Host?.Trim('[', ']');
        if (string.IsNullOrWhiteSpace(host) || IPAddress.TryParse(host, out _))
            return builder.ConnectionString;

        // Supabase session pooler hostnames resolve over IPv4; do not rewrite them.
        if (host.Contains("pooler.supabase.com", StringComparison.OrdinalIgnoreCase))
            return builder.ConnectionString;

        var ip = ResolveHostAddress(host)
                 ?? throw new InvalidOperationException(
                     $"Cannot resolve database host '{host}'. Use the Session pooler connection string from Supabase Dashboard in appsettings.json, or set EASYTRIP_CONNECTION_STRING.");

        builder.Host = ip.ToString();
        return builder.ConnectionString;
    }

    private static IPAddress? ResolveHostAddress(string host)
    {
        var families = host.Contains("supabase.co", StringComparison.OrdinalIgnoreCase)
            ? new[] { AddressFamily.InterNetworkV6, AddressFamily.InterNetwork, AddressFamily.Unspecified }
            : new[] { AddressFamily.Unspecified, AddressFamily.InterNetwork, AddressFamily.InterNetworkV6 };

        foreach (var family in families)
        {
            try
            {
                var addresses = family == AddressFamily.Unspecified
                    ? Dns.GetHostAddresses(host)
                    : Dns.GetHostAddresses(host, family);

                var ip = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                         ?? addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6);
                if (ip != null)
                    return ip;
            }
            catch (SocketException)
            {
                // Try next address family.
            }
        }

        return OperatingSystem.IsWindows() ? ResolveViaNsLookup(host) : null;
    }

    private static IPAddress? ResolveViaNsLookup(string host)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "nslookup",
                Arguments = host,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process == null)
                return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].TrimStart().StartsWith("Name:", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 >= lines.Length)
                    break;

                var addrLine = lines[i + 1].Trim();
                if (!addrLine.StartsWith("Address:", StringComparison.OrdinalIgnoreCase))
                    continue;

                var addr = addrLine["Address:".Length..].Trim();
                if (IPAddress.TryParse(addr, out var ip))
                    return ip;
            }
        }
        catch
        {
            // Ignore and let caller surface a clear configuration error.
        }

        return null;
    }
}

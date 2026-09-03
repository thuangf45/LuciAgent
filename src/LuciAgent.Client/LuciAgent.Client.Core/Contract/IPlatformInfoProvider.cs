using LuciferCore.Attributes;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace LuciAgent.Client.Core.Contract;

public interface IPlatformInfoProvider
{
    string GetHostName();
    string GetOsDescription();
    string GetLocalIPv4();
    string GetLanId();             // Get LAN ID (SSID for Wi-Fi, or card name for wired)
    string GetPersistentAgentId(); // Persisted ID per app install
}

[Singleton(ServiceType = typeof(IPlatformInfoProvider), Order = 0)]
public sealed class DefaultPlatformInfoProvider : IPlatformInfoProvider
{
    private readonly string _agentIdPath;
    private readonly object _idLock = new();
    private string? _cachedAgentGuid;

    public DefaultPlatformInfoProvider()
    {
        _agentIdPath = NormalizeAgentIdPath("LuciAgent");
    }

    public string GetHostName()
    {
        try
        {
#if ANDROID || IOS
            // Mobile devices may not have a proper hostname, so we fallback to MachineName or a default string
            return Environment.MachineName is { Length: > 0 } m ? m : "MobileDevice";
#else
            return Dns.GetHostName();
#endif
        }
        catch
        {
            try { return Environment.MachineName; } catch { return "UnknownHost"; }
        }
    }

    public string GetOsDescription()
    {
        try { return RuntimeInformation.OSDescription; }
        catch
        {
#if ANDROID
            return "Android";
#elif IOS
            return "iOS";
#elif MACCATALYST
            return "MacCatalyst";
#elif WINDOWS
            return "Windows";
#elif LINUX
            return "Linux";
#else
            return "UnknownOS";
#endif
        }
    }

    public string GetPersistentAgentId()
    {
        if (!string.IsNullOrWhiteSpace(_cachedAgentGuid)) return _cachedAgentGuid!;

        lock (_idLock)
        {
            if (!string.IsNullOrWhiteSpace(_cachedAgentGuid)) return _cachedAgentGuid!;

            try
            {
                var dir = Path.GetDirectoryName(_agentIdPath);
                if (string.IsNullOrWhiteSpace(dir))
                {
                    dir = GetSafeAppDataDirectory();
                }

                Directory.CreateDirectory(dir!);

                if (File.Exists(_agentIdPath))
                {
                    var existing = File.ReadAllText(_agentIdPath).Trim();
                    if (Guid.TryParse(existing, out _))
                    {
                        return _cachedAgentGuid = existing;
                    }
                }

                var id = Guid.NewGuid().ToString();
                File.WriteAllText(_agentIdPath, id);
                return _cachedAgentGuid = id;
            }
            catch
            {
                // Fallback deterministic ID based on device info if file write fails
                var seed = BuildDeviceSeed();
                return _cachedAgentGuid = DeterministicGuidFromString(seed);
            }
        }
    }

    public string GetLocalIPv4()
    {
        try
        {
            // 1) Get local IPv4 by creating a UDP socket and connecting to a public IP
            var byRoute = GetLocalIPv4ByRoute();
            if (!string.IsNullOrWhiteSpace(byRoute)) return byRoute;

            // 2) Scan the primary network interface for an IPv4 address
            var ni = GetPrimaryInterface();
            if (ni != null)
            {
                var ip = ni.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(a =>
                        a.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(a.Address))
                    ?.Address.ToString();

                if (!string.IsNullOrWhiteSpace(ip)) return ip;
            }

            // 3) Scan all network interfaces
            foreach (var n in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (n.OperationalStatus != OperationalStatus.Up) continue;
                if (n.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                foreach (var a in n.GetIPProperties().UnicastAddresses)
                {
                    if (a.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(a.Address))
                        return a.Address.ToString();
                }
            }
        }
        catch { }

        return "0.0.0.0";
    }

    public string GetLanId()
    {
        try
        {
            // 1) Try to get Wi-Fi SSID based on platform
#if ANDROID
            var ssid = GetAndroidWifiSSID();
            if (!string.IsNullOrWhiteSpace(ssid) && ssid != "<unknown ssid>") return ssid;
#elif IOS
            var ssid = GetIosWifiSSID();
            if (!string.IsNullOrWhiteSpace(ssid)) return ssid;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var ssid = GetWindowsWifiSSID();
                if (!string.IsNullOrWhiteSpace(ssid)) return ssid;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var ssid = GetLinuxWifiSSID();
                if (!string.IsNullOrWhiteSpace(ssid)) return ssid;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var ssid = GetMacWifiSSID();
                if (!string.IsNullOrWhiteSpace(ssid)) return ssid;
            }
#endif

            // 2) Fallback: If not Wi-Fi, use the primary network interface name
            var ni = GetPrimaryInterface();
            if (ni != null && !string.IsNullOrWhiteSpace(ni.Name))
            {
                return ni.Name; // VD: "Ethernet 3" in Windows, "en0" in macOS/Linux
            }
        }
        catch { }

        // 3) Fallback to a deterministic LAN ID based on device info
        var seed = BuildDeviceSeed();
        return "LAN-" + Sha256Hex(seed, 16);
    }

#if ANDROID
    private static string? GetAndroidWifiSSID()
    {
        try
        {
            // Requires ACCESS_FINE_LOCATION & ACCESS_WIFI_STATE in AndroidManifest.xml
            var context = Android.App.Application.Context;
            var wifiManager = (Android.Net.Wifi.WifiManager?)context.GetSystemService(Android.Content.Context.WifiService);
            var ssid = wifiManager?.ConnectionInfo?.SSID;
            
            if (!string.IsNullOrWhiteSpace(ssid) && ssid != "<unknown ssid>")
            {
                return ssid.Trim('"'); 
            }
        }
        catch { }
        return null;
    }
#endif

#if IOS
    private static string? GetIosWifiSSID()
    {
        try
        {
            // Requires "Access WiFi Information" entitlement and Location permission
            var dict = SystemConfiguration.CaptiveNetwork.CNCopyCurrentNetworkInfo("en0");
            if (dict != null && dict.TryGetValue(SystemConfiguration.CaptiveNetwork.NetworkInfoKeySSID, out var ssidObj))
            {
                return ssidObj?.ToString();
            }
        }
        catch { }
        return null;
    }
#endif

#if !ANDROID && !IOS
    private static string? GetWindowsWifiSSID()
    {
        try
        {
            var psi = new ProcessStartInfo("netsh", "wlan show interfaces")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains("SSID") && !line.Contains("BSSID"))
                {
                    var parts = line.Split(':');
                    if (parts.Length > 1)
                    {
                        string ssid = parts[1].Trim();
                        if (!string.IsNullOrWhiteSpace(ssid)) return ssid;
                    }
                }
            }
        }
        catch { }
        return null;
    }

    private static string? GetLinuxWifiSSID()
    {
        try
        {
            var psi = new ProcessStartInfo("iwgetid", "-r")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;

            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output)) return output;
        }
        catch { }
        return null;
    }

    private static string? GetMacWifiSSID()
    {
        try
        {
            var psi = new ProcessStartInfo("/System/Library/PrivateFrameworks/Apple80211.framework/Versions/Current/Resources/airport", "-I")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains("SSID:"))
                {
                    var parts = line.Split(':');
                    if (parts.Length > 1)
                    {
                        string ssid = parts[1].Trim();
                        if (!string.IsNullOrWhiteSpace(ssid)) return ssid;
                    }
                }
            }
        }
        catch { }
        return null;
    }
#endif

    private static string NormalizeAgentIdPath(string input)
    {
        if (!string.IsNullOrWhiteSpace(input)) return input;
        return Path.Combine(GetSafeAppDataDirectory(), "LuciAgent", "agent.id");
    }

    private static string GetSafeAppDataDirectory()
    {
        try
        {
#if ANDROID || IOS
            // app sandbox writable
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#else
            var p = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            if (!string.IsNullOrWhiteSpace(p)) return p;
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
        }
        catch
        {
            return AppContext.BaseDirectory;
        }
    }

    private static string BuildDeviceSeed()
    {
        var host = Safe(() => Environment.MachineName, "UnknownHost");
        var os = Safe(() => RuntimeInformation.OSDescription, "UnknownOS");
        var arch = Safe(() => RuntimeInformation.OSArchitecture.ToString(), "UnknownArch");
        var framework = Safe(() => RuntimeInformation.FrameworkDescription, "UnknownFx");

        return $"{host}|{os}|{arch}|{framework}";
    }

    private static string DeterministicGuidFromString(string text)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        return new Guid(hash[..16]).ToString();
    }

    private static string Sha256Hex(string text, int takeBytes)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash[..Math.Clamp(takeBytes, 4, 32)]);
    }

    private static string Safe(Func<string> func, string fallback)
    {
        try
        {
            var v = func();
            return string.IsNullOrWhiteSpace(v) ? fallback : v;
        }
        catch { return fallback; }
    }

    private static string? GetLocalIPv4ByRoute()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect("8.8.8.8", 65530);
            if (socket.LocalEndPoint is IPEndPoint ep &&
                ep.Address.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(ep.Address))
            {
                return ep.Address.ToString();
            }
        }
        catch { }
        return null;
    }

    private static NetworkInterface? GetPrimaryInterface()
    {
        try
        {
            var all = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .ToList();

            if (all.Count == 0) return null;

            // Filter interfaces that have a gateway (indicating they are connected to a network with internet access)
            var withGw = all.Where(ni =>
            {
                try { return ni.GetIPProperties().GatewayAddresses.Count > 0; }
                catch { return false; }
            });

            var sorted = withGw
                .OrderByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                .ThenByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .ToList();

            if (sorted.Count > 0) return sorted[0];

            // If no interfaces with a gateway, return the first available interface
            return all[0];
        }
        catch
        {
            return null;
        }
    }
}
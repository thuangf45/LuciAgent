using LuciAgent.Client.Core.Core;
using LuciferCore.Model;
using LuciferCore.Utf8;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LuciAgent.Client.Core.Contract;

public abstract class AgentBase
{
    public AgentBase(AgentClient client)
    {
        _client = client;
    }

    private readonly AgentClient _client;

    public readonly Dictionary<AgentIdentity, bool> OtherAgents = [];
    public virtual AgentIdentity GetIdentity()
    {
        var identity = Rent<AgentIdentity>();

        // Cross-platform reuseable code to get AgentID, OSVersion, LanIP, LanID, and WanIP
        identity.AgentID = System.Text.Encoding.UTF8.GetBytes(Dns.GetHostName());
        identity.OSVersion = System.Text.Encoding.UTF8.GetBytes(Environment.OSVersion.ToString());
        identity.LanIP = System.Text.Encoding.UTF8.GetBytes(GetLocalIPAddress());

        // Call the platform-specific method to get the LAN ID (SSID or network interface name)
        identity.LanID = System.Text.Encoding.UTF8.GetBytes(GetPlatformSpecificLanID());

        identity.WanIP = System.Text.Encoding.UTF8.GetBytes("0.0.0.0"); // Placeholder for WAN IP, can be updated later if needed

        return identity;
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
        }
        catch { }
        return "127.0.0.1";
    }

    protected virtual string GetPlatformSpecificLanID()
    {
        try
        {
            // 1. Wifi SSID detection based on OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string ssid = GetWindowsWifiSSID();
                if (!string.IsNullOrEmpty(ssid) && ssid != "Unknown") return ssid;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string ssid = GetLinuxWifiSSID();
                if (!string.IsNullOrEmpty(ssid) && ssid != "Unknown") return ssid;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string ssid = GetMacWifiSSID();
                if (!string.IsNullOrEmpty(ssid) && ssid != "Unknown") return ssid;
            }

            // 2. Fallback: Get the name of the first active network interface
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                    ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Tunnel)
                {
                    return ni.Name;
                }
            }
        }
        catch { }

        return "Unknown";
    }

    // --- CÁC HÀM TRỢ GIÚP LẤY SSID THEO TỪNG OS ---

    private string GetWindowsWifiSSID()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "wlan show interfaces",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
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
                        if (!string.IsNullOrEmpty(ssid)) return ssid;
                    }
                }
            }
        }
        catch { }
        return "Unknown";
    }

    private string GetLinuxWifiSSID()
    {
        try
        {
            // Use the iwgetid command to get the current Wi-Fi SSID on Linux
            var psi = new ProcessStartInfo
            {
                FileName = "iwgetid",
                Arguments = "-r",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output)) return output;
        }
        catch { }
        return "Unknown";
    }

    private string GetMacWifiSSID()
    {
        try
        {
            // Use the airport command to get the current Wi-Fi SSID on macOS
            var psi = new ProcessStartInfo
            {
                FileName = "/System/Library/PrivateFrameworks/Apple80211.framework/Versions/Current/Resources/airport",
                Arguments = "-I",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
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
                        if (!string.IsNullOrEmpty(ssid)) return ssid;
                    }
                }
            }
        }
        catch { }
        return "Unknown";
    }

    public virtual void JoinNetwork()
    {
        var identity = GetIdentity();
        EnsureConnected();

        using var request = Rent<RequestModel>();
        request.MakePostRequest<byte, byte>("/v1/api/agent/join"u8, identity.Buffer!);
    }

    private void EnsureConnected()
    {
        int retryCount = 0;
        while (_client.IsConnected == false && retryCount < 5)
        {
            _client.Connect();
            retryCount++;
            Thread.Sleep(1000); // Wait for 1 second before checking again
        }
    }
}

public partial class AgentIdentity : LayoutModel
{
    public AgentIdentity()
    {
        _totalFields = 5;
        Attach(Rent<Buffer>());
    }

    [LayoutIndex(0)]
    public partial ReadOnlySpan<byte> AgentID { get; set; }

    [LayoutIndex(1)]
    public partial ReadOnlySpan<byte> OSVersion { get; set; }

    [LayoutIndex(2)]
    public partial ReadOnlySpan<byte> LanIP { get; set; }

    [LayoutIndex(3)]
    public partial ReadOnlySpan<byte> LanID { get; set; }

    [LayoutIndex(4)]
    public partial ReadOnlySpan<byte> WanIP { get; set; }

    public override string ToString()
    {
        using var builder = Rent<Utf8Builder>()
            .Append("AgentID: "u8).Append(AgentID)
            .Append("\nOSVersion: "u8).Append(OSVersion)
            .Append("\nLanIP: "u8).Append(LanIP)
            .Append("\nLanID: "u8).Append(LanID)
            .Append("\nWanIP: "u8).Append(WanIP);
        return builder.ToString();
    }

    protected override void Reset()
    {
        base.Reset();
    }
}
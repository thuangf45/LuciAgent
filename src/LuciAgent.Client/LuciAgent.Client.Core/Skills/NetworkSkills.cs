using LuciferCore.Attributes;
using LuciferCore.Utf8;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace LuciAgent.Client.Core.Skills;

public static class NetworkSkills
{
    [ConsoleCommand("ping", "")]
    private static async Task Ping(params string[] args)
    {
        if (args.Length <= 1)
        {
            Warn("Help: ping help or ping ?"u8);
            return;
        }

        using var ping = new Ping();

        if (args.Length == 2)
        {
            var hostOrAddress = args[0];
            var count = int.TryParse(args[1], out int c) ? c : 4;

            IPAddress? targetIp = await ResolveIpAsync(hostOrAddress);
            if (targetIp == null)
            {
                Error<char>($"Could not resolve host: {hostOrAddress}");
                return;
            }

            await PingTargetAsync(ping, targetIp, count);
            return;
        }

        if (args.Length == 3)
        {
            var startHost = args[0];
            var endHost = args[1];
            var count = int.TryParse(args[2], out int c) ? c : 1;

            if (!IPAddress.TryParse(startHost, out var startIp) || !IPAddress.TryParse(endHost, out var endIp))
            {
                Error("Invalid start or end IP address."u8);
                return;
            }

            if (startIp.AddressFamily != AddressFamily.InterNetwork || endIp.AddressFamily != AddressFamily.InterNetwork)
            {
                Error("Range scanning is only supported for IPv4 addresses."u8);
                return;
            }

            uint startUint = IpToUint(startIp);
            uint endUint = IpToUint(endIp);

            if (startUint > endUint)
            {
                Error("Start IP must be less than or equal to end IP."u8);
                return;
            }

            for (uint current = startUint; current <= endUint; current++)
            {
                var targetIp = UintToIp(current);
                await PingTargetAsync(ping, targetIp, count);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task<IPAddress?> ResolveIpAsync(string hostOrAddress)
    {
        if (IPAddress.TryParse(hostOrAddress, out var ip))
            return ip;

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(hostOrAddress);
            return addresses.Length > 0 ? addresses[0] : null;
        }
        catch
        {
            return null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task PingTargetAsync(Ping ping, IPAddress targetIp, int count)
    {
        for (int i = 0; i < count; i++)
        {
            try
            {
                PingReply reply = await ping.SendPingAsync(targetIp, 1000);
                PrintReplyCore(reply, targetIp);
            }
            catch (Exception ex)
            {
                using var b = Rent<Utf8Builder>();
                b.Append("Ping error to "u8).Append(targetIp).Append(": "u8).Append<char>(ex.Message);
                Error(b.Span);
            }
        }
    }

    [ConsoleCommand("ping help", "")]
    [ConsoleCommand("ping ?", "")]
    private static void PingHelp()
    {
        Info("Usage: ping <host> <count>"u8);
        Info("       ping <start_ip> <end_ip> <count>"u8);
        Info("Example: ping 192.168.1.1 4"u8);
        Info("         ping 2001:4860:4860::8888 4"u8);
        Info("         ping 192.168.1.1 192.168.1.100 1"u8);
    }

    private static void PrintReplyCore(PingReply reply, IPAddress fallbackIp)
    {
        using var builder = Rent<Utf8Builder>();

        var displayIp = (reply.Status == IPStatus.Success && reply.Address != null &&
                         !reply.Address.Equals(IPAddress.Any) && !reply.Address.Equals(IPAddress.IPv6Any))
            ? reply.Address
            : fallbackIp;

        builder.Append("Reply from "u8).Append(displayIp);

        if (reply.Status == IPStatus.Success)
        {
            int bytes = reply.Buffer?.Length ?? 32;
            int ttl = reply.Options?.Ttl ?? 0;

            builder.Append(": bytes="u8).Append(bytes)
                .Append(" time="u8).Append(reply.RoundtripTime)
                .Append("ms TTL="u8).Append(ttl);

            Info(builder.Span);
        }
        else
        {
            builder.Append(": "u8).Append<char>(reply.Status.ToString());
            Error(builder.Span);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint IpToUint(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IPAddress UintToIp(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return new IPAddress(bytes);
    }

    [ConsoleCommand("tcpping", "")]
    private static async Task TcpPing(params string[] args)
    {
        if (args.Length < 2)
        {
            Warn("Help: tcpping help or tcpping ?"u8);
            return;
        }

        var host = args[0];

        for (int i = 1; i < args.Length; i++)
        {
            if (int.TryParse(args[i], out int port))
            {
                await CheckPortCoreAsync(host, port, 2000);
            }
            else
            {
                Error<char>($"Invalid port format: {args[i]}");
            }
        }
    }

    [ConsoleCommand("tcpping scan", "")]
    private static async Task TcpPingScan(params string[] args)
    {
        if (args.Length < 3)
        {
            Warn("Help: tcpping help or tcpping ?"u8);
            return;
        }

        var host = args[0];
        if (!int.TryParse(args[1], out int startPort) || !int.TryParse(args[2], out int endPort))
        {
            Error("Invalid start or end port."u8);
            return;
        }

        int timeoutMs = 2000;
        if (args.Length >= 4 && int.TryParse(args[3], out int parsedTimeout))
        {
            timeoutMs = parsedTimeout;
        }

        if (startPort > endPort || startPort <= 0 || endPort > 65535)
        {
            Error("Invalid port range. Ports must be between 1 and 65535."u8);
            return;
        }

        Info<char>($"Scanning {host} from port {startPort} to {endPort}...");
        for (int port = startPort; port <= endPort; port++)
        {
            await CheckPortCoreAsync(host, port, timeoutMs);
        }
    }

    [ConsoleCommand("tcpping help", "")]
    [ConsoleCommand("tcpping ?", "")]
    private static void TcpPingHelp()
    {
        Info("Usage: tcpping <host> <port1> [port2] [port3] ..."u8);
        Info("       tcpping scan <host> <start_port> <end_port> [timeoutMs]"u8);
        Info("Example: tcpping 192.168.1.1 80 443 8080"u8);
        Info("         tcpping scan 192.168.1.1 1 1024 1000"u8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task CheckPortCoreAsync(string host, int port, int timeoutMs)
    {
        using var client = new TcpClient();
        using var cts = new CancellationTokenSource(timeoutMs);
        var sw = Stopwatch.StartNew();
        bool isOpen = false;
        string? errorMsg = null;

        try
        {
            await client.ConnectAsync(host, port, cts.Token);
            isOpen = true;
        }
        catch (OperationCanceledException)
        {
            errorMsg = "TimedOut";
        }
        catch
        {
            errorMsg = "Closed";
        }
        finally
        {
            sw.Stop();
        }

        using var builder = Rent<Utf8Builder>();
        builder.Append("Reply from "u8).Append<char>(host)
               .Append(":"u8).Append(port);

        if (isOpen)
        {
            builder.Append(" - status=Open time="u8).Append(sw.ElapsedMilliseconds).Append("ms"u8);
            Info(builder.Span);
        }
        else
        {
            builder.Append(" - status="u8).Append<char>(errorMsg);
            Error(builder.Span);
        }
    }
}
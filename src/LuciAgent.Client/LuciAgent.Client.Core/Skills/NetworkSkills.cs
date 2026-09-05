using LuciferCore.Attributes;
using LuciferCore.Utf8;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LuciAgent.Client.Core.Skills;

public static class NetworkSkills
{
    // ==========================================
    // ICMP PING
    // ==========================================
    [ConsoleCommand("ping", "")]
    private static void Ping(params string[] args)
    {
        if (args.Length <= 1)
        {
            Warn<char>("Help: ping help or ping ?");
            return;
        }

        using var ping = new Ping();

        if (args.Length == 2)
        {
            var hostOrAddress = args[0];
            var count = int.TryParse(args[1], out int result) ? result : 4;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    PingReply reply = ping.SendPingAsync(hostOrAddress, 1000).Result;

                    string displayIp = (reply.Status == IPStatus.Success && reply.Address != null && !reply.Address.Equals(IPAddress.Any))
                        ? reply.Address.ToString()
                        : hostOrAddress;

                    PrintReplyCore(reply, displayIp);
                }
                catch (Exception ex)
                {
                    Error<char>($"Ping error to {hostOrAddress}: {ex.Message}");
                }
            }
            return;
        }

        if (args.Length == 3)
        {
            var startHost = args[0];
            var endHost = args[1];
            var count = int.TryParse(args[2], out int result) ? result : 1;

            if (!IPAddress.TryParse(startHost, out var startIp) || !IPAddress.TryParse(endHost, out var endIp))
            {
                Error<char>("Invalid start or end IP address.");
                return;
            }

            uint startUint = IpToUint(startIp);
            uint endUint = IpToUint(endIp);

            if (startUint > endUint)
            {
                Error<char>("Start IP must be less than or equal to end IP.");
                return;
            }

            Span<char> ipBuf = stackalloc char[45];

            for (uint current = startUint; current <= endUint; current++)
            {
                var targetIp = UintToIp(current);
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        PingReply reply = ping.SendPingAsync(targetIp, 1000).Result;

                        var ipToDisplay = (reply.Status == IPStatus.Success && reply.Address != null && !reply.Address.Equals(IPAddress.Any))
                            ? reply.Address
                            : targetIp;

                        ipToDisplay.TryFormat(ipBuf, out int written);

                        PrintReplyCore(reply, ipBuf.Slice(0, written));
                    }
                    catch (Exception ex)
                    {
                        Error<char>($"Ping error to {targetIp}: {ex.Message}");
                    }
                }
            }
        }
    }

    [ConsoleCommand("ping help", "")]
    [ConsoleCommand("ping ?", "")]
    private static void PingHelp()
    {
        Info<char>("Usage: ping <host> <count>");
        Info<char>("       ping <start_ip> <end_ip> <count>");
        Info<char>("Example: ping 192.168.1.1 4");
        Info<char>("         ping 192.168.1.1 192.168.1.100 1");
    }

    private static void PrintReplyCore(PingReply reply, ReadOnlySpan<char> displayIp)
    {
        using var builder = Rent<Utf8Builder>();

        if (reply.Status == IPStatus.Success)
        {
            int bytes = reply.Buffer?.Length ?? 32;
            int ttl = reply.Options?.Ttl ?? 0;

            builder.Append("Reply from "u8).Append<char>(displayIp)
                .Append(": bytes="u8).Append(bytes)
                .Append(" time="u8).Append(reply.RoundtripTime)
                .Append("ms TTL="u8).Append(ttl);

            Info(builder.Span);
        }
        else
        {
            builder.Append("Reply from "u8).Append<char>(displayIp)
                .Append(": "u8).Append<char>(reply.Status.ToString());

            Error(builder.Span);
        }
    }

    private static uint IpToUint(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private static IPAddress UintToIp(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return new IPAddress(bytes);
    }

    // ==========================================
    // TCP PING & PORT SCANNER
    // ==========================================
    [ConsoleCommand("tcpping", "")]
    private static void TcpPing(params string[] args)
    {
        if (args.Length < 2)
        {
            Warn<char>("Help: tcpping help or tcpping ?");
            return;
        }

        var host = args[0];

        for (int i = 1; i < args.Length; i++)
        {
            if (int.TryParse(args[i], out int port))
            {
                CheckPortCore(host, port, 2000);
            }
            else
            {
                Error<char>($"Invalid port format: {args[i]}");
            }
        }
    }

    [ConsoleCommand("tcpping scan", "")]
    private static void TcpPingScan(params string[] args)
    {
        if (args.Length < 3)
        {
            Warn<char>("Help: tcpping help or tcpping ?");
            return;
        }

        var host = args[0];
        if (!int.TryParse(args[1], out int startPort) || !int.TryParse(args[2], out int endPort))
        {
            Error<char>("Invalid start or end port.");
            return;
        }

        int timeoutMs = 2000;
        if (args.Length >= 4 && int.TryParse(args[3], out int parsedTimeout))
        {
            timeoutMs = parsedTimeout;
        }

        if (startPort > endPort || startPort <= 0 || endPort > 65535)
        {
            Error<char>("Invalid port range. Ports must be between 1 and 65535.");
            return;
        }

        Info<char>($"Scanning {host} from port {startPort} to {endPort}...");
        for (int port = startPort; port <= endPort; port++)
        {
            CheckPortCore(host, port, timeoutMs);
        }
    }

    [ConsoleCommand("tcpping help", "")]
    [ConsoleCommand("tcpping ?", "")]
    private static void TcpPingHelp()
    {
        Info<char>("Usage: tcpping <host> <port1> [port2] [port3] ...");
        Info<char>("       tcpping scan <host> <start_port> <end_port> [timeoutMs]");
        Info<char>("Example: tcpping 192.168.1.1 80 443 8080");
        Info<char>("         tcpping scan 192.168.1.1 1 1024 1000");
    }

    private static void CheckPortCore(string host, int port, int timeoutMs)
    {
        using var client = new TcpClient();
        var sw = Stopwatch.StartNew();
        bool isOpen = false;
        string? errorMsg = null;

        try
        {
            var connectTask = client.ConnectAsync(host, port);
            if (connectTask.Wait(timeoutMs))
            {
                isOpen = true;
            }
            else
            {
                errorMsg = "TimedOut";
            }
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
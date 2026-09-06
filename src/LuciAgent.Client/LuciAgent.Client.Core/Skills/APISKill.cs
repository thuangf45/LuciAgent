using LuciAgent.Client.Core.Core;
using LuciferCore.Attributes;
using LuciferCore.Model;
using LuciferCore.NetCoreServer.Transport.SSL;
using LuciferCore.Utf8;
using System.Security.Authentication;

namespace LuciAgent.Client.Core.Skills;

public class APISKill
{
    [ConsoleCommand("api")]
    private static void API(params string[] args) => ExecuteRequest(isSecure: false, args);

    [ConsoleCommand("api-secure")]
    private static void APISecure(params string[] args) => ExecuteRequest(isSecure: true, args);

    private static void ExecuteRequest(bool isSecure, string[] args)
    {
        if (args.Length < 2)
        {
            Warn(isSecure ? "Help: api-secure help or api-secure ?"u8 : "Help: api help or api ?"u8);
            return;
        }

        var method = args[0].ToUpperInvariant();
        var rawUrl = args[1];

        var defaultScheme = isSecure ? "https://" : "http://";
        var url = rawUrl.Contains("://", StringComparison.OrdinalIgnoreCase)
            ? rawUrl
            : string.Concat(defaultScheme, rawUrl);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            Warn("Invalid URL"u8);
            return;
        }

        var host = uri.Host;
        var port = uri.Port;
        var pathAndQuery = uri.PathAndQuery;

        using var request = Rent<RequestModel>();
        request.SetBegin<char, char>(method, pathAndQuery);

        int state = 0;
        using var bodyBuilder = Rent<Utf8Builder>();
        bool hasBody = false;

        for (int i = 2; i < args.Length; i++)
        {
            var token = args[i];

            if (token.Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                state = 1;
                continue;
            }

            if (token.Equals("-d", StringComparison.OrdinalIgnoreCase))
            {
                state = 2;
                continue;
            }

            if (state == 1)
            {
                if (TrySplitAt(token, ':', out var key, out var value))
                {
                    request.AddHeader(key, value);
                }
            }
            else if (state == 2)
            {
                if (hasBody)
                {
                    bodyBuilder.Append(" "u8);
                }
                bodyBuilder.Append<char>(token);
                hasBody = true;
            }
        }

        if (hasBody)
        {
            request.SetBody<byte>(bodyBuilder.Span);
        }

        if (isSecure)
        {
            var sslContext = new SslContext(SslProtocols.Tls12 | SslProtocols.Tls13);
            var client = new AgentsClient(sslContext, host, port);
            client.Connect();
            client.SendRequest(request);
        }
        else
        {
            var client = new AgentClient(host, port);
            client.Connect();
            client.SendRequest(request);
        }
    }

    [ConsoleCommand("api help")]
    [ConsoleCommand("api ?")]
    private static void APIHelp(params string[] args)
    {
        Info("API Command Help:"u8);
        Info("Usage: api <method> <url> [-h <header1:val1> <header2:val2> ...] [-d <body...>]"u8);
        Info("Methods: GET, POST, PUT, DELETE, HEAD, TRACE"u8);
        Info("Example: api GET google.com"u8);
        Info("         api POST /api/agent -h Content-Type:application/json Authorization:Bearer_token -d {\"name\":\"test\"}"u8);
        Info("         api POST /api/agent -d Hello World From LuciAgent -h Content-Type:text/plain"u8);
    }

    [ConsoleCommand("api-secure help")]
    [ConsoleCommand("api-secure ?")]
    private static void APISecureHelp(params string[] args)
    {
        Info("API Secure Command Help:"u8);
        Info("Usage: api-secure <method> <url> [-h <header1:val1> <header2:val2> ...] [-d <body...>]"u8);
        Info("Methods: GET, POST, PUT, DELETE, HEAD, TRACE"u8);
        Info("Example: api-secure GET google.com"u8);
        Info("         api-secure POST /api/agent -h Content-Type:application/json Authorization:Bearer_token -d {\"name\":\"test\"}"u8);
        Info("         api-secure POST /api/agent -d Hello World From LuciAgent -h Content-Type:text/plain"u8);
    }
}
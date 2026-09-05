using LuciferCore.Attributes;
using LuciferCore.NetCoreServer.Server;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace LuciAgent.Client.Core.Core;

[Server("App Server", 8080)]
public class AgentServer : WsServer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AgentServer(int port) : base(IPAddress.Any, AppPort != -1 ? AppPort : port)
    {
        ServerInfo.Options.OptionDualMode = true;
        ServerInfo.Options.OptionNoDelay = true;
        ServerInfo.Options.OptionReuseAddress = true;

        ServerInfo.Options.OptionKeepAlive = true;
        ServerInfo.Options.OptionTcpKeepAliveTime = 30;
        ServerInfo.Options.OptionTcpKeepAliveInterval = 10;
        ServerInfo.Options.OptionTcpKeepAliveRetryCount = 5;

        AddStaticContent(StaticPath);

        Mapping = new(true)
        {
            { "/","/index.html" },
        };
    }

    protected override AgentSession CreateSession() => new(this);

    [Config("WWW")]
    private static string StaticPath { get; set; } = string.Empty;

    [Config("PORT")]
    private static int AppPort { get; set; } = -1;

    private IPAddress? IPLocal = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

    private string UrlLocal => $"http://{IPLocal}:{ServerInfo.Port}";

    protected override void OnStarted()
    {
        Info<char>($"Started successfully and listening on {Endpoint}", this);
        Info<char>($"LAN URL: {UrlLocal}", this);
        Info<char>($"Localhost URL: http://localhost:{ServerInfo.Port}", this);
    }
}

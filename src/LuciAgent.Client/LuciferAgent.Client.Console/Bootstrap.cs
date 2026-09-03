global using HttpClient = LuciferCore.NetCoreServer.Client.HttpClient;
using LuciferCore.Attributes;
using LuciferCore.NetCoreServer.Client;
using LuciferCore.NetCoreServer.Transport.SSL;
using System.Security.Authentication;

namespace LuciAgent.Client.Console;

public static class Bootstrap
{
    [Config("Address", "127.0.0.1")]
    private static string Address { get; set; }

    [Config("Port", "23456")]
    private static int Port { get; set; }

    [Bootstrap]
    internal static void Initialize()
    {
        SetModelT<HttpClient>(() => new HttpClient(Address, Port));
        SetModelT<HttpsClient>(() => new HttpsClient(new SslContext(SslProtocols.Tls12 | SslProtocols.Tls13), Address, Port));
    }
}

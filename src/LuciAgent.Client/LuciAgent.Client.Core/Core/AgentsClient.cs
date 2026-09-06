using LuciferCore.Model;
using LuciferCore.NetCoreServer.Client;
using LuciferCore.NetCoreServer.Transport.SSL;
using System.Net;

namespace LuciAgent.Client.Core.Core;

public class AgentsClient : WssClient
{
    public AgentsClient(SslContext context, DnsEndPoint endpoint) : base(context, endpoint)
    {
    }

    public AgentsClient(SslContext context, IPEndPoint endpoint) : base(context, endpoint)
    {
    }

    public AgentsClient(SslContext context, IPAddress address, int port) : base(context, address, port)
    {
    }

    public AgentsClient(SslContext context, string hostOrAddress, int port = 443) : base(context, hostOrAddress, port)
    {
    }

    protected override void OnConnected()
    {
        base.OnConnected();
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        base.OnReceived(buffer, offset, size);
    }

    protected override void OnReceivedResponse(ResponseModel response)
    {
        //RouteHandler.Route(response, this);
        Info<byte>(response);
    }

    protected override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        base.OnWsReceived(buffer, offset, size);
    }
}

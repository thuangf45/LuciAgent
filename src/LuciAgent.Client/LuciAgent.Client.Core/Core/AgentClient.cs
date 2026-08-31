using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.NetCoreServer.Client;
using System.Net;

namespace LuciAgent.Client.Core.Core;

public class AgentClient : WsClient
{
    public AgentClient(string host) : base(host)
    {
    }

    public AgentClient(DnsEndPoint endpoint) : base(endpoint)
    {
    }

    public AgentClient(IPEndPoint endpoint) : base(endpoint)
    {
    }

    public AgentClient(IPAddress address, int port) : base(address, port)
    {
    }

    public AgentClient(string address, int port) : base(address, port)
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
        RouteHandler.Route(response, this);
    }

    protected override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        base.OnWsReceived(buffer, offset, size);
    }
}

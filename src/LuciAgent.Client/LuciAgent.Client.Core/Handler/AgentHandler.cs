using LuciAgent.Client.Core.Service;
using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.NetCoreServer.Transport.Core;

namespace LuciAgent.Client.Core.Handler;

[Handler("v1", "/api/agent")]
public class AgentHandler : RouteHandler
{
    private readonly AgentService _agentService;

    public AgentHandler(AgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost("join")]
    public void JoinAgent([Data] ResponseModel res, [Session] SessionTransport ss)
    {
        _agentService.JoinNetwork(res);
        Console.WriteLine($"Agent joined the network.");
    }
}

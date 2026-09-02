using LuciAgent.Server.Service;
using LuciferCore.Attributes;
using LuciferCore.Handler;

namespace LuciAgent.Server.Handler;

[Handler("v1", "/api/agent")]
public class AgentHandler : RouteHandler
{
    private readonly AgentService _agentService;

    public AgentHandler(AgentService agentService)
    {
        _agentService = agentService;
    }
}

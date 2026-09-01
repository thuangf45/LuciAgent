using LuciAgent.Client.Core.Contract;
using LuciferCore.Attributes;
using LuciferCore.Model;

namespace LuciAgent.Client.Core.Service;

[Singleton(Order = 10)]
public class AgentService
{
    private readonly AgentBase _agent;
    public AgentService(AgentBase agent)
    {
        _agent = agent;
        Console.WriteLine("AgentService initialized.");
    }

    public void JoinNetwork(ResponseModel response)
    {
        _agent.JoinNetwork();
    }

}

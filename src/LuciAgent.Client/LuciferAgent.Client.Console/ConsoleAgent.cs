using LuciAgent.Client.Core.Contract;
using LuciAgent.Client.Core.Core;
using LuciferCore.Attributes;

namespace LuciAgent.Client.Console;

[Singleton(ServiceType = typeof(AgentBase), Order = 1)]
public class ConsoleAgent : AgentBase
{
    public ConsoleAgent() : base(new AgentClient("127.0.0.1", 12345))
    {
        System.Console.WriteLine("ConsoleAgent initialized.");
    }

    public override AgentIdentity GetIdentity()
    {
        throw new NotImplementedException();
    }
}

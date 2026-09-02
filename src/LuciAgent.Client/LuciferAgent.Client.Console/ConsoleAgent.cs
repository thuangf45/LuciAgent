using LuciAgent.Client.Core;
using LuciAgent.Client.Core.Contract;
using LuciAgent.Client.Core.Core;
using LuciferCore.Attributes;

namespace LuciAgent.Client.Console;

[Singleton(ServiceType = typeof(AgentBase), Order = 1)]
public class ConsoleAgent : AgentBase
{
    public ConsoleAgent() : base(new AgentClient("lucifervn.org"), new DefaultPlatformInfoProvider("ConsoleAgent"))
    {

    }
}

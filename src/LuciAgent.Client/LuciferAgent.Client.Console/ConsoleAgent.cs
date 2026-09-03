using LuciAgent.Client.Core.Contract;
using LuciferCore.Attributes;

namespace LuciAgent.Client.Console;

[Singleton(ServiceType = typeof(AgentBase), Order = 1)]
public class ConsoleAgent : AgentBase
{
    public ConsoleAgent(IPlatformInfoProvider platform) : base(platform)
    {

    }
}

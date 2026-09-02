using LuciAgent.Client.Core.Contract;

var agent = GetModelI<AgentBase>();

var identity = await agent.GetIdentityAsync();

Console.WriteLine(identity);
//CMD("/run");

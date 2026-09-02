using LuciAgent.Client.Core;

var agent = GetModelI<AgentBase>();

var identity = await agent.GetIdentityAsync();

Console.WriteLine(identity);
//CMD("/run");

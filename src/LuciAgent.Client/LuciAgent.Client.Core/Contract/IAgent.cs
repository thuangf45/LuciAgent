namespace LuciAgent.Client.Core.Contract;

public interface IAgent
{
    object? GetIdentity();
    object? GetVisibleEntities();
    object? GetObservers();
    object? GetCommunicationEndpoints();
}

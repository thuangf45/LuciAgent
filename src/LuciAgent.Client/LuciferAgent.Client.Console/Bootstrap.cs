using LuciferCore.Attributes;

namespace LuciAgent.Client.Console;

public static class Bootstrap
{
    [Bootstrap]
    internal static void Initialize()
    {
        System.Console.WriteLine("Bootstrapping the application...");
    }
}

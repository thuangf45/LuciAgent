using LuciferCore.Attributes;

namespace LuciAgent.Client.Console;

public static class Boot
{
    [Bootstrap]
    internal static void Initialize()
    {
        System.Console.WriteLine("Bootstrapping the application...");
    }
}

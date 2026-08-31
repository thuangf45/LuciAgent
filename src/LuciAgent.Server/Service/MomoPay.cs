using LuciAgent.Server.Contract;
using LuciferCore.Attributes;
using System.Runtime.CompilerServices;

namespace LuciAgent.Server.Service;

[Singleton]
public class MomoPay : IPayment
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Payment(int amount)
    {
        Console.WriteLine("Processing Momo payment of amount: " + amount);
    }
}

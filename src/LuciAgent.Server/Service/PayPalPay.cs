using LuciAgent.Server.Contract;
using LuciferCore.Attributes;
using System.Runtime.CompilerServices;

namespace LuciAgent.Server.Service;

[Singleton]
public class PayPalPay : IPayment
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Payment(int amount)
    {
        Console.WriteLine("Processing PayPal payment of amount: " + amount);
    }
}

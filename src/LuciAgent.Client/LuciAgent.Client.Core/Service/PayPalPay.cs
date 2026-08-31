using LuciAgent.Client.Core.Contract;
using LuciferCore.Attributes;
using System.Runtime.CompilerServices;

namespace LuciAgent.Client.Core.Service;

[Singleton]
public class PayPalPay : IPayment
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Payment(int amount)
    {
        Console.WriteLine("Processing PayPal payment of amount: " + amount);
    }
}

using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Middleware;
using LuciferCore.NetCoreServer.Transport.Core;
using System.Runtime.CompilerServices;

namespace LuciAgent.Client.Core.Middleware;

[Middleware("LogMiddleware")]
public class LogMiddleware : MiddlewareHandler
{
    protected override bool Handle(IRoutable? data, SessionTransport session)
    {
        // Do something with the data and session, for example, log the request information
        // Info<char>(this, "Hello World");
        return true;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class LogAttribute : UseMiddlewareAttribute
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogAttribute() : base("LogMiddleware")
    {
    }
}

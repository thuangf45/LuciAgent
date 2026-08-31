using LuciAgent.Client.Core.Core;
using LuciAgent.Client.Core.Middleware;
using LuciAgent.Client.Core.Service;
using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using System.Runtime.CompilerServices;

namespace LuciAgent.Client.Core.Handler;

[Handler("v1", "/api/auth")]
public class AuthHandler : RouteHandler
{
    private readonly AuthService _authService;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AuthHandler(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("/login")]
    [Log]
    private void HandleLogin([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = _authService.Login(session, request);
        session.SendResponseAsync(response);
    }

    [HttpPost("/register")]
    [Log]
    private void HandleRegister([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = _authService.Register(session, request);
        session.SendResponseAsync(response);
    }

    [HttpPost("/logout")]
    [Log]
    private void HandleLogout([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = _authService.Logout(session, request);
        session.SendResponseAsync(response);
    }
}

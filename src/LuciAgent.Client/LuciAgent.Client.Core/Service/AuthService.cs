using LuciAgent.Client.Core.Core;
using LuciferCore.Attributes;
using LuciferCore.Main;
using LuciferCore.Model;
using System.Runtime.CompilerServices;

namespace LuciAgent.Client.Core.Service;

[Singleton]
public class AuthService
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResponseModel Login(AgentSession session, RequestModel request)
    {
        // Create a response model
        var response = Lucifer.Rent<ResponseModel>();

        // Implement your login logic here
        // For example, validate the request data, check credentials, etc.


        return response;
    }

    public ResponseModel Register(AgentSession session, RequestModel request)
    {
        // Create a response model
        var response = Lucifer.Rent<ResponseModel>();

        // Implement your registration logic here
        // For example, validate the request data, create a new user, etc.


        return response;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResponseModel Logout(AgentSession session, RequestModel request)
    {
        // Create a response model
        var response = Lucifer.Rent<ResponseModel>();

        // Implement your logout logic here
        // For example, invalidate the session, clear cookies, etc.


        return response;
    }
}

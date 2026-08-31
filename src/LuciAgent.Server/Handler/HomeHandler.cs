using LuciAgent.Server.Core;
using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Main;
using LuciferCore.Model;

namespace LuciAgent.Server.Handler;

[Handler("v1", "/api")]
public class HomeHandler : RouteHandler
{
    [HttpGet("/ping")]
    private void Ping([Session] AppSession session, [Data] RequestModel request)
    {
        // Build a custom response using the MakeCustomResponse method
        using var response = Lucifer.Rent<ResponseModel>().MakeCustomResponse<char, char, char>
            (
                200, // Status code
                "HTTP/1.1", // Protocol version
                "application/json", // Content type
                new
                {
                    status = "healthy",
                    framework = "LuciferCore"
                }.ToJson() // Body content
            );

        // Send the response back to the client
        session.SendResponseAsync(response);
    }
}

using LuciAgent.Client.Core.Service;
using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.NetCoreServer.Transport.Core;

namespace LuciAgent.Client.Core.Handler;

[Handler("v1", "/api/agent")]
public class AgentHandler : RouteHandler
{
    private readonly AgentService _agentService;

    public AgentHandler(AgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet("")]
    private void GetAgent([Data] RequestModel res, [Session] SessionTransport ss)
    {
        using var response = Rent<ResponseModel>().MakeOkResponse(200);
        ss.SendAsync<byte>(response);
    }

    [HttpPost("")]
    private void PostAgent([Data] RequestModel res, [Session] SessionTransport ss)
    {
        //Info<char>(res.ToString());
        using var response = _agentService.Handle(res);
        ss.SendAsync<byte>(response);
    }

    [HttpPut("")]
    private void PutAgent([Data] RequestModel res, [Session] SessionTransport ss)
    {
        using var response = Rent<ResponseModel>().MakeOkResponse(200);
        ss.SendAsync<byte>(response);
    }

    [HttpDelete("")]
    private void DeleteAgent([Data] RequestModel res, [Session] SessionTransport ss)
    {
        using var response = Rent<ResponseModel>().MakeOkResponse(200);
        ss.SendAsync<byte>(response);
    }

    [HttpHead("")]
    private void HeadAgent([Data] RequestModel res, [Session] SessionTransport ss)
    {
        using var response = Rent<ResponseModel>().MakeHeadResponse();
        ss.SendAsync<byte>(response);
    }

    [HttpTrace("")]
    private void TraceAgent([Data] RequestModel res, [Session] SessionTransport ss)
    {
        using var response = Rent<ResponseModel>().MakeTraceResponse(res);
        ss.SendAsync<byte>(response);
    }

    [HttpOptions("")]
    private void OptionsAgent([Data] RequestModel res, [Session] SessionTransport ss)
    {
        using var response = Rent<ResponseModel>().MakeOptionsResponse("HEAD,GET,POST,PUT,DELETE,OPTIONS,TRACE"u8);
        ss.SendAsync<byte>(response);
    }
}

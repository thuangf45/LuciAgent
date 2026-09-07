using LuciAgent.Client.Core.Contract;
using LuciferCore.Attributes;
using LuciferCore.Model;

namespace LuciAgent.Client.Core.Service;

[Singleton(Order = 10)]
public class AgentService
{
    private readonly AgentBase _agent;
    public AgentService(AgentBase agent)
    {
        _agent = agent;
    }

    //public void JoinNetwork(ResponseModel response)
    //{
    //    //_agent.JoinNetwork();
    //}

    public ResponseModel Handle(RequestModel request)
    {
        var response = Rent<ResponseModel>();

        try
        {
            CMD(request.BodySpan);
            response.MakeOkResponse(200);
        }
        catch (Exception ex)
        {
            response.MakeErrorResponse<char>(500, ex.Message);
        }

        return response;
    }

}

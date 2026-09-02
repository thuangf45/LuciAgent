using LuciAgent.Server.Core;
using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Model;
using LuciferCore.Utf8;

namespace LuciAgent.Server.Service;

[Singleton]
public class AgentService
{
    private readonly Utf8Map<Utf8Map<Utf8Map<AgentIdentity>>> _agentMap = new();
    public ResponseModel JoinNetwork(RequestModel request)
    {
        using var req = request;
        var res = Rent<ResponseModel>();
        var identity = Rent<AgentIdentity>();

        identity.Attach(req.Cache);

        var _wanMap = _agentMap.GetOrAdd(identity.WanIP, _ => []);
        var _lanMap = _wanMap.GetOrAdd(identity.LanID, _ => []);

        res.MakeCustomResponse<byte, byte, char>(200, "HTTP/1.1"u8, "application/json"u8, _lanMap.ToJson());

        return res;
    }
}

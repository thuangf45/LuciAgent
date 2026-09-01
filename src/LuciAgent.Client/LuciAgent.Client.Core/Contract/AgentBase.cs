using LuciAgent.Client.Core.Core;
using LuciferCore.Model;
using LuciferCore.Utf8;

namespace LuciAgent.Client.Core.Contract;

public abstract class AgentBase
{
    public AgentBase(AgentClient client)
    {
        _client = client;
    }

    private readonly AgentClient _client;

    public readonly Dictionary<AgentIdentity, bool> OtherAgents = [];
    public abstract AgentIdentity GetIdentity();

    public virtual void JoinNetwork()
    {
        var identity = GetIdentity();
        EnsureConnected();

        using var request = Rent<RequestModel>();
        request.MakePostRequest<byte, byte>("/v1/api/agent/join"u8, identity.Buffer!);
    }

    private void EnsureConnected()
    {
        int retryCount = 0;
        while (_client.IsConnected == false && retryCount < 5)
        {
            _client.Connect();
            retryCount++;
            Thread.Sleep(1000); // Wait for 1 second before checking again
        }
    }
}

public partial class AgentIdentity : LayoutModel
{
    public AgentIdentity()
    {
        _totalFields = 6;
        Attach(Rent<Buffer>());
    }

    [LayoutIndex(0)]
    public partial ReadOnlySpan<byte> AgentID { get; set; }

    [LayoutIndex(1)]
    public partial ReadOnlySpan<byte> AgentName { get; set; }

    [LayoutIndex(2)]
    public partial ReadOnlySpan<byte> OSVersion { get; set; }

    [LayoutIndex(3)]
    public partial ReadOnlySpan<byte> LanIP { get; set; }

    [LayoutIndex(4)]
    public partial ReadOnlySpan<byte> LanID { get; set; }

    [LayoutIndex(5)]
    public partial ReadOnlySpan<byte> WanIP { get; set; }

    public override string ToString()
    {
        using var builder = Rent<Utf8Builder>()
            .Append("AgentID: "u8).Append(AgentID)
            .Append("\nAgentName: "u8).Append(AgentName)
            .Append("\nOSVersion: "u8).Append(OSVersion)
            .Append("\nLanIP: "u8).Append(LanIP)
            .Append("\nLanID: "u8).Append(LanID)
            .Append("\nWanIP: "u8).Append(WanIP);
        return builder.ToString();
    }

    protected override void Reset()
    {
        base.Reset();
    }
}
using LuciferCore.Model;
using LuciferCore.Utf8;

namespace LuciAgent.Client.Core.Model;

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
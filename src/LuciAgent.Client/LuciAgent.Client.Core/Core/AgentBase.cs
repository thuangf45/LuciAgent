using LuciAgent.Client.Core.Contract;
using LuciAgent.Client.Core.Core;
using LuciferCore.Model;
using LuciferCore.Utf8;
using System.Net;
using System.Text;

namespace LuciAgent.Client.Core;

public abstract class AgentBase
{
    private readonly AgentClient _client;
    private readonly IPlatformInfoProvider _platform;
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(3) };

    protected AgentBase(AgentClient client, IPlatformInfoProvider platform)
    {
        _client = client;
        _platform = platform;
    }

    public readonly Dictionary<AgentIdentity, bool> OtherAgents = [];

    public virtual AgentIdentity GetIdentity()
    {
        var identity = Rent<AgentIdentity>();
        identity.AgentID = Encoding.UTF8.GetBytes(_platform.GetPersistentAgentId());
        identity.AgentName = Encoding.UTF8.GetBytes(_platform.GetHostName());
        identity.OSVersion = Encoding.UTF8.GetBytes(_platform.GetOsDescription());
        identity.LanIP = Encoding.UTF8.GetBytes(_platform.GetLocalIPv4());
        identity.LanID = Encoding.UTF8.GetBytes(_platform.GetLanId());
        identity.WanIP = Encoding.UTF8.GetBytes("0.0.0.0");
        return identity;
    }

    public virtual async Task<AgentIdentity> GetIdentityAsync(CancellationToken ct = default)
    {
        var identity = GetIdentity();
        identity.WanIP = Encoding.UTF8.GetBytes(await GetPublicIPAddressAsync(ct));
        return identity;
    }

    private static async Task<string> GetPublicIPAddressAsync(CancellationToken ct)
    {
        string[] endpoints =
        {
            "https://api.ipify.org",
            "https://api64.ipify.org",
            "https://ifconfig.me/ip"
        };

        foreach (var ep in endpoints)
        {
            try
            {
                using var resp = await _http.GetAsync(ep, ct).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode) continue;

                var txt = (await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false)).Trim();
                if (IPAddress.TryParse(txt, out _)) return txt;
            }
            catch (OperationCanceledException) { throw; }
            catch { }
        }

        return "0.0.0.0";
    }

    public virtual async Task JoinNetworkAsync(CancellationToken ct = default)
    {
        var identity = await GetIdentityAsync(ct).ConfigureAwait(false);
        await EnsureConnectedAsync(ct).ConfigureAwait(false);

        using var request = Rent<RequestModel>();
        request.MakePostRequest<byte, byte>("/v1/api/agent/join"u8, identity.Buffer!);
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        for (int i = 0; i < 5 && !_client.IsConnected; i++)
        {
            ct.ThrowIfCancellationRequested();
            _client.Connect();
            if (_client.IsConnected) break;
            await Task.Delay(500 * (i + 1), ct).ConfigureAwait(false);
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
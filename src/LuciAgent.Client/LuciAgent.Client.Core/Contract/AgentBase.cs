using LuciAgent.Client.Core.Core;
using LuciAgent.Client.Core.Model;
using LuciferCore.Model;
using System.Net;
using System.Text;

namespace LuciAgent.Client.Core.Contract;

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
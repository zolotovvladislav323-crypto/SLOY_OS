using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Radio;

public class WiFiDirectEngine
{
    private readonly IWiFiDirectAdapter _wifiDirect;
    private bool _isGroupOwner;

    public bool IsConnected { get; private set; }
    public string? LocalIp { get; private set; }

    public WiFiDirectEngine(IWiFiDirectAdapter wifiDirect)
    {
        _wifiDirect = wifiDirect;
        _wifiDirect.OnConnected += (_, _) => IsConnected = true;
        _wifiDirect.OnDisconnected += (_, _) =>
        {
            IsConnected = false;
            _isGroupOwner = false;
        };
    }

    public async Task<bool> CreateMeshGroupAsync(string nodeId)
    {
        var ssid = $"SLOY_{nodeId[..6]}";
        var passphrase = GeneratePassphrase(nodeId);
        await _wifiDirect.StartGroupOwnerAsync(ssid, passphrase);
        _isGroupOwner = true;
        IsConnected = true;
        LocalIp = await _wifiDirect.GetLocalIpAddressAsync();
        return true;
    }

    public async Task<bool> JoinMeshGroupAsync(string ssid, string nodeId)
    {
        var passphrase = GeneratePassphrase(nodeId);
        await _wifiDirect.ConnectToGroupAsync(ssid, passphrase);
        IsConnected = true;
        LocalIp = await _wifiDirect.GetLocalIpAddressAsync();
        return true;
    }

    public async Task<List<DiscoveredGroup>> ScanForGroupsAsync(int timeoutMs = 10000)
        => await _wifiDirect.DiscoverGroupsAsync(timeoutMs);

    private static string GeneratePassphrase(string seed)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"SLOY_MESH_{seed}")
        );
        return Convert.ToBase64String(hash)[..12].Replace("/", "A").Replace("+", "B") + "!1";
    }
}
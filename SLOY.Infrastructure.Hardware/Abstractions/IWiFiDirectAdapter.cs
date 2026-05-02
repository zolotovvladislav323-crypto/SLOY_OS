namespace SLOY.Infrastructure.Hardware.Abstractions;

public interface IWiFiDirectAdapter
{
    bool IsEnabled { get; }
    bool IsAvailable { get; }
    Task<bool> EnableAsync();
    Task StartGroupOwnerAsync(string ssid, string passphrase);
    Task ConnectToGroupAsync(string ssid, string passphrase);
    Task<List<DiscoveredGroup>> DiscoverGroupsAsync(int timeoutMs = 10000);
    Task DisconnectAsync();
    Task<string> GetLocalIpAddressAsync();
    event EventHandler<DiscoveredGroup> OnGroupDiscovered;
    event EventHandler OnConnected;
    event EventHandler OnDisconnected;
}

public record DiscoveredGroup(string Ssid, string? Bssid, int SignalStrength, bool IsSecure);
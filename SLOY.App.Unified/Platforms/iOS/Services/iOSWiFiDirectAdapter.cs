using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.iOS.Services;

public class iOSWiFiDirectAdapter : IWiFiDirectAdapter
{
    public bool IsEnabled => false;
    public bool IsAvailable => false;

    public event EventHandler<DiscoveredGroup>? OnGroupDiscovered;
    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;

    public Task<bool> EnableAsync() => Task.FromResult(false);
    public Task StartGroupOwnerAsync(string ssid, string passphrase) => Task.CompletedTask;
    public Task ConnectToGroupAsync(string ssid, string passphrase) => Task.CompletedTask;
    public Task<List<DiscoveredGroup>> DiscoverGroupsAsync(int timeoutMs = 10000)
        => Task.FromResult(new List<DiscoveredGroup>());
    public Task DisconnectAsync() => Task.CompletedTask;
    public Task<string> GetLocalIpAddressAsync() => Task.FromResult("0.0.0.0");
}
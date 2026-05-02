using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.Windows.Services;

public class WindowsWiFiDirectAdapter : IWiFiDirectAdapter
{
    public bool IsEnabled => true;
    public bool IsAvailable => true;

    public event EventHandler<DiscoveredGroup>? OnGroupDiscovered;
    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;

    public Task<bool> EnableAsync() => Task.FromResult(true);

    public async Task StartGroupOwnerAsync(string ssid, string passphrase)
    {
        await Task.CompletedTask;
    }

    public async Task ConnectToGroupAsync(string ssid, string passphrase)
    {
        await Task.CompletedTask;
    }

    public async Task<List<DiscoveredGroup>> DiscoverGroupsAsync(int timeoutMs = 10000)
    {
        await Task.Delay(timeoutMs);
        return new List<DiscoveredGroup>();
    }

    public Task DisconnectAsync() => Task.CompletedTask;

    public async Task<string> GetLocalIpAddressAsync()
    {
        await Task.CompletedTask;
        var hostName = System.Net.Dns.GetHostName();
        var host = await System.Net.Dns.GetHostEntryAsync(hostName);
        var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        return ip?.ToString() ?? "127.0.0.1";
    }
}
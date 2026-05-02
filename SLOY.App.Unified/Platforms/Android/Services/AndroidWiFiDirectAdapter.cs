using Android.Net.Wifi.P2p;
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.Android.Services;

public class AndroidWiFiDirectAdapter : IWiFiDirectAdapter
{
    private readonly WifiP2pManager? _manager;
    private readonly WifiP2pManager.Channel? _channel;

    public bool IsEnabled => true;
    public bool IsAvailable => _manager != null;

    public event EventHandler<DiscoveredGroup>? OnGroupDiscovered;
    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;

    public AndroidWiFiDirectAdapter()
    {
        _manager = (WifiP2pManager?)Platform.CurrentActivity?.GetSystemService(
            Android.Content.Context.WifiP2pService);
        _channel = _manager?.Initialize(Platform.CurrentActivity, Platform.CurrentActivity?.MainLooper, null);
    }

    public async Task<bool> EnableAsync()
    {
        await Task.CompletedTask;
        return _manager != null;
    }

    public Task StartGroupOwnerAsync(string ssid, string passphrase)
    {
        var config = new WifiP2pConfig.Builder()
            ?.SetNetworkName(ssid)
            ?.SetPassphrase(passphrase)
            ?.Build();

        _manager?.CreateGroup(_channel, config, new GroupActionListener());
        return Task.CompletedTask;
    }

    public Task ConnectToGroupAsync(string ssid, string passphrase)
    {
        var config = new WifiP2pConfig.Builder()
            ?.SetNetworkName(ssid)
            ?.SetPassphrase(passphrase)
            ?.Build();

        _manager?.Connect(_channel, config, new ConnectActionListener());
        return Task.CompletedTask;
    }

    public Task<List<DiscoveredGroup>> DiscoverGroupsAsync(int timeoutMs = 10000)
    {
        var groups = new List<DiscoveredGroup>();
        _manager?.DiscoverPeers(_channel, new PeerListListener(groups));
        return Task.FromResult(groups);
    }

    public Task DisconnectAsync()
    {
        _manager?.RemoveGroup(_channel, null);
        return Task.CompletedTask;
    }

    public Task<string> GetLocalIpAddressAsync()
    {
        return Task.FromResult("192.168.49.1");
    }

    private class GroupActionListener : Java.Lang.Object, WifiP2pManager.IGroupInfoListener
    {
        public void OnGroupInfoAvailable(WifiP2pGroup? group)
        {
            // Группа создана
        }
    }

    private class ConnectActionListener : Java.Lang.Object, WifiP2pManager.IActionListener
    {
        public void OnSuccess() { }
        public void OnFailure(int reason) { }
    }

    private class PeerListListener : Java.Lang.Object, WifiP2pManager.IPeerListListener
    {
        private readonly List<DiscoveredGroup> _groups;

        public PeerListListener(List<DiscoveredGroup> groups)
        {
            _groups = groups;
        }

        public void OnPeersAvailable(WifiP2pDeviceList? peers)
        {
            if (peers == null) return;
            foreach (var device in peers.DeviceList)
            {
                _groups.Add(new DiscoveredGroup(
                    device.DeviceName ?? "unknown",
                    device.DeviceAddress,
                    device.Status == WifiP2pDevice.Connected ? -30 : -70,
                    true
                ));
            }
        }
    }
}
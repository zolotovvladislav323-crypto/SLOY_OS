using Android.Bluetooth;
using Android.Bluetooth.LE;
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.Android.Services;

public class AndroidBluetoothAdapter : IBluetoothAdapter
{
    private readonly BluetoothManager? _manager;
    private readonly BluetoothLeAdvertiser? _advertiser;
    private readonly BluetoothLeScanner? _scanner;
    private readonly Dictionary<string, BluetoothGatt> _connections = new();

    public bool IsEnabled => BluetoothAdapter.DefaultAdapter?.IsEnabled ?? false;
    public bool IsAvailable => BluetoothAdapter.DefaultAdapter != null;

    public event EventHandler<DiscoveredDevice>? OnDeviceDiscovered;
    public event EventHandler<BluetoothConnection>? OnDeviceConnected;

    public AndroidBluetoothAdapter()
    {
        _manager = (BluetoothManager?)Platform.CurrentActivity?.GetSystemService(
            Android.Content.Context.BluetoothService);
        _advertiser = BluetoothAdapter.DefaultAdapter?.BluetoothLeAdvertiser;
        _scanner = BluetoothAdapter.DefaultAdapter?.BluetoothLeScanner;
    }

    public async Task<bool> EnableAsync()
    {
        if (BluetoothAdapter.DefaultAdapter == null) return false;

        if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
        {
            var intent = new Android.Content.Intent(BluetoothAdapter.ActionRequestEnable);
            Platform.CurrentActivity?.StartActivity(intent);
            await Task.Delay(2000);
        }

        return BluetoothAdapter.DefaultAdapter.IsEnabled;
    }

    public Task<bool> DisableAsync()
    {
        var result = BluetoothAdapter.DefaultAdapter?.Disable() ?? false;
        return Task.FromResult(result);
    }

    public async Task StartAdvertisingAsync(string serviceUuid, byte[]? manufacturerData = null)
    {
        await Task.Run(() =>
        {
            var settings = new AdvertiseSettings.Builder()
                ?.SetAdvertiseMode(AdvertiseMode.LowLatency)
                ?.SetTxPowerLevel(AdvertiseTx.PowerHigh)
                ?.SetConnectable(false)
                ?.Build();

            var dataBuilder = new AdvertiseData.Builder()
                ?.SetIncludeDeviceName(false)
                ?.SetIncludeTxPowerLevel(false);

            if (manufacturerData != null)
                dataBuilder?.AddManufacturerData(0xFFFF, manufacturerData);

            _advertiser?.StartAdvertising(settings, dataBuilder?.Build(), null);
        });
    }

    public Task StopAdvertisingAsync()
    {
        _advertiser?.StopAdvertising(null);
        return Task.CompletedTask;
    }

    public async Task<List<DiscoveredDevice>> ScanAsync(int timeoutMs = 5000)
    {
        var devices = new List<DiscoveredDevice>();
        var tcs = new TaskCompletionSource<List<DiscoveredDevice>>();

        var callback = new ScanCallback((name, address, rssi, data) =>
        {
            var device = new DiscoveredDevice(address, name, rssi, data);
            if (!devices.Any(d => d.Address == address))
                devices.Add(device);
            OnDeviceDiscovered?.Invoke(this, device);
        });

        _scanner?.StartScan(callback);

        await Task.Delay(timeoutMs);
        _scanner?.StopScan(callback);

        return devices;
    }

    public Task<BluetoothConnection?> ConnectAsync(string address)
    {
        return Task.FromResult<BluetoothConnection?>(null);
    }

    private class ScanCallback : Android.Bluetooth.LE.ScanCallback
    {
        private readonly Action<string?, string, int, byte[]?> _onFound;

        public ScanCallback(Action<string?, string, int, byte[]?> onFound)
        {
            _onFound = onFound;
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
        {
            if (result?.Device == null) return;
            var record = result.ScanRecord;
            _onFound(
                result.Device.Name,
                result.Device.Address ?? "unknown",
                result.Rssi,
                record?.GetBytes()
            );
        }
    }
}
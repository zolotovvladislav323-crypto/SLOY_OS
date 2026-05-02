using CoreBluetooth;
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.iOS.Services;

public class iOSBluetoothAdapter : IBluetoothAdapter
{
    private readonly CBCentralManager? _centralManager;
    private readonly CBPeripheralManager? _peripheralManager;
    private bool _isAdvertising;

    public bool IsEnabled => _centralManager?.State == CBManagerState.PoweredOn;
    public bool IsAvailable => true;

    public event EventHandler<DiscoveredDevice>? OnDeviceDiscovered;
    public event EventHandler<BluetoothConnection>? OnDeviceConnected;

    public iOSBluetoothAdapter()
    {
        _centralManager = new CBCentralManager();
        _peripheralManager = new CBPeripheralManager();
    }

    public async Task<bool> EnableAsync()
    {
        await Task.Delay(500);
        return IsEnabled;
    }

    public Task<bool> DisableAsync()
    {
        return Task.FromResult(false);
    }

    public async Task StartAdvertisingAsync(string serviceUuid, byte[]? manufacturerData = null)
    {
        var service = new CBMutableService(CBUUID.FromString(serviceUuid), true);
        await Task.Run(() =>
        {
            _peripheralManager?.AddService(service);
            _peripheralManager?.StartAdvertising(new StartAdvertisingOptions
            {
                ServicesUUIDs = new[] { CBUUID.FromString(serviceUuid) }
            });
        });
    }

    public Task StopAdvertisingAsync()
    {
        _peripheralManager?.StopAdvertising();
        return Task.CompletedTask;
    }

    public async Task<List<DiscoveredDevice>> ScanAsync(int timeoutMs = 5000)
    {
        var devices = new List<DiscoveredDevice>();
        await Task.Delay(timeoutMs);
        return devices;
    }

    public Task<BluetoothConnection?> ConnectAsync(string address)
    {
        return Task.FromResult<BluetoothConnection?>(null);
    }
}
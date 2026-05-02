namespace SLOY.Infrastructure.Hardware.Abstractions;

public interface IBluetoothAdapter
{
    bool IsEnabled { get; }
    bool IsAvailable { get; }
    Task<bool> EnableAsync();
    Task<bool> DisableAsync();
    Task StartAdvertisingAsync(string serviceUuid, byte[]? manufacturerData = null);
    Task StopAdvertisingAsync();
    Task<List<DiscoveredDevice>> ScanAsync(int timeoutMs = 5000);
    Task<BluetoothConnection?> ConnectAsync(string address);
    event EventHandler<DiscoveredDevice> OnDeviceDiscovered;
    event EventHandler<BluetoothConnection> OnDeviceConnected;
}

public record DiscoveredDevice(string Address, string? Name, int Rssi, byte[]? AdvertisementData);
public record BluetoothConnection(string Address, string? Name);
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Radio;

public class BleAdvertiser
{
    private readonly IBluetoothAdapter _bluetooth;
    private bool _isAdvertising;
    private byte[]? _payload;

    public bool IsAdvertising => _isAdvertising;

    public BleAdvertiser(IBluetoothAdapter bluetooth)
    {
        _bluetooth = bluetooth;
    }

    public async Task StartAdvertisingAsync(byte[] payload, string serviceUuid = "0000ABCD-0000-1000-8000-00805F9B34FB")
    {
        _payload = payload;
        await _bluetooth.StartAdvertisingAsync(serviceUuid, payload);
        _isAdvertising = true;
    }

    public async Task StopAdvertisingAsync()
    {
        await _bluetooth.StopAdvertisingAsync();
        _isAdvertising = false;
    }

    public byte[] EncodePayload(string nodeId, byte[] publicKeyFingerprint)
    {
        var nodeIdBytes = System.Text.Encoding.UTF8.GetBytes(nodeId);
        var payload = new byte[nodeIdBytes.Length + publicKeyFingerprint.Length + 1];
        payload[0] = (byte)nodeIdBytes.Length;
        Array.Copy(nodeIdBytes, 0, payload, 1, nodeIdBytes.Length);
        Array.Copy(publicKeyFingerprint, 0, payload, nodeIdBytes.Length + 1, publicKeyFingerprint.Length);
        return payload;
    }
}
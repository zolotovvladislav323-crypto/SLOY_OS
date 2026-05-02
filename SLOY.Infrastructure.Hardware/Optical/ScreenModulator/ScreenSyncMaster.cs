namespace SLOY.Infrastructure.Hardware.Optical.ScreenModulator;

public class ScreenSyncMaster
{
    private readonly ColorShiftEncoder _encoder;
    private readonly ScreenLinkReceiver _receiver;
    private bool _isActive;

    public bool IsActive => _isActive;
    public bool IsMaster { get; private set; }

    public ScreenSyncMaster(ColorShiftEncoder encoder, ScreenLinkReceiver receiver)
    {
        _encoder = encoder;
        _receiver = receiver;
    }

    public async Task StartAsMasterAsync(byte[] initialData)
    {
        IsMaster = true;
        _isActive = true;

        var syncPacket = CreateSyncPacket(initialData);
        await foreach (var color in _encoder.EncodeStreamAsync(syncPacket))
        {
            if (!_isActive) break;
        }
    }

    public async Task StartAsSlaveAsync()
    {
        IsMaster = false;
        _isActive = true;

        _receiver.OnDataReceived += (_, data) =>
        {
            if (!_isActive) return;

            if (IsSyncPacket(data))
            {
                var payload = ExtractPayload(data);
                var response = CreateResponse(payload);
                RespondWithColor(response);
            }
        };

        await _receiver.StartReceivingAsync();
    }

    private byte[] CreateSyncPacket(byte[] data)
    {
        var packet = new byte[data.Length + 4];
        packet[0] = 0x53; // 'S'
        packet[1] = 0x59; // 'Y'
        packet[2] = 0x4E; // 'N'
        packet[3] = (byte)data.Length;
        Array.Copy(data, 0, packet, 4, data.Length);
        return packet;
    }

    private bool IsSyncPacket(byte[] data)
    {
        return data.Length >= 4 && data[0] == 0x53 && data[1] == 0x59 && data[2] == 0x4E;
    }

    private byte[] ExtractPayload(byte[] packet)
    {
        var length = packet[3];
        var payload = new byte[length];
        Array.Copy(packet, 4, payload, 0, length);
        return payload;
    }

    private byte[] CreateResponse(byte[] data)
    {
        var response = new byte[data.Length + 4];
        response[0] = 0x41; // 'A'
        response[1] = 0x43; // 'C'
        response[2] = 0x4B; // 'K'
        response[3] = (byte)data.Length;
        Array.Copy(data, 0, response, 4, data.Length);
        return response;
    }

    private async void RespondWithColor(byte[] response)
    {
        await foreach (var color in _encoder.EncodeStreamAsync(response))
        {
            if (!_isActive) break;
        }
    }

    public void Stop()
    {
        _isActive = false;
        _encoder.Stop();
    }
}
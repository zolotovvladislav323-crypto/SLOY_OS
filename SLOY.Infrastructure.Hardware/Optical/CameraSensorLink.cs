using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Optical;

public class CameraSensorLink
{
    private readonly ICameraAdapter _camera;
    private readonly Queue<byte[]> _frameBuffer = new(30);
    private bool _isReceiving;
    private CancellationTokenSource? _cts;

    public event EventHandler<byte[]>? OnDataReceived;
    public int FrameBufferSize => _frameBuffer.Count;

    public CameraSensorLink(ICameraAdapter camera)
    {
        _camera = camera;
    }

    public async Task StartReceivingAsync(int fps = 30)
    {
        _cts = new CancellationTokenSource();
        _isReceiving = true;
        await _camera.StartCaptureAsync(fps);

        _camera.OnFrameCaptured += (_, frame) =>
        {
            if (!_isReceiving) return;

            _frameBuffer.Enqueue(frame);
            if (_frameBuffer.Count > 30)
                _frameBuffer.Dequeue();

            var decoded = DecodeFrame(frame);
            if (decoded != null)
                OnDataReceived?.Invoke(this, decoded);
        };

        await Task.CompletedTask;
    }

    public Task StopReceivingAsync()
    {
        _isReceiving = false;
        _cts?.Cancel();
        return _camera.StopCaptureAsync();
    }

    private byte[]? DecodeFrame(byte[] frame)
    {
        if (frame.Length < 100) return null;

        var brightnessChanges = CountBrightnessChanges(frame);
        if (brightnessChanges < 10) return null;

        var decoded = new List<byte>();
        var threshold = CalculateAdaptiveThreshold(frame);

        for (int i = 1; i < frame.Length; i += 4)
        {
            var diff = Math.Abs(frame[i] - frame[i - 1]);
            decoded.Add(diff > threshold ? (byte)1 : (byte)0);
        }

        return ConvertBitArrayToBytes(decoded.ToArray());
    }

    private int CountBrightnessChanges(byte[] frame)
    {
        int count = 0;
        for (int i = 1; i < frame.Length; i += 4)
            if (Math.Abs(frame[i] - frame[i - 1]) > 20) count++;
        return count;
    }

    private byte CalculateAdaptiveThreshold(byte[] frame)
    {
        var sum = 0;
        var samples = Math.Min(frame.Length / 4, 1000);
        for (int i = 0; i < samples * 4; i += 4)
            sum += Math.Abs(frame[i] - frame[Math.Min(i + 4, frame.Length - 1)]);
        return (byte)(sum / samples * 0.7);
    }

    private byte[] ConvertBitArrayToBytes(byte[] bits)
    {
        var bytes = new byte[bits.Length / 8];
        for (int i = 0; i < bytes.Length; i++)
        {
            byte value = 0;
            for (int bit = 0; bit < 8; bit++)
                if (i * 8 + bit < bits.Length && bits[i * 8 + bit] == 1)
                    value |= (byte)(1 << (7 - bit));
            bytes[i] = value;
        }
        return bytes;
    }
}
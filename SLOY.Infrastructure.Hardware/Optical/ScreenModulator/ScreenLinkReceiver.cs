using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Optical.ScreenModulator;

public class ScreenLinkReceiver
{
    private readonly ICameraAdapter _camera;
    private readonly ColorShiftEncoder _decoder;
    private readonly Queue<byte> _colorBuffer = new();
    private bool _isReceiving;

    public event EventHandler<byte[]>? OnDataReceived;
    public event EventHandler<string>? OnStatusChanged;

    public ScreenLinkReceiver(ICameraAdapter camera)
    {
        _camera = camera;
        _decoder = new ColorShiftEncoder();
    }

    public async Task StartReceivingAsync()
    {
        _isReceiving = true;
        await _camera.StartCaptureAsync(60);

        OnStatusChanged?.Invoke(this, "Сканирование экрана...");

        _camera.OnFrameCaptured += (_, frame) =>
        {
            if (!_isReceiving) return;
            ProcessFrame(frame);
        };
    }

    private void ProcessFrame(byte[] frame)
    {
        var dominantColor = GetDominantColor(frame);
        var colorValue = ClassifyColor(dominantColor);

        _colorBuffer.Enqueue(colorValue);
        if (_colorBuffer.Count > 1024)
            _colorBuffer.Dequeue();

        if (_colorBuffer.Count >= 12)
        {
            var data = _decoder.DecodeFromColorSequence(_colorBuffer.ToArray());
            if (data.Length > 2)
            {
                OnDataReceived?.Invoke(this, data);
                OnStatusChanged?.Invoke(this, $"Принято {data.Length} байт");
            }
        }
    }

    private (byte r, byte g, byte b) GetDominantColor(byte[] frame)
    {
        long r = 0, g = 0, b = 0;
        var pixels = Math.Min(frame.Length / 4, 100);
        var step = frame.Length / 4 / pixels;

        for (int i = 0; i < pixels; i++)
        {
            var idx = i * step * 4;
            if (idx + 3 >= frame.Length) break;
            r += frame[idx + 2];
            g += frame[idx + 1];
            b += frame[idx];
        }

        pixels = Math.Max(pixels, 1);
        return ((byte)(r / pixels), (byte)(g / pixels), (byte)(b / pixels));
    }

    private byte ClassifyColor((byte r, byte g, byte b) color)
    {
        var (r, g, b) = color;
        var max = Math.Max(r, Math.Max(g, b));

        if (max < 30) return 0b00;
        if (b > r && b > g) return 0b01;
        if (g > r && g > b) return 0b10;
        return 0b11;
    }

    public Task StopReceivingAsync()
    {
        _isReceiving = false;
        OnStatusChanged?.Invoke(this, "Остановлено");
        return _camera.StopCaptureAsync();
    }
}
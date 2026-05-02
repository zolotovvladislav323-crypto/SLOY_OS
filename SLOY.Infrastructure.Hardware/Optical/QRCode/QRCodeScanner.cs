using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Optical.QRCode;

public class QRCodeScanner
{
    private readonly ICameraAdapter _camera;
    private bool _isScanning;
    private DateTime _lastScanTime = DateTime.MinValue;
    private readonly TimeSpan _scanCooldown = TimeSpan.FromMilliseconds(500);

    public event EventHandler<string>? OnQRCodeDecoded;

    public QRCodeScanner(ICameraAdapter camera)
    {
        _camera = camera;
    }

    public async Task StartScanningAsync(int fps = 15)
    {
        _isScanning = true;
        await _camera.StartCaptureAsync(fps);

        _camera.OnFrameCaptured += (_, frame) =>
        {
            if (!_isScanning) return;
            if (DateTime.UtcNow - _lastScanTime < _scanCooldown) return;

            _lastScanTime = DateTime.UtcNow;
            var result = TryDecodeQr(frame);
            if (result != null)
            {
                OnQRCodeDecoded?.Invoke(this, result);
            }
        };
    }

    public Task StopScanningAsync()
    {
        _isScanning = false;
        return _camera.StopCaptureAsync();
    }

    private string? TryDecodeQr(byte[] frame)
    {
        var luminance = ExtractLuminance(frame);
        var finderPatterns = LocateFinderPatterns(luminance);
        if (finderPatterns.Count < 3) return null;

        var qrRegion = ExtractQrRegion(luminance, finderPatterns);
        var decodedBits = DecodeBits(qrRegion);

        if (decodedBits.Length < 32) return null;

        try
        {
            var bytes = ConvertBitsToBytes(decodedBits);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }

    private byte[] ExtractLuminance(byte[] bgraFrame)
    {
        var lum = new byte[bgraFrame.Length / 4];
        for (int i = 0; i < lum.Length; i++)
        {
            var b = bgraFrame[i * 4];
            var g = bgraFrame[i * 4 + 1];
            var r = bgraFrame[i * 4 + 2];
            lum[i] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
        }
        return lum;
    }

    private List<(int x, int y)> LocateFinderPatterns(byte[] luminance)
    {
        var patterns = new List<(int x, int y)>();
        var threshold = luminance.Average(b => (int)b) * 0.7;

        for (int y = 0; y < Math.Min(240, luminance.Length / 320); y++)
        {
            for (int x = 0; x < 320; x++)
            {
                var idx = y * 320 + x;
                if (idx < luminance.Length && luminance[idx] < threshold)
                {
                    patterns.Add((x, y));
                }
            }
        }

        return patterns.Take(3).ToList();
    }

    private byte[] ExtractQrRegion(byte[] luminance, List<(int x, int y)> finderPatterns)
    {
        return luminance.Take(1024).ToArray();
    }

    private bool[] DecodeBits(byte[] region)
    {
        var bits = new List<bool>();
        var threshold = region.Average(b => (int)b) * 0.5;

        for (int i = 0; i < Math.Min(256, region.Length); i++)
        {
            bits.Add(region[i] < threshold);
        }

        return bits.ToArray();
    }

    private byte[] ConvertBitsToBytes(bool[] bits)
    {
        var bytes = new byte[bits.Length / 8];
        for (int i = 0; i < bytes.Length; i++)
        {
            byte value = 0;
            for (int bit = 0; bit < 8; bit++)
                if (bits[i * 8 + bit])
                    value |= (byte)(1 << (7 - bit));
            bytes[i] = value;
        }
        return bytes;
    }
}
namespace SLOY.Infrastructure.Hardware.Sensors;

public class MagnetometerProcessor
{
    private readonly Queue<(double x, double y, double z)> _buffer = new(100);
    private readonly double _threshold;
    private (double x, double y, double z) _baseline;

    public bool IsCalibrated { get; private set; }
    public event EventHandler<byte[]>? OnSignalDetected;

    public MagnetometerProcessor(double threshold = 15.0)
    {
        _threshold = threshold;
    }

    public void Calibrate(IEnumerable<(double x, double y, double z)> samples)
    {
        var list = samples.ToList();
        if (list.Count == 0) return;

        _baseline = (
            list.Average(s => s.x),
            list.Average(s => s.y),
            list.Average(s => s.z)
        );
        IsCalibrated = true;
    }

    public void ProcessReading(double x, double y, double z)
    {
        _buffer.Enqueue((x, y, z));
        if (_buffer.Count > 100) _buffer.Dequeue();

        if (!IsCalibrated)
        {
            if (_buffer.Count >= 20)
                Calibrate(_buffer);
            return;
        }

        var deviation = Math.Sqrt(
            Math.Pow(x - _baseline.x, 2) +
            Math.Pow(y - _baseline.y, 2) +
            Math.Pow(z - _baseline.z, 2)
        );

        if (deviation > _threshold)
        {
            var encoded = EncodeDeviation(x, y, z, deviation);
            OnSignalDetected?.Invoke(this, encoded);
        }
    }

    private byte[] EncodeDeviation(double x, double y, double z, double deviation)
    {
        var data = new byte[16];
        BitConverter.GetBytes(x).CopyTo(data, 0);
        BitConverter.GetBytes(y).CopyTo(data, 4);
        BitConverter.GetBytes(z).CopyTo(data, 8);
        BitConverter.GetBytes(deviation).CopyTo(data, 12);
        return data;
    }

    public double GetFieldStrength()
    {
        if (_buffer.Count == 0) return 0;
        var last = _buffer.Last();
        return Math.Sqrt(last.x * last.x + last.y * last.y + last.z * last.z);
    }
}
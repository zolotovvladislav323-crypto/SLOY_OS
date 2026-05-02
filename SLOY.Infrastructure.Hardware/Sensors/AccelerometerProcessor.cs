namespace SLOY.Infrastructure.Hardware.Sensors;

public class AccelerometerProcessor
{
    private readonly Queue<(double x, double y, double z)> _buffer = new(50);
    private readonly double _shakeThreshold;
    private int _shakeCount;
    private DateTime _lastShakeTime = DateTime.MinValue;

    public bool IsShaking { get; private set; }
    public event EventHandler<int>? OnShakeDetected;
    public event EventHandler<(double x, double y, double z)>? OnMovementDetected;

    public AccelerometerProcessor(double shakeThreshold = 12.0)
    {
        _shakeThreshold = shakeThreshold;
    }

    public void ProcessReading(double x, double y, double z)
    {
        _buffer.Enqueue((x, y, z));
        if (_buffer.Count > 50) _buffer.Dequeue();

        DetectShake(x, y, z);
        DetectMovement(x, y, z);
    }

    private void DetectShake(double x, double y, double z)
    {
        var magnitude = Math.Sqrt(x * x + y * y + z * z);
        var gravity = 9.81;

        if (Math.Abs(magnitude - gravity) > _shakeThreshold)
        {
            var now = DateTime.UtcNow;
            if ((now - _lastShakeTime).TotalMilliseconds < 500)
            {
                _shakeCount++;
                if (_shakeCount >= 3 && !IsShaking)
                {
                    IsShaking = true;
                    OnShakeDetected?.Invoke(this, _shakeCount);
                }
            }
            else
            {
                _shakeCount = 1;
                IsShaking = false;
            }
            _lastShakeTime = now;
        }
        else if ((DateTime.UtcNow - _lastShakeTime).TotalMilliseconds > 1000)
        {
            IsShaking = false;
            _shakeCount = 0;
        }
    }

    private void DetectMovement(double x, double y, double z)
    {
        if (_buffer.Count < 10) return;

        var avgX = _buffer.Average(v => v.x);
        var avgY = _buffer.Average(v => v.y);
        var avgZ = _buffer.Average(v => v.z);

        var delta = Math.Sqrt(
            Math.Pow(x - avgX, 2) +
            Math.Pow(y - avgY, 2) +
            Math.Pow(z - avgZ, 2)
        );

        if (delta > 1.5)
            OnMovementDetected?.Invoke(this, (x, y, z));
    }

    public (double pitch, double roll) GetOrientation()
    {
        if (_buffer.Count == 0) return (0, 0);

        var last = _buffer.Last();
        var pitch = Math.Atan2(last.x, Math.Sqrt(last.y * last.y + last.z * last.z)) * 180 / Math.PI;
        var roll = Math.Atan2(last.y, last.z) * 180 / Math.PI;

        return (pitch, roll);
    }
}
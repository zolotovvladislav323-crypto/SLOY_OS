namespace SLOY.DspEngine.Filters;

public class KalmanFilter
{
    private double _q;
    private double _r;
    private double _p;
    private double _k;
    private double _x;
    private bool _initialized;

    public double ProcessNoise { get => _q; set => _q = value; }
    public double MeasurementNoise { get => _r; set => _r = value; }
    public double State => _x;
    public double Variance => _p;

    public KalmanFilter(double processNoise = 0.01, double measurementNoise = 0.1)
    {
        _q = processNoise;
        _r = measurementNoise;
    }

    public double Update(double measurement)
    {
        if (!_initialized)
        {
            _x = measurement;
            _p = 1;
            _initialized = true;
            return _x;
        }

        _p += _q;
        _k = _p / (_p + _r);
        _x += _k * (measurement - _x);
        _p *= (1 - _k);

        return _x;
    }

    public void Reset()
    {
        _initialized = false;
        _x = 0;
        _p = 1;
        _k = 0;
    }

    public double[] Smooth(double[] measurements)
    {
        var result = new double[measurements.Length];
        for (int i = 0; i < measurements.Length; i++)
            result[i] = Update(measurements[i]);
        return result;
    }
}
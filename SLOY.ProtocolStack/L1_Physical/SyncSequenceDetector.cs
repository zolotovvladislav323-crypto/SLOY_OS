namespace SLOY.ProtocolStack.L1_Physical;

public class SyncSequenceDetector
{
    private readonly double[] _syncPattern;
    private readonly double _threshold;
    private readonly Queue<double> _window;

    public bool IsSynced { get; private set; }
    public event EventHandler? OnSyncAcquired;
    public event EventHandler? OnSyncLost;

    public SyncSequenceDetector(double[]? syncPattern = null, double threshold = 0.8)
    {
        _syncPattern = syncPattern ?? new double[] { 1, 1, 1, -1, -1, 1, -1, -1, 1, 1 };
        _threshold = threshold;
        _window = new Queue<double>(_syncPattern.Length);
    }

    public void ProcessSample(double sample)
    {
        _window.Enqueue(sample);
        if (_window.Count > _syncPattern.Length)
            _window.Dequeue();

        if (_window.Count == _syncPattern.Length)
        {
            var correlation = CalculateCorrelation();
            var wasSynced = IsSynced;
            IsSynced = correlation >= _threshold;

            if (IsSynced && !wasSynced)
                OnSyncAcquired?.Invoke(this, EventArgs.Empty);
            else if (!IsSynced && wasSynced)
                OnSyncLost?.Invoke(this, EventArgs.Empty);
        }
    }

    private double CalculateCorrelation()
    {
        var window = _window.ToArray();
        var numerator = 0.0;
        var denomA = 0.0;
        var denomB = 0.0;

        for (int i = 0; i < _syncPattern.Length; i++)
        {
            numerator += window[i] * _syncPattern[i];
            denomA += window[i] * window[i];
            denomB += _syncPattern[i] * _syncPattern[i];
        }

        return Math.Abs(numerator) / Math.Sqrt(denomA * denomB);
    }

    public void Reset()
    {
        _window.Clear();
        IsSynced = false;
    }
}
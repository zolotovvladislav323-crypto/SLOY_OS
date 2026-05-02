namespace SLOY.Core.Time;

public class DriftCompensator
{
    private readonly Queue<double> _samples = new(100);
    private double _correctionFactor = 1.0;

    public void AddSample(double driftPpm)
    {
        _samples.Enqueue(driftPpm);
        if (_samples.Count > 100) _samples.Dequeue();
        _correctionFactor = 1.0 - _samples.Average() / 1_000_000;
    }

    public long Compensate(long ticks)
    {
        return (long)(ticks * _correctionFactor);
    }

    public void Reset()
    {
        _samples.Clear();
        _correctionFactor = 1.0;
    }
}
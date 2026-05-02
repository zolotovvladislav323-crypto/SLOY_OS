using System.Diagnostics;

namespace SLOY.Core.Metrics;

public class PerformanceCounter
{
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private long _totalOps;

    public void RecordOp() => Interlocked.Increment(ref _totalOps);
    public long TotalOps => _totalOps;
    public double OpsPerSecond => _totalOps / _sw.Elapsed.TotalSeconds;
    public void Reset() { _sw.Restart(); _totalOps = 0; }
}
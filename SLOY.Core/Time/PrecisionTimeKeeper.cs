using System.Diagnostics;

namespace SLOY.Core.Time;

public class PrecisionTimeKeeper
{
    private readonly Stopwatch _stopwatch;
    private readonly long _baseTicks;
    private double _driftPpm;

    public long NowUtcTicks => _baseTicks + _stopwatch.ElapsedTicks;
    public long NowUnixMilliseconds => NowUtcTicks / TimeSpan.TicksPerMillisecond;
    public double DriftPpm => _driftPpm;

    public PrecisionTimeKeeper()
    {
        _baseTicks = DateTimeOffset.UtcNow.Ticks;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Synchronize(long referenceTicks)
    {
        var currentTicks = NowUtcTicks;
        var diff = referenceTicks - currentTicks;
        _driftPpm = (double)diff / currentTicks * 1_000_000;
    }

    public DateTimeOffset GetUtcNow() => new(NowUtcTicks, TimeSpan.Zero);
}
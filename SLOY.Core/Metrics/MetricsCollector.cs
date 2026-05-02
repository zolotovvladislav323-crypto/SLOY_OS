using System.Collections.Concurrent;

namespace SLOY.Core.Metrics;

public class MetricsCollector
{
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, List<double>> _gauges = new();

    public void Increment(string name) => _counters.AddOrUpdate(name, 1, (_, v) => v + 1);
    public void Record(string name, double value) => _gauges.AddOrUpdate(name, new List<double> { value }, (_, list) => { list.Add(value); return list; });
    public long GetCounter(string name) => _counters.GetValueOrDefault(name);
    public double GetAverage(string name) => _gauges.TryGetValue(name, out var list) && list.Count > 0 ? list.Average() : 0;
    public void Reset() { _counters.Clear(); _gauges.Clear(); }
}
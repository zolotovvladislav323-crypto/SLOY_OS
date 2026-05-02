using System.Diagnostics;

namespace SLOY.Core.Profiling;

public class SloyProfiler
{
    private readonly Dictionary<string, Stopwatch> _timers = new();
    private readonly Dictionary<string, List<long>> _results = new();

    public void Start(string key) { _timers[key] = Stopwatch.StartNew(); }

    public void Stop(string key)
    {
        if (_timers.TryGetValue(key, out var sw))
        {
            sw.Stop();
            if (!_results.ContainsKey(key)) _results[key] = new();
            _results[key].Add(sw.ElapsedMilliseconds);
        }
    }

    public (long avg, long min, long max) GetStats(string key)
    {
        if (!_results.TryGetValue(key, out var list) || list.Count == 0) return (0, 0, 0);
        return ((long)list.Average(), list.Min(), list.Max());
    }

    public Dictionary<string, (long, long, long)> GetAllStats() => _results.ToDictionary(k => k.Key, k => GetStats(k.Key));
}
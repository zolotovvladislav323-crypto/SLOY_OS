namespace SLOY.Orchestrator.ChannelBonding;

public class ChannelAggregator
{
    private readonly Dictionary<string, ChannelStats> _channels = new();

    public void ReportLatency(string channel, double ms)
    {
        if (!_channels.ContainsKey(channel)) _channels[channel] = new ChannelStats();
        _channels[channel].AddLatency(ms);
    }

    public void ReportBandwidth(string channel, double kbps)
    {
        if (!_channels.ContainsKey(channel)) _channels[channel] = new ChannelStats();
        _channels[channel].AddBandwidth(kbps);
    }

    public string? SelectBestChannel()
    {
        return _channels.OrderByDescending(c => c.Value.BandwidthScore)
                        .ThenBy(c => c.Value.LatencyScore)
                        .FirstOrDefault().Key;
    }

    public Dictionary<string, (double latency, double bandwidth)> GetStats()
    {
        return _channels.ToDictionary(k => k.Key, v => (v.Value.AvgLatency, v.Value.AvgBandwidth));
    }

    private class ChannelStats
    {
        private readonly List<double> _latencies = new();
        private readonly List<double> _bandwidths = new();
        public double AvgLatency => _latencies.Count > 0 ? _latencies.Average() : double.MaxValue;
        public double AvgBandwidth => _bandwidths.Count > 0 ? _bandwidths.Average() : 0;
        public double LatencyScore => 1000 / (AvgLatency + 1);
        public double BandwidthScore => AvgBandwidth;
        public void AddLatency(double ms) { _latencies.Add(ms); if (_latencies.Count > 100) _latencies.RemoveAt(0); }
        public void AddBandwidth(double kbps) { _bandwidths.Add(kbps); if (_bandwidths.Count > 100) _bandwidths.RemoveAt(0); }
    }
}
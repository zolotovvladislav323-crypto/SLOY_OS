namespace SLOY.Orchestrator.PeerScoring;

public class PeerScoringEngine
{
    private readonly Dictionary<string, PeerScore> _scores = new();

    public void RecordRelaySuccess(string peerId) => UpdateScore(peerId, s => s with { RelaysCompleted = s.RelaysCompleted + 1 });
    public void RecordRelayFailure(string peerId) => UpdateScore(peerId, s => s with { RelaysFailed = s.RelaysFailed + 1 });
    public void RecordUptime(string peerId, TimeSpan uptime) => UpdateScore(peerId, s => s with { UptimeHours = s.UptimeHours + uptime.TotalHours });
    public void RecordLatency(string peerId, double ms) => UpdateScore(peerId, s => s with { AverageLatencyMs = (s.AverageLatencyMs * s.TotalSamples + ms) / (s.TotalSamples + 1), TotalSamples = s.TotalSamples + 1 });

    public double GetScore(string peerId) => _scores.TryGetValue(peerId, out var s) ? s.CalculateScore() : 0;

    public List<(string peerId, double score)> GetTopPeers(int count = 10)
    {
        return _scores.OrderByDescending(x => x.Value.CalculateScore())
                      .Take(count)
                      .Select(x => (x.Key, x.Value.CalculateScore()))
                      .ToList();
    }

    private void UpdateScore(string peerId, Func<PeerScore, PeerScore> updater)
    {
        _scores[peerId] = updater(_scores.GetValueOrDefault(peerId, new PeerScore()));
    }
}
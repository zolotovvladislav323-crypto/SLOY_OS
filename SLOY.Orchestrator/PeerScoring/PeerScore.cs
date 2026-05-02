namespace SLOY.Orchestrator.PeerScoring;

public record PeerScore
{
    public long RelaysCompleted { get; init; }
    public long RelaysFailed { get; init; }
    public double UptimeHours { get; init; }
    public double AverageLatencyMs { get; init; }
    public int TotalSamples { get; init; }

    public double CalculateScore()
    {
        var successRate = RelaysCompleted + RelaysFailed > 0
            ? (double)RelaysCompleted / (RelaysCompleted + RelaysFailed)
            : 0.5;
        var latencyScore = AverageLatencyMs > 0 ? 100 / (AverageLatencyMs + 10) : 1;
        var uptimeScore = Math.Min(UptimeHours / 168, 1.0);

        return (successRate * 0.4 + latencyScore * 0.3 + uptimeScore * 0.3) * 100;
    }
}
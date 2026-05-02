namespace SLOY.Orchestrator.Failover;

public class FailoverPolicy
{
    public int MaxFailures { get; init; } = 3;
    public TimeSpan ResetWindow { get; init; } = TimeSpan.FromMinutes(1);
    public string? FallbackService { get; init; }
}
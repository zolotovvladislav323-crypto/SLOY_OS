namespace SLOY.Core.Health;

public class HealthChecker
{
    private readonly List<IHealthCheck> _checks = new();

    public HealthChecker Add(IHealthCheck check) { _checks.Add(check); return this; }

    public async Task<HealthReport> CheckAllAsync()
    {
        var results = new List<HealthCheckResult>();
        foreach (var check in _checks)
        {
            try { results.Add(await check.CheckAsync()); }
            catch { results.Add(HealthCheckResult.Unhealthy(check.GetType().Name, "Exception")); }
        }
        return new HealthReport(results);
    }
}

public interface IHealthCheck
{
    Task<HealthCheckResult> CheckAsync();
}

public class HealthCheckResult
{
    public string Component { get; init; } = "";
    public bool IsHealthy { get; init; }
    public string? Message { get; init; }
    public static HealthCheckResult Healthy(string c) => new() { Component = c, IsHealthy = true };
    public static HealthCheckResult Unhealthy(string c, string msg) => new() { Component = c, IsHealthy = false, Message = msg };
}

public class HealthReport
{
    public bool IsHealthy { get; }
    public List<HealthCheckResult> Results { get; }

    public HealthReport(List<HealthCheckResult> results)
    {
        Results = results;
        IsHealthy = results.All(r => r.IsHealthy);
    }
}
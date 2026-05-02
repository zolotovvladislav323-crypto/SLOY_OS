namespace SLOY.Orchestrator.Failover;

public class FailoverManager
{
    private readonly Dictionary<string, FailoverPolicy> _policies = new();
    private readonly Dictionary<string, int> _failureCounts = new();

    public void Register(string service, FailoverPolicy policy)
    {
        _policies[service] = policy;
    }

    public void RecordSuccess(string service) => _failureCounts[service] = 0;

    public void RecordFailure(string service)
    {
        if (!_failureCounts.ContainsKey(service)) _failureCounts[service] = 0;
        _failureCounts[service]++;
    }

    public bool ShouldFailover(string service)
    {
        if (!_policies.TryGetValue(service, out var policy)) return false;
        return _failureCounts.GetValueOrDefault(service) >= policy.MaxFailures;
    }

    public async Task ExecuteWithFailoverAsync<T>(string service, Func<Task<T>> primary, Func<Task<T>> secondary)
    {
        try
        {
            await primary();
            RecordSuccess(service);
        }
        catch
        {
            RecordFailure(service);
            if (ShouldFailover(service))
                await secondary();
            else throw;
        }
    }
}
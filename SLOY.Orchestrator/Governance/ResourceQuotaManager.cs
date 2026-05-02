namespace SLOY.Orchestrator.Governance;

public class ResourceQuotaManager
{
    private readonly Dictionary<string, ResourceQuota> _quotas = new();
    private readonly Dictionary<string, double> _usage = new();

    public void SetQuota(string nodeId, double maxStorageMb, double maxBandwidthKbps, int maxConnections)
    {
        _quotas[nodeId] = new ResourceQuota
        {
            MaxStorageMb = maxStorageMb,
            MaxBandwidthKbps = maxBandwidthKbps,
            MaxConnections = maxConnections
        };

        if (!_usage.ContainsKey(nodeId))
            _usage[nodeId] = 0;
    }

    public bool CanStore(string nodeId, double sizeMb)
    {
        if (!_quotas.TryGetValue(nodeId, out var quota)) return true;
        return _usage[nodeId] + sizeMb <= quota.MaxStorageMb;
    }

    public bool CanTransfer(string nodeId, double bandwidthKbps)
    {
        if (!_quotas.TryGetValue(nodeId, out var quota)) return true;

        var currentUsage = GetCurrentBandwidth(nodeId);
        return currentUsage + bandwidthKbps <= quota.MaxBandwidthKbps;
    }

    public bool CanConnect(string nodeId)
    {
        if (!_quotas.TryGetValue(nodeId, out var quota)) return true;

        var activeConnections = _usage.Where(u => u.Key.StartsWith(nodeId + ":conn:"))
                                      .Sum(u => u.Value);
        return activeConnections < quota.MaxConnections;
    }

    public void ReportStorage(string nodeId, double mb)
    {
        _usage[nodeId] = mb;
    }

    public void ReportConnection(string nodeId, string connectionId, bool active)
    {
        var key = $"{nodeId}:conn:{connectionId}";
        _usage[key] = active ? 1 : 0;
    }

    public void ResetQuota(string nodeId)
    {
        _quotas.Remove(nodeId);
        _usage.Remove(nodeId);
        var connKeys = _usage.Keys.Where(k => k.StartsWith(nodeId + ":conn:")).ToList();
        foreach (var key in connKeys)
            _usage.Remove(key);
    }

    private double GetCurrentBandwidth(string nodeId)
    {
        return _usage.Where(u => u.Key.StartsWith(nodeId + ":bw:"))
                     .Sum(u => u.Value);
    }

    private class ResourceQuota
    {
        public double MaxStorageMb { get; init; }
        public double MaxBandwidthKbps { get; init; }
        public int MaxConnections { get; init; }
    }
}
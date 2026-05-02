using SLOY.Shared.Models;

namespace SLOY.Orchestrator;

public class LoadBalancer
{
    private readonly Dictionary<string, NodeLoad> _nodeLoads = new();
    private readonly int _maxLoadPerNode;
    private readonly Random _random = new();

    public LoadBalancer(int maxLoadPerNode = 100)
    {
        _maxLoadPerNode = maxLoadPerNode;
    }

    public void RegisterNode(string nodeId)
    {
        if (!_nodeLoads.ContainsKey(nodeId))
            _nodeLoads[nodeId] = new NodeLoad { NodeId = nodeId };
    }

    public void UnregisterNode(string nodeId)
    {
        _nodeLoads.Remove(nodeId);
    }

    public void ReportPacket(string nodeId, Packet packet)
    {
        if (_nodeLoads.TryGetValue(nodeId, out var load))
        {
            load.PacketCount++;
            load.BytesTransferred += packet.Size;
            load.LastActivity = DateTime.UtcNow;
        }
    }

    public string? SelectLeastLoadedNode(IEnumerable<string> candidates)
    {
        var available = candidates
            .Where(c => _nodeLoads.ContainsKey(c) && _nodeLoads[c].PacketCount < _maxLoadPerNode)
            .ToList();

        if (available.Count == 0) return null;

        return available.OrderBy(c => _nodeLoads[c].PacketCount)
                        .ThenBy(c => _random.Next())
                        .FirstOrDefault();
    }

    public List<string> GetTopLoadedNodes(int count = 5)
    {
        return _nodeLoads.Values
            .OrderByDescending(n => n.PacketCount)
            .Take(count)
            .Select(n => n.NodeId)
            .ToList();
    }

    public Dictionary<string, double> GetLoadDistribution()
    {
        var total = _nodeLoads.Values.Sum(n => n.PacketCount);
        if (total == 0) return new Dictionary<string, double>();

        return _nodeLoads.ToDictionary(
            k => k.Key,
            v => v.Value.PacketCount / (double)total
        );
    }

    private class NodeLoad
    {
        public string NodeId { get; init; } = string.Empty;
        public int PacketCount { get; set; }
        public long BytesTransferred { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }
}
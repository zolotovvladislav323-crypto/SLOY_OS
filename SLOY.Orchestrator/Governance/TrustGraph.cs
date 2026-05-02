namespace SLOY.Orchestrator.Governance;

public class TrustGraph
{
    private readonly Dictionary<string, TrustNode> _nodes = new();
    private readonly Dictionary<(string, string), double> _edges = new();

    public void AddNode(string nodeId, double initialTrust = 0.5)
    {
        if (!_nodes.ContainsKey(nodeId))
            _nodes[nodeId] = new TrustNode { Id = nodeId, TrustScore = initialTrust };
    }

    public void RemoveNode(string nodeId)
    {
        _nodes.Remove(nodeId);
        var edgesToRemove = _edges.Keys.Where(k => k.Item1 == nodeId || k.Item2 == nodeId).ToList();
        foreach (var edge in edgesToRemove)
            _edges.Remove(edge);
    }

    public void SetTrust(string fromNodeId, string toNodeId, double score)
    {
        AddNode(fromNodeId);
        AddNode(toNodeId);

        score = Math.Clamp(score, 0, 1);
        _edges[(fromNodeId, toNodeId)] = score;
    }

    public double GetTrust(string fromNodeId, string toNodeId)
    {
        if (_edges.TryGetValue((fromNodeId, toNodeId), out var direct))
            return direct;

        var path = FindTrustPath(fromNodeId, toNodeId);
        return path.Any() ? path.Average() : 0.1;
    }

    public List<string> GetTrustedNodes(string nodeId, double threshold = 0.5)
    {
        return _edges
            .Where(e => e.Key.Item1 == nodeId && e.Value >= threshold)
            .Select(e => e.Key.Item2)
            .ToList();
    }

    private List<double> FindTrustPath(string from, string to, int maxDepth = 4)
    {
        var visited = new HashSet<string> { from };
        var queue = new Queue<(string node, List<double> scores)>();
        queue.Enqueue((from, new List<double>()));

        while (queue.Count > 0)
        {
            var (current, scores) = queue.Dequeue();

            foreach (var edge in _edges.Where(e => e.Key.Item1 == current))
            {
                if (visited.Contains(edge.Key.Item2)) continue;
                if (scores.Count >= maxDepth) continue;

                var newScores = new List<double>(scores) { edge.Value };

                if (edge.Key.Item2 == to)
                    return newScores;

                visited.Add(edge.Key.Item2);
                queue.Enqueue((edge.Key.Item2, newScores));
            }
        }

        return new List<double>();
    }

    public Dictionary<string, double> GetTrustScores()
    {
        return _nodes.ToDictionary(n => n.Key, n => n.Value.TrustScore);
    }

    public void DecayTrust(double factor = 0.99)
    {
        foreach (var edge in _edges.Keys.ToList())
        {
            _edges[edge] *= factor;
            if (_edges[edge] < 0.05)
                _edges.Remove(edge);
        }

        foreach (var node in _nodes.Values)
            node.TrustScore *= factor;
    }

    public void BoostTrust(string fromNodeId, string toNodeId, double amount)
    {
        if (_edges.TryGetValue((fromNodeId, toNodeId), out var current))
            _edges[(fromNodeId, toNodeId)] = Math.Min(1.0, current + amount);
        else
            SetTrust(fromNodeId, toNodeId, 0.5 + amount);
    }

    private class TrustNode
    {
        public string Id { get; init; } = string.Empty;
        public double TrustScore { get; set; }
    }
}
using System.Security.Cryptography;
using System.Text;

namespace SLOY.Persistence.Secure.Ledger;

public class DirectedAcyclicGraph
{
    private readonly Dictionary<string, DagNode> _nodes = new();
    private readonly List<string> _tips = new();

    public int NodeCount => _nodes.Count;
    public int TipCount => _tips.Count;
    public IReadOnlyList<string> Tips => _tips.AsReadOnly();

    public string AddNode(byte[] data, string? parent1 = null, string? parent2 = null)
    {
        var id = ComputeNodeId(data, parent1, parent2);
        var node = new DagNode
        {
            Id = id,
            Data = data,
            Parent1 = parent1,
            Parent2 = parent2,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _nodes[id] = node;

        if (parent1 != null && _tips.Contains(parent1))
            _tips.Remove(parent1);
        if (parent2 != null && _tips.Contains(parent2))
            _tips.Remove(parent2);

        _tips.Add(id);

        return id;
    }

    public DagNode? GetNode(string id) => _nodes.TryGetValue(id, out var node) ? node : null;

    public List<DagNode> GetPath(string fromId, string toId)
    {
        var path = new List<DagNode>();
        var visited = new HashSet<string>();
        var queue = new Queue<string>();

        queue.Enqueue(fromId);
        visited.Add(fromId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!_nodes.TryGetValue(current, out var node)) continue;

            path.Add(node);
            if (current == toId) return path;

            if (node.Parent1 != null && visited.Add(node.Parent1))
                queue.Enqueue(node.Parent1);
            if (node.Parent2 != null && visited.Add(node.Parent2))
                queue.Enqueue(node.Parent2);
        }

        return path;
    }

    public bool ValidateIntegrity()
    {
        foreach (var node in _nodes.Values)
        {
            var expectedId = ComputeNodeId(node.Data, node.Parent1, node.Parent2);
            if (node.Id != expectedId) return false;

            if (node.Parent1 != null && !_nodes.ContainsKey(node.Parent1)) return false;
            if (node.Parent2 != null && !_nodes.ContainsKey(node.Parent2)) return false;
        }
        return true;
    }

    private static string ComputeNodeId(byte[] data, string? parent1, string? parent2)
    {
        var input = data.ToList();
        if (parent1 != null) input.AddRange(Encoding.UTF8.GetBytes(parent1));
        if (parent2 != null) input.AddRange(Encoding.UTF8.GetBytes(parent2));

        var hash = SHA256.HashData(input.ToArray());
        return Convert.ToHexString(hash)[..32];
    }
}

public class DagNode
{
    public string Id { get; init; } = string.Empty;
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public string? Parent1 { get; init; }
    public string? Parent2 { get; init; }
    public long Timestamp { get; init; }
}
namespace SLOY.Persistence.Secure.Ledger;

public class DAGPruner
{
    private readonly DirectedAcyclicGraph _dag;
    private readonly long _maxAgeMs;
    private readonly int _maxDepth;

    public DAGPruner(DirectedAcyclicGraph dag, long maxAgeMs = 86400000, int maxDepth = 1000)
    {
        _dag = dag;
        _maxAgeMs = maxAgeMs;
        _maxDepth = maxDepth;
    }

    public List<string> FindPrunableNodes()
    {
        var prunable = new List<string>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var tipId in _dag.Tips)
        {
            var tip = _dag.GetNode(tipId);
            if (tip == null) continue;

            var path = _dag.GetPath(tipId, tip.Parent1 ?? tipId);
            var depth = path.Count;

            for (int i = 0; i < path.Count; i++)
            {
                var node = path[i];
                var age = now - node.Timestamp;

                if (age > _maxAgeMs || (depth - i) > _maxDepth)
                {
                    prunable.Add(node.Id);
                }
            }
        }

        return prunable.Distinct().ToList();
    }

    public int GetPrunableCount() => FindPrunableNodes().Count;
}
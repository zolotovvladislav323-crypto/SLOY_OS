namespace SLOY.ProtocolStack.L3_Network;

public class PathOptimizer
{
    private readonly Dictionary<string, Dictionary<string, double>> _graph = new();

    public void AddLink(string nodeA, string nodeB, double cost)
    {
        if (!_graph.ContainsKey(nodeA)) _graph[nodeA] = new();
        if (!_graph.ContainsKey(nodeB)) _graph[nodeB] = new();

        _graph[nodeA][nodeB] = cost;
        _graph[nodeB][nodeA] = cost;
    }

    public void RemoveLink(string nodeA, string nodeB)
    {
        _graph[nodeA]?.Remove(nodeB);
        _graph[nodeB]?.Remove(nodeA);
    }

    public List<string>? FindShortestPath(string source, string target)
    {
        if (!_graph.ContainsKey(source) || !_graph.ContainsKey(target))
            return null;

        var distances = new Dictionary<string, double>();
        var previous = new Dictionary<string, string?>();
        var unvisited = new HashSet<string>();

        foreach (var node in _graph.Keys)
        {
            distances[node] = double.MaxValue;
            previous[node] = null;
            unvisited.Add(node);
        }

        distances[source] = 0;

        while (unvisited.Count > 0)
        {
            var current = unvisited.OrderBy(n => distances[n]).First();

            if (current == target) break;
            if (distances[current] == double.MaxValue) break;

            unvisited.Remove(current);

            if (!_graph.ContainsKey(current)) continue;

            foreach (var neighbor in _graph[current])
            {
                if (!unvisited.Contains(neighbor.Key)) continue;

                var alt = distances[current] + neighbor.Value;
                if (alt < distances[neighbor.Key])
                {
                    distances[neighbor.Key] = alt;
                    previous[neighbor.Key] = current;
                }
            }
        }

        if (previous[target] == null && source != target) return null;

        var path = new List<string>();
        var nodePtr = target;

        while (nodePtr != null)
        {
            path.Add(nodePtr);
            nodePtr = previous[nodePtr];
        }

        path.Reverse();
        return path;
    }

    public double CalculatePathCost(List<string> path)
    {
        double cost = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (_graph.TryGetValue(path[i], out var neighbors) &&
                neighbors.TryGetValue(path[i + 1], out var linkCost))
            {
                cost += linkCost;
            }
            else return double.MaxValue;
        }
        return cost;
    }
}
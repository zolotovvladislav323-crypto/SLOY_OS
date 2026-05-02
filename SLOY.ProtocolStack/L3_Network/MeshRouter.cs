using SLOY.Core.Interfaces;
using SLOY.Shared.Models;

namespace SLOY.ProtocolStack.L3_Network;

public class MeshRouter : IMeshRouter
{
    private readonly Dictionary<string, List<RouteEntry>> _routingTable = new();
    private readonly Dictionary<string, DateTime> _peerLastSeen = new();
    private readonly TimeSpan _peerTimeout = TimeSpan.FromMinutes(5);

    public event EventHandler<Packet>? OnPacketReceived;
    public event EventHandler<string>? OnPeerDiscovered;
    public event EventHandler<string>? OnPeerLost;

    public async Task SendAsync(Packet packet)
    {
        if (_routingTable.TryGetValue(packet.ReceiverId, out var routes))
        {
            var bestRoute = routes.OrderBy(r => r.HopCount).FirstOrDefault();
            if (bestRoute != null)
            {
                // Передаём следующему узлу
                await Task.CompletedTask;
            }
        }
    }

    public Task<Packet?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Packet?>(null);
    }

    public async Task BroadcastAsync(Packet packet)
    {
        foreach (var peer in _peerLastSeen.Keys)
        {
            packet.ReceiverId = peer;
            await SendAsync(packet);
        }
    }

    public Task<List<string>> DiscoverPeersAsync()
    {
        return Task.FromResult(_peerLastSeen.Keys.ToList());
    }

    public Task<int> GetHopCountAsync(string targetNodeId)
    {
        if (_routingTable.TryGetValue(targetNodeId, out var routes))
            return Task.FromResult(routes.Min(r => r.HopCount));
        return Task.FromResult(-1);
    }

    public Task<bool> IsNodeReachableAsync(string nodeId)
    {
        return Task.FromResult(_peerLastSeen.ContainsKey(nodeId) &&
                               DateTime.UtcNow - _peerLastSeen[nodeId] < _peerTimeout);
    }

    public void AddRoute(string destination, string nextHop, int hopCount, double linkQuality)
    {
        if (!_routingTable.ContainsKey(destination))
            _routingTable[destination] = new List<RouteEntry>();

        _routingTable[destination].RemoveAll(r => r.NextHop == nextHop);
        _routingTable[destination].Add(new RouteEntry(nextHop, hopCount, linkQuality));

        _peerLastSeen[destination] = DateTime.UtcNow;
    }

    public void RemoveRoute(string destination, string nextHop)
    {
        if (_routingTable.TryGetValue(destination, out var routes))
        {
            routes.RemoveAll(r => r.NextHop == nextHop);
            if (routes.Count == 0)
                _routingTable.Remove(destination);
        }
    }

    public Dictionary<string, int> GetRoutingTableSnapshot()
    {
        return _routingTable.ToDictionary(k => k.Key, v => v.Value.Min(r => r.HopCount));
    }

    private class RouteEntry
    {
        public string NextHop { get; }
        public int HopCount { get; }
        public double LinkQuality { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public RouteEntry(string nextHop, int hopCount, double linkQuality)
        {
            NextHop = nextHop;
            HopCount = hopCount;
            LinkQuality = linkQuality;
        }
    }
}
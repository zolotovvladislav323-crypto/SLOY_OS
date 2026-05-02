using SLOY.Shared.Models;

namespace SLOY.Core.Interfaces;

public interface IMeshRouter
{
    Task SendAsync(Packet packet);
    Task<Packet?> ReceiveAsync(CancellationToken cancellationToken = default);
    Task BroadcastAsync(Packet packet);
    Task<List<string>> DiscoverPeersAsync();
    Task<int> GetHopCountAsync(string targetNodeId);
    Task<bool> IsNodeReachableAsync(string nodeId);
    event EventHandler<Packet> OnPacketReceived;
    event EventHandler<string> OnPeerDiscovered;
    event EventHandler<string> OnPeerLost;
}
using SLOY.Core.Interfaces;
using SLOY.Shared.Enums;

namespace SLOY.Orchestrator;

public class MeshLifecycleManager
{
    private readonly IMeshRouter _router;
    private readonly List<string> _connectedPeers = new();
    private NodeStatus _status = NodeStatus.Offline;
    private bool _isRunning;

    public NodeStatus Status => _status;
    public int PeerCount => _connectedPeers.Count;
    public IReadOnlyList<string> ConnectedPeers => _connectedPeers.AsReadOnly();

    public event EventHandler<NodeStatus>? OnStatusChanged;
    public event EventHandler<string>? OnPeerJoined;
    public event EventHandler<string>? OnPeerLeft;

    public MeshLifecycleManager(IMeshRouter router)
    {
        _router = router;
        _router.OnPeerDiscovered += (_, peerId) => AddPeer(peerId);
        _router.OnPeerLost += (_, peerId) => RemovePeer(peerId);
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        SetStatus(NodeStatus.Online);

        _ = Task.Run(async () =>
        {
            while (_isRunning)
            {
                var peers = await _router.DiscoverPeersAsync();
                foreach (var peer in peers.Where(p => !_connectedPeers.Contains(p)))
                    AddPeer(peer);

                foreach (var peer in _connectedPeers.Where(p => !peers.Contains(p)).ToList())
                    RemovePeer(peer);

                await Task.Delay(5000);
            }
        });
    }

    public async Task StopAsync()
    {
        _isRunning = false;
        _connectedPeers.Clear();
        SetStatus(NodeStatus.Offline);
        await Task.CompletedTask;
    }

    public async Task SetAwayAsync()
    {
        SetStatus(NodeStatus.Away);
        await Task.CompletedTask;
    }

    public async Task SetDoNotDisturbAsync()
    {
        SetStatus(NodeStatus.DoNotDisturb);
        await Task.CompletedTask;
    }

    private void AddPeer(string peerId)
    {
        if (!_connectedPeers.Contains(peerId))
        {
            _connectedPeers.Add(peerId);
            OnPeerJoined?.Invoke(this, peerId);
        }
    }

    private void RemovePeer(string peerId)
    {
        if (_connectedPeers.Remove(peerId))
            OnPeerLeft?.Invoke(this, peerId);
    }

    private void SetStatus(NodeStatus status)
    {
        if (_status != status)
        {
            _status = status;
            OnStatusChanged?.Invoke(this, status);
        }
    }
}
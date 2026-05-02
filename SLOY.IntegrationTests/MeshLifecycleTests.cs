using SLOY.Orchestrator;
using SLOY.ProtocolStack.L3_Network;
using SLOY.Shared.Enums;

namespace SLOY.IntegrationTests;

public class MeshLifecycleTests
{
    [Fact]
    public async Task StartAsync_ChangesStatusToOnline()
    {
        var router = new MeshRouter();
        var manager = new MeshLifecycleManager(router);

        NodeStatus? receivedStatus = null;
        manager.OnStatusChanged += (_, status) => receivedStatus = status;

        await manager.StartAsync();
        await Task.Delay(100);

        Assert.Equal(NodeStatus.Online, manager.Status);
        Assert.Equal(NodeStatus.Online, receivedStatus);
    }

    [Fact]
    public async Task StopAsync_ChangesStatusToOffline()
    {
        var router = new MeshRouter();
        var manager = new MeshLifecycleManager(router);

        await manager.StartAsync();
        await manager.StopAsync();

        Assert.Equal(NodeStatus.Offline, manager.Status);
        Assert.Empty(manager.ConnectedPeers);
    }

    [Fact]
    public async Task PeerJoin_AddsToConnectedList()
    {
        var router = new MeshRouter();
        var manager = new MeshLifecycleManager(router);

        string? joinedPeer = null;
        manager.OnPeerJoined += (_, peer) => joinedPeer = peer;

        router.AddRoute("peer1", "peer1", 1, 1.0);

        await manager.StartAsync();
        await Task.Delay(200);

        Assert.Contains("peer1", manager.ConnectedPeers);
    }
}
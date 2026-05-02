using SLOY.ProtocolStack.L3_Network;
using SLOY.Shared.Enums;
using SLOY.Shared.Models;

namespace SLOY.Tests.ProtocolStack;

public class MeshRouterTests
{
    [Fact]
    public async Task DiscoverPeers_InitiallyEmpty()
    {
        var router = new MeshRouter();
        var peers = await router.DiscoverPeersAsync();
        Assert.Empty(peers);
    }

    [Fact]
    public async Task AddRoute_PeerBecomesReachable()
    {
        var router = new MeshRouter();
        router.AddRoute("nodeB", "nodeB", 1, 0.9);

        var reachable = await router.IsNodeReachableAsync("nodeB");
        Assert.True(reachable);
    }

    [Fact]
    public async Task GetHopCount_ReturnsCorrectValue()
    {
        var router = new MeshRouter();
        router.AddRoute("nodeB", "nodeC", 2, 0.8);
        router.AddRoute("nodeB", "nodeD", 3, 0.7);

        var hops = await router.GetHopCountAsync("nodeB");
        Assert.Equal(2, hops);
    }

    [Fact]
    public async Task RemoveRoute_NodeBecomesUnreachable()
    {
        var router = new MeshRouter();
        router.AddRoute("nodeB", "nodeB", 1, 0.9);
        router.RemoveRoute("nodeB", "nodeB");

        var reachable = await router.IsNodeReachableAsync("nodeB");
        Assert.False(reachable);
    }

    [Fact]
    public void GetRoutingTableSnapshot_ReturnsCorrectData()
    {
        var router = new MeshRouter();
        router.AddRoute("nodeA", "nodeX", 1, 0.9);
        router.AddRoute("nodeB", "nodeY", 2, 0.8);

        var snapshot = router.GetRoutingTableSnapshot();
        Assert.Equal(2, snapshot.Count);
        Assert.Equal(1, snapshot["nodeA"]);
        Assert.Equal(2, snapshot["nodeB"]);
    }
}
using SLOY.ProtocolStack.L3_Network;

namespace SLOY.Tests.ProtocolStack;

public class PathOptimizerTests
{
    [Fact]
    public void FindShortestPath_DirectConnection()
    {
        var optimizer = new PathOptimizer();
        optimizer.AddLink("A", "B", 1.0);

        var path = optimizer.FindShortestPath("A", "B");

        Assert.NotNull(path);
        Assert.Equal(2, path!.Count);
        Assert.Equal("A", path[0]);
        Assert.Equal("B", path[1]);
    }

    [Fact]
    public void FindShortestPath_IndirectConnection()
    {
        var optimizer = new PathOptimizer();
        optimizer.AddLink("A", "B", 1.0);
        optimizer.AddLink("B", "C", 1.0);

        var path = optimizer.FindShortestPath("A", "C");

        Assert.NotNull(path);
        Assert.Equal(3, path!.Count);
        Assert.Equal("A", path[0]);
        Assert.Equal("B", path[1]);
        Assert.Equal("C", path[2]);
    }

    [Fact]
    public void FindShortestPath_ChoosesLowestCost()
    {
        var optimizer = new PathOptimizer();
        optimizer.AddLink("A", "B", 10.0);
        optimizer.AddLink("B", "D", 1.0);
        optimizer.AddLink("A", "C", 1.0);
        optimizer.AddLink("C", "D", 1.0);

        var path = optimizer.FindShortestPath("A", "D");

        Assert.NotNull(path);
        Assert.Equal(3, path!.Count);
        Assert.Equal("A", path[0]);
        Assert.Equal("C", path[1]);
        Assert.Equal("D", path[2]);
    }

    [Fact]
    public void FindShortestPath_NoPath_ReturnsNull()
    {
        var optimizer = new PathOptimizer();
        optimizer.AddLink("A", "B", 1.0);

        var path = optimizer.FindShortestPath("A", "X");

        Assert.Null(path);
    }

    [Fact]
    public void CalculatePathCost_ReturnsCorrectSum()
    {
        var optimizer = new PathOptimizer();
        optimizer.AddLink("A", "B", 2.0);
        optimizer.AddLink("B", "C", 3.0);

        var cost = optimizer.CalculatePathCost(new List<string> { "A", "B", "C" });

        Assert.Equal(5.0, cost);
    }
}
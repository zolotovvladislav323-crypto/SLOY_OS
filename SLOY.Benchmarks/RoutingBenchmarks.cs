using BenchmarkDotNet.Attributes;
using SLOY.ProtocolStack.L3_Network;

namespace SLOY.Benchmarks;

[MemoryDiagnoser]
public class RoutingBenchmarks
{
    private PathOptimizer _optimizer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _optimizer = new PathOptimizer();
        for (int i = 0; i < 100; i++)
            _optimizer.AddLink($"n{i}", $"n{i + 1}", 1.0);
    }

    [Benchmark]
    public List<string>? FindPath() => _optimizer.FindShortestPath("n0", "n100");
}
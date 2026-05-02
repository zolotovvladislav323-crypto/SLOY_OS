using BenchmarkDotNet.Attributes;
using SLOY.ProtocolStack.L2_DataLink;
using SLOY.Shared.Models;

namespace SLOY.Benchmarks;

[MemoryDiagnoser]
public class PacketProcessingBenchmarks
{
    private readonly FrameAssembler _assembler = new();
    private byte[] _testData = null!;

    [GlobalSetup]
    public void Setup() => _testData = new byte[1024];

    [Benchmark]
    public byte[] CreateFrame() => _assembler.CreateFrame(_testData);

    [Benchmark]
    public void ProcessBytes()
    {
        var frame = _assembler.CreateFrame(_testData);
        foreach (var b in frame) _assembler.ProcessByte(b);
    }
}
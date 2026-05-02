using BenchmarkDotNet.Attributes;
using SLOY.Cryptography.PostQuantum;

namespace SLOY.Benchmarks;

[MemoryDiagnoser]
public class CryptoBenchmarks
{
    private LatticeEngine _engine = null!;
    private byte[] _publicKey = null!;
    private byte[] _privateKey = null!;
    private byte[] _message = null!;

    [GlobalSetup]
    public void Setup()
    {
        _engine = new LatticeEngine(64, 12289);
        (_publicKey, _privateKey) = _engine.GenerateKeyPair();
        _message = new byte[256];
    }

    [Benchmark]
    public byte[] Encrypt() => _engine.Encrypt(_message, _publicKey);

    [Benchmark]
    public (byte[], byte[]) GenerateKeyPair() => _engine.GenerateKeyPair();
}
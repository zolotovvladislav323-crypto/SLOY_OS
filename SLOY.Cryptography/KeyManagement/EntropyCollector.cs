using System.Diagnostics;
using System.Security.Cryptography;

namespace SLOY.Cryptography.KeyManagement;

public class EntropyCollector
{
    private readonly List<byte> _entropyPool = new();
    private readonly object _lock = new();
    private bool _isCollecting;

    public int PoolSize => _entropyPool.Count;

    public void StartCollecting()
    {
        _isCollecting = true;
        _ = CollectLoopAsync();
    }

    public void StopCollecting()
    {
        _isCollecting = false;
    }

    private async Task CollectLoopAsync()
    {
        while (_isCollecting)
        {
            AddSource(DateTime.UtcNow.Ticks);
            AddSource(Environment.TickCount64);
            AddSource(Stopwatch.GetTimestamp());

            try
            {
                AddSource(GC.GetTotalMemory(false));
                AddSource(Environment.WorkingSet);
            }
            catch { }

            await Task.Delay(100);
        }
    }

    public void AddSource(long value)
    {
        lock (_lock)
        {
            _entropyPool.AddRange(BitConverter.GetBytes(value));
        }
    }

    public void AddSource(byte[] data)
    {
        lock (_lock)
        {
            _entropyPool.AddRange(data);
        }
    }

    public byte[] GetRandomBytes(int count)
    {
        lock (_lock)
        {
            if (_entropyPool.Count < count)
                throw new InvalidOperationException("Недостаточно энтропии в пуле");

            var result = new byte[count];
            _entropyPool.CopyTo(0, result, 0, count);
            _entropyPool.RemoveRange(0, count);
            return result;
        }
    }

    public byte[] GetMixedRandomBytes(int count)
    {
        var raw = GetRandomBytes(count * 2);
        return SHA256.HashData(raw)[..count];
    }
}
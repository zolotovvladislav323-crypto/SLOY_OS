using System.Buffers;

namespace SLOY.Core.Memory;

public class BufferPool
{
    private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;
    private readonly int _bufferSize;

    public BufferPool(int bufferSize = 4096)
    {
        _bufferSize = bufferSize;
    }

    public byte[] Rent() => _pool.Rent(_bufferSize);

    public void Return(byte[] buffer)
    {
        if (buffer != null) _pool.Return(buffer);
    }

    public PooledBuffer RentSafe() => new(_pool.Rent(_bufferSize), this);

    public struct PooledBuffer : IDisposable
    {
        private readonly BufferPool _owner;
        public byte[] Data { get; }

        public PooledBuffer(byte[] data, BufferPool owner)
        {
            Data = data;
            _owner = owner;
        }

        public void Dispose() => _owner.Return(Data);
    }
}
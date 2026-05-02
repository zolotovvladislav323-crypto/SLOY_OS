using System.Runtime.InteropServices;

namespace SLOY.Core.Memory;

public unsafe class ZeroCopyMechanism
{
    private readonly Dictionary<IntPtr, GCHandle> _pinnedBuffers = new();

    public IntPtr Pin(byte[] buffer)
    {
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        var ptr = handle.AddrOfPinnedObject();
        _pinnedBuffers[ptr] = handle;
        return ptr;
    }

    public void Unpin(IntPtr ptr)
    {
        if (_pinnedBuffers.TryGetValue(ptr, out var handle))
        {
            handle.Free();
            _pinnedBuffers.Remove(ptr);
        }
    }

    public unsafe Span<T> AsSpan<T>(IntPtr ptr, int count) where T : unmanaged
        => new((void*)ptr, count);

    public unsafe void Copy(IntPtr source, IntPtr destination, int length)
    {
        Buffer.MemoryCopy((void*)source, (void*)destination, length, length);
    }

    public void ReleaseAll()
    {
        foreach (var handle in _pinnedBuffers.Values)
            handle.Free();
        _pinnedBuffers.Clear();
    }
}
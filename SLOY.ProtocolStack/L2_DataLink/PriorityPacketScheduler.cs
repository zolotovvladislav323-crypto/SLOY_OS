using SLOY.Shared.Enums;
using SLOY.Shared.Models;

namespace SLOY.ProtocolStack.L2_DataLink;

public class PriorityPacketScheduler
{
    private readonly PriorityQueue<Packet, int> _queue = new();

    public int QueueSize => _queue.Count;

    public void Enqueue(Packet packet)
    {
        var priority = packet.Priority switch
        {
            PacketPriority.Critical => 0,
            PacketPriority.High => 1,
            PacketPriority.Normal => 2,
            PacketPriority.Low => 3,
            _ => 2
        };
        _queue.Enqueue(packet, priority);
    }

    public Packet? Dequeue()
    {
        return _queue.TryDequeue(out var packet, out _) ? packet : null;
    }

    public Packet? Peek()
    {
        return _queue.TryPeek(out var packet, out _) ? packet : null;
    }

    public void Clear()
    {
        _queue.Clear();
    }

    public List<Packet> DrainAll()
    {
        var packets = new List<Packet>();
        while (_queue.TryDequeue(out var packet, out _))
            packets.Add(packet);
        return packets;
    }
}
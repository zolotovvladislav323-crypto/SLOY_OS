using SLOY.Shared.Models;

namespace SLOY.UI.Forensics.Diagnostics;

public class PacketFlowView
{
    private readonly Queue<PacketLogEntry> _recentPackets = new(200);
    private long _totalPackets;
    private long _totalBytes;
    private DateTime _startTime = DateTime.UtcNow;
    private readonly Dictionary<string, long> _packetsPerNode = new();

    public int RecentCount => _recentPackets.Count;
    public long TotalPackets => _totalPackets;
    public long TotalBytes => _totalBytes;
    public TimeSpan Uptime => DateTime.UtcNow - _startTime;
    public double PacketsPerSecond => _totalPackets / Uptime.TotalSeconds;
    public double BytesPerSecond => _totalBytes / Uptime.TotalSeconds;

    public event EventHandler<PacketLogEntry>? OnPacketLogged;

    public void LogPacket(Packet packet, PacketDirection direction)
    {
        var entry = new PacketLogEntry
        {
            PacketId = packet.Id,
            Direction = direction,
            Protocol = packet.Protocol,
            Priority = packet.Priority,
            Size = packet.Size,
            Timestamp = DateTime.UtcNow,
            SenderId = packet.SenderId,
            ReceiverId = packet.ReceiverId
        };

        _recentPackets.Enqueue(entry);
        if (_recentPackets.Count > 200)
            _recentPackets.Dequeue();

        _totalPackets++;
        _totalBytes += packet.Size;

        var peerId = direction == PacketDirection.Incoming ? packet.SenderId : packet.ReceiverId;
        if (!_packetsPerNode.ContainsKey(peerId))
            _packetsPerNode[peerId] = 0;
        _packetsPerNode[peerId]++;

        OnPacketLogged?.Invoke(this, entry);
    }

    public List<PacketLogEntry> GetRecentPackets(int count = 50)
    {
        return _recentPackets.TakeLast(count).ToList();
    }

    public Dictionary<string, long> GetPacketsPerNode()
    {
        return _packetsPerNode.OrderByDescending(kv => kv.Value)
                              .Take(10)
                              .ToDictionary(k => k.Key, v => v.Value);
    }

    public FlowStatistics GetStatistics()
    {
        return new FlowStatistics
        {
            TotalPackets = _totalPackets,
            TotalBytes = _totalBytes,
            PacketsPerSecond = PacketsPerSecond,
            BytesPerSecond = BytesPerSecond,
            Uptime = Uptime,
            ActivePeers = _packetsPerNode.Count
        };
    }

    public void Reset()
    {
        _recentPackets.Clear();
        _totalPackets = 0;
        _totalBytes = 0;
        _startTime = DateTime.UtcNow;
        _packetsPerNode.Clear();
    }
}

public class PacketLogEntry
{
    public string PacketId { get; init; } = string.Empty;
    public PacketDirection Direction { get; init; }
    public Shared.Enums.ProtocolType Protocol { get; init; }
    public Shared.Enums.PacketPriority Priority { get; init; }
    public int Size { get; init; }
    public DateTime Timestamp { get; init; }
    public string SenderId { get; init; } = string.Empty;
    public string ReceiverId { get; init; } = string.Empty;
}

public enum PacketDirection
{
    Incoming,
    Outgoing
}

public class FlowStatistics
{
    public long TotalPackets { get; init; }
    public long TotalBytes { get; init; }
    public double PacketsPerSecond { get; init; }
    public double BytesPerSecond { get; init; }
    public TimeSpan Uptime { get; init; }
    public int ActivePeers { get; init; }
}
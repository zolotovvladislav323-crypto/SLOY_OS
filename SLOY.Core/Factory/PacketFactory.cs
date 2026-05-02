using SLOY.Shared.Enums;
using SLOY.Shared.Models;

namespace SLOY.Core.Factory;

public class PacketFactory
{
    public Packet CreateFromMessage(Message m) => new()
    {
        SenderId = m.SenderNodeId,
        ReceiverId = m.SenderNodeId,
        Protocol = ProtocolType.TextMessage,
        Priority = PacketPriority.Normal,
        Payload = System.Text.Encoding.UTF8.GetBytes(m.Content)
    };

    public Packet CreateHeartbeat(string nodeId) => new()
    {
        SenderId = nodeId,
        ReceiverId = "BROADCAST",
        Protocol = ProtocolType.Heartbeat,
        Priority = PacketPriority.Low,
        Payload = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
    };

    public Packet CreateSystemCommand(string targetId, string command) => new()
    {
        SenderId = "SYSTEM",
        ReceiverId = targetId,
        Protocol = ProtocolType.SystemCommand,
        Priority = PacketPriority.Critical,
        Payload = System.Text.Encoding.UTF8.GetBytes(command)
    };
}
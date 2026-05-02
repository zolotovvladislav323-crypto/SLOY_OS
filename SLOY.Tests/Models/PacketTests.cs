using SLOY.Shared.Enums;
using SLOY.Shared.Models;

namespace SLOY.Tests.Models;

public class PacketTests
{
    [Fact]
    public void Constructor_CreatesUniqueId()
    {
        var p1 = new Packet();
        var p2 = new Packet();
        Assert.NotEqual(p1.Id, p2.Id);
    }

    [Fact]
    public void Timestamp_SetToUtcNow()
    {
        var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var packet = new Packet();
        var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.True(packet.Timestamp >= before);
        Assert.True(packet.Timestamp <= after);
    }

    [Fact]
    public void Size_ReturnsPayloadLength()
    {
        var packet = new Packet { Payload = new byte[100] };
        Assert.Equal(100, packet.Size);
    }

    [Fact]
    public void Hash_ChangesWithPayload()
    {
        var p1 = new Packet { Payload = new byte[] { 1, 2, 3 } };
        var p2 = new Packet { Payload = new byte[] { 4, 5, 6 } };
        Assert.NotEqual(p1.Hash, p2.Hash);
    }

    [Fact]
    public void ToString_ContainsIdAndProtocol()
    {
        var packet = new Packet
        {
            SenderId = "sender123",
            ReceiverId = "receiver456",
            Protocol = ProtocolType.TextMessage
        };

        var result = packet.ToString();
        Assert.Contains(packet.Id, result);
        Assert.Contains("sender123", result);
        Assert.Contains("receiver456", result);
        Assert.Contains("TextMessage", result);
    }
}
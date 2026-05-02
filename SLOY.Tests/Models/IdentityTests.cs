using SLOY.Shared.Models;

namespace SLOY.Tests.Models;

public class IdentityTests
{
    [Fact]
    public void Constructor_ValidNickname_CreatesIdentity()
    {
        var identity = new Identity("Ghost_7");
        Assert.Equal("Ghost_7", identity.Nickname);
        Assert.NotNull(identity.NodeId);
        Assert.Equal(12, identity.NodeId.Length);
        Assert.NotNull(identity.Checksum);
    }

    [Fact]
    public void Constructor_EmptyNickname_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Identity(""));
    }

    [Fact]
    public void Constructor_WhitespaceNickname_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Identity("   "));
    }

    [Fact]
    public void Constructor_TooShortNickname_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Identity("A"));
    }

    [Fact]
    public void Constructor_SpecialChars_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Identity("Ghost#7"));
    }

    [Fact]
    public void Constructor_ValidNicknameWithDotsAndDashes_Works()
    {
        var identity = new Identity("Ghost-7.0");
        Assert.Equal("Ghost-7.0", identity.Nickname);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var identity = new Identity("TestUser");
        var result = identity.ToString();
        Assert.Contains("TestUser#", result);
    }

    [Fact]
    public void NodeId_IsUniquePerInstance()
    {
        var id1 = new Identity("User1");
        var id2 = new Identity("User2");
        Assert.NotEqual(id1.NodeId, id2.NodeId);
    }
}
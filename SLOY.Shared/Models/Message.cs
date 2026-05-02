namespace SLOY.Shared.Models;

public class Message
{
    public string Id { get; } = Guid.NewGuid().ToString("N")[..12];
    public string SenderNickname { get; set; } = string.Empty;
    public string SenderNodeId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public bool IsRead { get; set; }
    public bool IsMine { get; set; }

    public string DisplayTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).ToLocalTime().ToString("HH:mm");

    public override string ToString() => $"[{DisplayTime}] {SenderNickname}: {Content}";
}
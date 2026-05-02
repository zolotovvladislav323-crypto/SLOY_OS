namespace SLOY.Orchestrator.Auth;

public class AuthChallenge
{
    public byte[] Nonce { get; init; } = Array.Empty<byte>();
    public long Timestamp { get; init; }
    public string SenderNodeId { get; init; } = string.Empty;
    public string TargetNodeId { get; init; } = string.Empty;
}
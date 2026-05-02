using SLOY.Core.Interfaces;
using SLOY.Shared.Models;

namespace SLOY.Orchestrator.Auth;

public class PeerAuthenticator
{
    private readonly ICryptoProvider _crypto;
    private readonly Identity _identity;
    private readonly Dictionary<string, byte[]> _peerPublicKeys = new();

    public PeerAuthenticator(ICryptoProvider crypto, Identity identity)
    {
        _crypto = crypto;
        _identity = identity;
    }

    public void RegisterPeer(string nodeId, byte[] publicKey)
    {
        _peerPublicKeys[nodeId] = publicKey;
    }

    public AuthChallenge CreateChallenge(string targetNodeId)
    {
        var nonce = _crypto.GenerateRandomBytes(32);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return new AuthChallenge
        {
            Nonce = nonce,
            Timestamp = timestamp,
            SenderNodeId = _identity.NodeId,
            TargetNodeId = targetNodeId
        };
    }

    public bool VerifyChallenge(AuthChallenge challenge, byte[] signature)
    {
        if (!_peerPublicKeys.TryGetValue(challenge.SenderNodeId, out var publicKey))
            return false;

        var age = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - challenge.Timestamp;
        if (Math.Abs(age) > 30000) return false;

        var challengeData = challenge.Nonce
            .Concat(BitConverter.GetBytes(challenge.Timestamp))
            .Concat(System.Text.Encoding.UTF8.GetBytes(challenge.SenderNodeId))
            .ToArray();

        return _crypto.Verify(challengeData, signature, publicKey);
    }

    public byte[] SignChallenge(AuthChallenge challenge)
    {
        var challengeData = challenge.Nonce
            .Concat(BitConverter.GetBytes(challenge.Timestamp))
            .Concat(System.Text.Encoding.UTF8.GetBytes(challenge.SenderNodeId))
            .ToArray();

        return _crypto.Sign(challengeData, _identity.PublicKey ?? Array.Empty<byte>());
    }

    public bool IsPeerKnown(string nodeId) => _peerPublicKeys.ContainsKey(nodeId);
}
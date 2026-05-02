using SLOY.Core.Interfaces;
using SLOY.Shared.Models;

namespace SLOY.Core.Pipeline;

public class SignatureMiddleware : IPacketMiddleware
{
    private readonly ICryptoProvider _crypto;
    private readonly byte[] _privateKey;

    public SignatureMiddleware(ICryptoProvider crypto, byte[] privateKey)
    {
        _crypto = crypto; _privateKey = privateKey;
    }

    public Task<Packet> ProcessAsync(Packet packet, Func<Packet, Task<Packet>> next)
    {
        var sig = _crypto.Sign(packet.Payload, _privateKey);
        var signed = new byte[sig.Length + packet.Payload.Length];
        Array.Copy(sig, 0, signed, 0, sig.Length);
        Array.Copy(packet.Payload, 0, signed, sig.Length, packet.Payload.Length);
        packet.Payload = signed;
        return next(packet);
    }
}
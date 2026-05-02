using SLOY.Core.Interfaces;
using SLOY.Shared.Models;

namespace SLOY.Core.Pipeline;

public class EncryptionMiddleware : IPacketMiddleware
{
    private readonly ICryptoProvider _crypto;
    private readonly byte[] _key;

    public EncryptionMiddleware(ICryptoProvider crypto, byte[] key)
    {
        _crypto = crypto; _key = key;
    }

    public async Task<Packet> ProcessAsync(Packet packet, Func<Packet, Task<Packet>> next)
    {
        packet.Payload = _crypto.Encrypt(packet.Payload, _key);
        return await next(packet);
    }
}
using SLOY.Core.Interfaces;

namespace SLOY.Core.UseCases;

public class EstablishSecureChannelUseCase
{
    private readonly ICryptoProvider _crypto;
    public EstablishSecureChannelUseCase(ICryptoProvider crypto) => _crypto = crypto;

    public async Task<(byte[] sessionKey, byte[] publicKey)> ExecuteAsync(byte[] peerPublicKey)
    {
        var (pub, priv) = _crypto.GenerateKeyPair();
        var key = _crypto.DeriveSharedSecret(priv, peerPublicKey);
        await Task.CompletedTask;
        return (key, pub);
    }
}
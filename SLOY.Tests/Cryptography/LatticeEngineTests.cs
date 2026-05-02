using SLOY.Cryptography.PostQuantum;

namespace SLOY.Tests.Cryptography;

public class LatticeEngineTests
{
    [Fact]
    public void GenerateKeyPair_ReturnsValidKeys()
    {
        var engine = new LatticeEngine(dimension: 64, modulus: 12289);
        var (publicKey, privateKey) = engine.GenerateKeyPair();

        Assert.NotNull(publicKey);
        Assert.NotNull(privateKey);
        Assert.True(publicKey.Length > 0);
        Assert.True(privateKey.Length > 0);
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip()
    {
        var engine = new LatticeEngine(dimension: 64, modulus: 12289);
        var (publicKey, privateKey) = engine.GenerateKeyPair();

        var message = new byte[256];
        new Random(42).NextBytes(message);

        var encrypted = engine.Encrypt(message, publicKey);
        var decrypted = engine.Decrypt(encrypted, privateKey);

        Assert.Equal(message.Length, encrypted.Length);
        Assert.Equal(message.Length, decrypted.Length);
    }

    [Fact]
    public void DifferentKeys_ProduceDifferentCiphertext()
    {
        var engine = new LatticeEngine(dimension: 64, modulus: 12289);
        var (pk1, _) = engine.GenerateKeyPair();
        var (pk2, _) = engine.GenerateKeyPair();

        var message = new byte[256];
        new Random(42).NextBytes(message);

        var enc1 = engine.Encrypt(message, pk1);
        var enc2 = engine.Encrypt(message, pk2);

        Assert.NotEqual(enc1, enc2);
    }
}
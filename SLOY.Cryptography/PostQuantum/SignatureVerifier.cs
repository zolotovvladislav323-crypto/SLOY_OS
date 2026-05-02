using System.Security.Cryptography;

namespace SLOY.Cryptography.PostQuantum;

public class SignatureVerifier
{
    public byte[] Sign(byte[] message, byte[] privateKey)
    {
        using var hmac = new HMACSHA512(privateKey);
        return hmac.ComputeHash(message);
    }

    public bool Verify(byte[] message, byte[] signature, byte[] publicKey)
    {
        using var hmac = new HMACSHA512(publicKey);
        var expected = hmac.ComputeHash(message);
        return CryptographicOperations.FixedTimeEquals(expected, signature);
    }
}
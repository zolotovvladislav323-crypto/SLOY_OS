using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SLOY.Orchestrator.Update;

public class UpdateVerifier
{
    private readonly byte[] _publicKey;

    public UpdateVerifier(byte[] publicKey)
    {
        _publicKey = publicKey;
    }

    public bool Verify(UpdatePackage package)
    {
        if (!VerifyHash(package.Data, package.Manifest.Hash))
            return false;

        if (!VerifySignature(package.Data, package.Manifest.Signature))
            return false;

        return true;
    }

    private bool VerifyHash(byte[] data, string expectedHash)
    {
        var actualHash = Convert.ToHexString(SHA256.HashData(data));
        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    private bool VerifySignature(byte[] data, string signatureBase64)
    {
        try
        {
            var signature = Convert.FromBase64String(signatureBase64);

            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(_publicKey, out _);

            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pss);
        }
        catch
        {
            return false;
        }
    }
}
namespace SLOY.Core.Interfaces;

public interface ICryptoProvider
{
    byte[] GenerateRandomBytes(int length);
    byte[] Hash(byte[] data);
    byte[] Sign(byte[] data, byte[] privateKey);
    bool Verify(byte[] data, byte[] signature, byte[] publicKey);
    (byte[] publicKey, byte[] privateKey) GenerateKeyPair();
    byte[] DeriveSharedSecret(byte[] privateKey, byte[] peerPublicKey);
    byte[] Encrypt(byte[] plaintext, byte[] key);
    byte[] Decrypt(byte[] ciphertext, byte[] key);
}
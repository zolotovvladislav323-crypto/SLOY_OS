using System.Security.Cryptography;

namespace SLOY.Cryptography.KeyManagement;

public class DiffieHellmanRatchet
{
    private byte[] _rootKey;
    private byte[] _sendingChainKey;
    private byte[] _receivingChainKey;
    private uint _sendingIndex;
    private uint _receivingIndex;

    public DiffieHellmanRatchet(byte[] sharedSecret)
    {
        _rootKey = DeriveKey(sharedSecret, Array.Empty<byte>(), "SLOY_ROOT_KEY", 32);
        _sendingChainKey = DeriveKey(_rootKey, Array.Empty<byte>(), "SLOY_SEND_KEY", 32);
        _receivingChainKey = DeriveKey(_rootKey, Array.Empty<byte>(), "SLOY_RECV_KEY", 32);
    }

    public (byte[] messageKey, uint index) RatchetSending()
    {
        _sendingIndex++;
        var messageKey = HMACSHA256.HashData(_sendingChainKey, BitConverter.GetBytes(_sendingIndex));
        _sendingChainKey = HMACSHA256.HashData(_sendingChainKey, new byte[] { 0x01 });
        return (messageKey, _sendingIndex);
    }

    public byte[] RatchetReceiving(uint index)
    {
        _receivingIndex = index;
        var messageKey = HMACSHA256.HashData(_receivingChainKey, BitConverter.GetBytes(_receivingIndex));
        _receivingChainKey = HMACSHA256.HashData(_receivingChainKey, new byte[] { 0x02 });
        return messageKey;
    }

    public void AdvanceRatchet(byte[] newSharedSecret)
    {
        _rootKey = DeriveKey(_rootKey, newSharedSecret, "SLOY_RATCHET", 32);
        _sendingChainKey = DeriveKey(_rootKey, Array.Empty<byte>(), "SLOY_SEND_KEY", 32);
        _receivingChainKey = DeriveKey(_rootKey, Array.Empty<byte>(), "SLOY_RECV_KEY", 32);
        _sendingIndex = 0;
        _receivingIndex = 0;
    }

    private static byte[] DeriveKey(byte[] ikm, byte[] salt, string info, int length)
    {
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, length, salt, System.Text.Encoding.UTF8.GetBytes(info));
    }
}
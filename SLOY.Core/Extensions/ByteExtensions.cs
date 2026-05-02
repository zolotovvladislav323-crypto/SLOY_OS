using System.Security.Cryptography;

namespace SLOY.Core.Extensions;

public static class ByteExtensions
{
    public static string ToHex(this byte[] data) => Convert.ToHexString(data);
    public static string ToBase64(this byte[] data) => Convert.ToBase64String(data);
    public static byte[] HashSha256(this byte[] data) => SHA256.HashData(data);
    public static bool ConstantTimeEquals(this byte[] a, byte[] b) => CryptographicOperations.FixedTimeEquals(a, b);
    public static byte[] Concat(this byte[] a, byte[] b) => a.Concat(b).ToArray();
}
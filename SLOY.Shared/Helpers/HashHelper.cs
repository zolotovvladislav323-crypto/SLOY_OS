using System.Security.Cryptography;

namespace SLOY.Shared.Helpers;

public static class HashHelper
{
    public static string Sha256(byte[] data) => Convert.ToHexString(SHA256.HashData(data));
    public static string Sha256(string text) => Sha256(System.Text.Encoding.UTF8.GetBytes(text));
    public static string ShortHash(byte[] data, int length = 8) => Sha256(data)[..length];
    public static string ShortHash(string text, int length = 8) => Sha256(text)[..length];
}
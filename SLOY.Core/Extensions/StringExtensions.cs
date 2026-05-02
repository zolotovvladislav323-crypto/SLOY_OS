using System.Security.Cryptography;
using System.Text;

namespace SLOY.Core.Extensions;

public static class StringExtensions
{
    public static byte[] ToBytes(this string s) => Encoding.UTF8.GetBytes(s);
    public static string ToBase64(this string s) => Convert.ToBase64String(s.ToBytes());
    public static string Sha256(this string s) => Convert.ToHexString(SHA256.HashData(s.ToBytes()));
    public static string Truncate(this string s, int maxLength) => s.Length <= maxLength ? s : s[..maxLength] + "...";
    public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);
}
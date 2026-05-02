using System.Security.Cryptography;

namespace SLOY.Shared.Helpers;

public static class IdGenerator
{
    public static string New(int length = 16)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToHexString(bytes)[..length];
    }

    public static string NewNumeric(int digits = 6)
    {
        return RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, digits)).ToString($"D{digits}");
    }
}
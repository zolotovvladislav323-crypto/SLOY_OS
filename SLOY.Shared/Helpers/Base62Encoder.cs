namespace SLOY.Shared.Helpers;

public static class Base62Encoder
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string Encode(byte[] data)
    {
        var value = new System.Numerics.BigInteger(data.Concat(new byte[] { 0 }).ToArray());
        var result = new System.Text.StringBuilder();
        while (value > 0)
        {
            result.Insert(0, Alphabet[(int)(value % 62)]);
            value /= 62;
        }
        return result.ToString();
    }

    public static byte[] Decode(string encoded)
    {
        var value = new System.Numerics.BigInteger();
        foreach (var c in encoded)
            value = value * 62 + Alphabet.IndexOf(c);
        return value.ToByteArray().Reverse().SkipWhile(b => b == 0).ToArray();
    }
}
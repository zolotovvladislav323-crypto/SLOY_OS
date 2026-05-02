namespace SLOY.Core.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixMs(this DateTime dt) => new DateTimeOffset(dt).ToUnixTimeMilliseconds();
    public static long ToUnixMs(this DateTimeOffset dto) => dto.ToUnixTimeMilliseconds();
    public static DateTime FromUnixMs(this long ms) => DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
    public static string ToIso8601(this DateTime dt) => dt.ToString("O");
    public static bool IsExpired(this DateTime dt, TimeSpan ttl) => DateTime.UtcNow - dt > ttl;
}
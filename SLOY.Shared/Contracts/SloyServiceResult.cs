namespace SLOY.Shared.Contracts;

public class SloyServiceResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public long ElapsedMs { get; init; }

    public static SloyServiceResult Ok(long elapsedMs = 0) => new() { Success = true, ElapsedMs = elapsedMs };
    public static SloyServiceResult Fail(string error) => new() { Success = false, Error = error };
}
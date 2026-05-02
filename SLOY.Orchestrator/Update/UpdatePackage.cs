namespace SLOY.Orchestrator.Update;

public class UpdatePackage
{
    public UpdateManifest Manifest { get; init; } = new();
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public string TempPath { get; set; } = string.Empty;
    public bool IsDelta { get; init; }
}
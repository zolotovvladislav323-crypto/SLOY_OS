namespace SLOY.Orchestrator.Update;

public class UpdateManifest
{
    public string Version { get; set; } = "0.0.0";
    public string Channel { get; set; } = "stable";
    public long SizeBytes { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
    public bool IsCritical { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string? DeltaUrl { get; set; }
    public long DeltaSizeBytes { get; set; }
    public string? MinVersion { get; set; }
    public List<string> AffectedFiles { get; set; } = new();
}
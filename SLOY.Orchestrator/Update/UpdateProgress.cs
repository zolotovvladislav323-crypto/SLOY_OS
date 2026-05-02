namespace SLOY.Orchestrator.Update;

public class UpdateProgress
{
    public UpdateStage Stage { get; set; } = UpdateStage.Idle;
    public double PercentComplete { get; set; }
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public string? StatusMessage { get; set; }
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum UpdateStage
{
    Idle,
    Checking,
    Downloading,
    Verifying,
    Applying,
    RollingBack,
    Complete,
    Failed
}
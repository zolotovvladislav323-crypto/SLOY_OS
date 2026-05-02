namespace SLOY.Shared.Models;

public class PrivacySettings
{
    public bool HideNodeId { get; set; }
    public bool EncryptMetadata { get; set; } = true;
    public bool RouteThroughMixNodes { get; set; }
    public bool EnableDecoyTraffic { get; set; }
    public int AutoShredMinutes { get; set; } = 30;
    public bool BiometricLockEnabled { get; set; }
}
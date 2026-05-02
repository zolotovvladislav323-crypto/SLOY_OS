namespace SLOY.Core.Configuration;

public static class DefaultConfig
{
    public static SloyConfig Create()
    {
        return new SloyConfig
        {
            Mesh = { PeerDiscoveryIntervalMs = 5000, PeerTimeoutMinutes = 5, MaxPeers = 50, MaxHopCount = 7 },
            Security = { PostQuantumEnabled = true, DoubleRatchetEnabled = true, KeyRotationMinutes = 60, MaxFailedBiometricAttempts = 5, AutoShredMinutes = 30 },
            Network = { MTU = 1024, MaxRetries = 5, AckTimeoutMs = 3000, DecoyTrafficEnabled = false, DecoyPacketsPerMinute = 10, CamouflageProfile = "HTTP" },
            Storage = { DatabasePath = "sloy_data", MaxStorageMb = 500, AutoShredEnabled = true, ShredIntervalHours = 24, DeadManSwitchMinutes = 30 },
            UI = { Theme = "Dark", AnimationsEnabled = true, HelloPageDurationMs = 3000, ShowDebugInfo = false }
        };
    }
}
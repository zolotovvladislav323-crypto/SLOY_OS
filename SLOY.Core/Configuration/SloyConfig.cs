namespace SLOY.Core.Configuration;

public class SloyConfig
{
    public MeshConfig Mesh { get; set; } = new();
    public SecurityConfig Security { get; set; } = new();
    public NetworkConfig Network { get; set; } = new();
    public StorageConfig Storage { get; set; } = new();
    public UIConfig UI { get; set; } = new();
}

public class MeshConfig
{
    public int PeerDiscoveryIntervalMs { get; set; } = 5000;
    public int PeerTimeoutMinutes { get; set; } = 5;
    public int MaxPeers { get; set; } = 50;
    public int MaxHopCount { get; set; } = 7;
}

public class SecurityConfig
{
    public bool PostQuantumEnabled { get; set; } = true;
    public bool DoubleRatchetEnabled { get; set; } = true;
    public int KeyRotationMinutes { get; set; } = 60;
    public int MaxFailedBiometricAttempts { get; set; } = 5;
    public int AutoShredMinutes { get; set; } = 30;
}

public class NetworkConfig
{
    public int MTU { get; set; } = 1024;
    public int MaxRetries { get; set; } = 5;
    public int AckTimeoutMs { get; set; } = 3000;
    public bool DecoyTrafficEnabled { get; set; } = false;
    public int DecoyPacketsPerMinute { get; set; } = 10;
    public string CamouflageProfile { get; set; } = "HTTP";
}

public class StorageConfig
{
    public string DatabasePath { get; set; } = "sloy_data";
    public long MaxStorageMb { get; set; } = 500;
    public bool AutoShredEnabled { get; set; } = true;
    public int ShredIntervalHours { get; set; } = 24;
    public int DeadManSwitchMinutes { get; set; } = 30;
}

public class UIConfig
{
    public string Theme { get; set; } = "Dark";
    public bool AnimationsEnabled { get; set; } = true;
    public int HelloPageDurationMs { get; set; } = 3000;
    public bool ShowDebugInfo { get; set; } = false;
}
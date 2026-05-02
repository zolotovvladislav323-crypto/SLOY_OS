using System.ComponentModel;
using System.Runtime.CompilerServices;
using SLOY.Shared.Models;

namespace SLOY.UI.Common.ViewModels;

public class PrivacySettingsViewModel : INotifyPropertyChanged
{
    private readonly PrivacySettings _settings = new();

    public bool HideNodeId
    {
        get => _settings.HideNodeId;
        set { _settings.HideNodeId = value; OnPropertyChanged(); }
    }

    public bool EncryptMetadata
    {
        get => _settings.EncryptMetadata;
        set { _settings.EncryptMetadata = value; OnPropertyChanged(); }
    }

    public bool RouteThroughMixNodes
    {
        get => _settings.RouteThroughMixNodes;
        set { _settings.RouteThroughMixNodes = value; OnPropertyChanged(); }
    }

    public bool EnableDecoyTraffic
    {
        get => _settings.EnableDecoyTraffic;
        set { _settings.EnableDecoyTraffic = value; OnPropertyChanged(); }
    }

    public int AutoShredMinutes
    {
        get => _settings.AutoShredMinutes;
        set { _settings.AutoShredMinutes = value; OnPropertyChanged(); }
    }

    public bool BiometricLockEnabled
    {
        get => _settings.BiometricLockEnabled;
        set { _settings.BiometricLockEnabled = value; OnPropertyChanged(); }
    }

    public PrivacySettings GetSettings() => _settings;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
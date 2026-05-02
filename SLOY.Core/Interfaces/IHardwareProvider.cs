namespace SLOY.Core.Interfaces;

public interface IHardwareProvider
{
    bool IsBluetoothAvailable { get; }
    bool IsWiFiDirectAvailable { get; }
    bool IsCameraAvailable { get; }
    bool IsFlashlightAvailable { get; }
    bool IsSpeakerAvailable { get; }
    bool IsMicrophoneAvailable { get; }
    bool IsMagnetometerAvailable { get; }
    bool IsAccelerometerAvailable { get; }

    Task<bool> RequestBluetoothPermissionAsync();
    Task<bool> RequestCameraPermissionAsync();
    Task<bool> RequestMicrophonePermissionAsync();
    Task<bool> RequestLocationPermissionAsync();
}
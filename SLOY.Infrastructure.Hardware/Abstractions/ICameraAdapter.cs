namespace SLOY.Infrastructure.Hardware.Abstractions;

public interface ICameraAdapter
{
    bool IsAvailable { get; }
    bool IsFlashlightAvailable { get; }
    Task<bool> RequestPermissionAsync();
    Task<bool> StartCaptureAsync(int fps = 30);
    Task StopCaptureAsync();
    Task<bool> ToggleFlashlightAsync(bool enable);
    Task<byte[]?> CaptureFrameAsync();
    event EventHandler<byte[]> OnFrameCaptured;
}
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.Windows.Services;

public class WindowsCameraAdapter : ICameraAdapter
{
    public bool IsAvailable => true;
    public bool IsFlashlightAvailable => false;

    public event EventHandler<byte[]>? OnFrameCaptured;

    public Task<bool> RequestPermissionAsync() => Task.FromResult(true);

    public Task<bool> StartCaptureAsync(int fps = 30) => Task.FromResult(true);

    public Task StopCaptureAsync() => Task.CompletedTask;

    public Task<bool> ToggleFlashlightAsync(bool enable) => Task.FromResult(false);

    public Task<byte[]?> CaptureFrameAsync() => Task.FromResult<byte[]?>(null);
}
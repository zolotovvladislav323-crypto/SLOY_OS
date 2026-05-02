using AVFoundation;
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.iOS.Services;

public class iOSCameraAdapter : ICameraAdapter
{
    private AVCaptureSession? _session;
    private AVCaptureDeviceInput? _input;

    public bool IsAvailable => AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video) != null;
    public bool IsFlashlightAvailable => AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video)?.HasTorch ?? false;

    public event EventHandler<byte[]>? OnFrameCaptured;

    public async Task<bool> RequestPermissionAsync()
    {
        var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaTypes.Video);
        if (status == AVAuthorizationStatus.NotDetermined)
        {
            var granted = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaTypes.Video);
            return granted;
        }
        return status == AVAuthorizationStatus.Authorized;
    }

    public async Task<bool> StartCaptureAsync(int fps = 30)
    {
        await Task.CompletedTask;
        return IsAvailable;
    }

    public Task StopCaptureAsync()
    {
        _session?.StopRunning();
        return Task.CompletedTask;
    }

    public async Task<bool> ToggleFlashlightAsync(bool enable)
    {
        var device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
        if (device?.HasTorch != true) return false;

        device.LockForConfiguration(out _);
        device.TorchMode = enable ? AVCaptureTorchMode.On : AVCaptureTorchMode.Off;
        device.UnlockForConfiguration();

        await Task.CompletedTask;
        return true;
    }

    public Task<byte[]?> CaptureFrameAsync()
    {
        return Task.FromResult<byte[]?>(null);
    }
}
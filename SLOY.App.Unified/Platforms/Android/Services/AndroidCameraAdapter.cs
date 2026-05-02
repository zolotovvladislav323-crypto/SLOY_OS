using Android.Hardware.Camera2;
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.App.Unified.Platforms.Android.Services;

public class AndroidCameraAdapter : ICameraAdapter
{
    private CameraDevice? _camera;
    private CameraCaptureSession? _session;
    private ImageReader? _imageReader;
    private bool _isFlashlightOn;

    public bool IsAvailable => true;
    public bool IsFlashlightAvailable => true;

    public event EventHandler<byte[]>? OnFrameCaptured;

    public async Task<bool> RequestPermissionAsync()
    {
        await Task.CompletedTask;
        return true;
    }

    public Task<bool> StartCaptureAsync(int fps = 30)
    {
        _imageReader = ImageReader.NewInstance(640, 480, (int)Android.Graphics.ImageFormatType.Yuv420888, 30);
        _imageReader.SetOnImageAvailableListener(new ImageListener(frame =>
        {
            OnFrameCaptured?.Invoke(this, frame);
        }), null);

        return Task.FromResult(true);
    }

    public Task StopCaptureAsync()
    {
        _imageReader?.Close();
        _imageReader = null;
        return Task.CompletedTask;
    }

    public Task<bool> ToggleFlashlightAsync(bool enable)
    {
        _isFlashlightOn = enable;
        return Task.FromResult(true);
    }

    public Task<byte[]?> CaptureFrameAsync()
    {
        return Task.FromResult<byte[]?>(null);
    }

    private class ImageListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        private readonly Action<byte[]> _onFrame;

        public ImageListener(Action<byte[]> onFrame)
        {
            _onFrame = onFrame;
        }

        public void OnImageAvailable(ImageReader? reader)
        {
            var image = reader?.AcquireLatestImage();
            if (image == null) return;

            var buffer = image.GetPlanes()[0].Buffer;
            var data = new byte[buffer.Remaining()];
            buffer.Get(data);

            _onFrame(data);
            image.Close();
        }
    }
}
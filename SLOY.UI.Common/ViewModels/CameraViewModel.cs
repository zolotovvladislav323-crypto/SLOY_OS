using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.UI.Common.ViewModels;

public class CameraViewModel : INotifyPropertyChanged
{
    private readonly ICameraAdapter _camera;
    private bool _isScanning;
    private string _lastResult = string.Empty;
    private bool _isFlashlightOn;

    public bool IsScanning
    {
        get => _isScanning;
        set { _isScanning = value; OnPropertyChanged(); }
    }

    public string LastResult
    {
        get => _lastResult;
        set { _lastResult = value; OnPropertyChanged(); }
    }

    public bool IsFlashlightOn
    {
        get => _isFlashlightOn;
        set { _isFlashlightOn = value; OnPropertyChanged(); }
    }

    public ICommand StartScanCommand { get; }
    public ICommand StopScanCommand { get; }
    public ICommand ToggleFlashlightCommand { get; }

    public CameraViewModel(ICameraAdapter camera)
    {
        _camera = camera;

        StartScanCommand = new Command(async () => await StartScan());
        StopScanCommand = new Command(async () => await StopScan());
        ToggleFlashlightCommand = new Command(async () => await ToggleFlashlight());
    }

    private async Task StartScan()
    {
        if (IsScanning) return;
        IsScanning = true;
        LastResult = "Сканирование...";
        await _camera.StartCaptureAsync(30);
    }

    private async Task StopScan()
    {
        if (!IsScanning) return;
        IsScanning = false;
        await _camera.StopCaptureAsync();
        LastResult = "Остановлено";
    }

    private async Task ToggleFlashlight()
    {
        IsFlashlightOn = !IsFlashlightOn;
        await _camera.ToggleFlashlightAsync(IsFlashlightOn);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
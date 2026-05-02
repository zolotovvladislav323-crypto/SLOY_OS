using SLOY.Infrastructure.Hardware.Optical.QRCode;

namespace SLOY.App.Unified.Views.Shared;

public partial class ConnectViaQRPage : ContentPage
{
    private readonly QRCodeGenerator _qrGenerator = new(moduleSize: 6, borderModules: 2);
    private byte[]? _currentQR;

    public ConnectViaQRPage()
    {
        InitializeComponent();
        GenerateQR();
    }

    private void GenerateQR()
    {
        var identity = App.CurrentIdentity;
        if (identity == null) return;

        var payload = $"SLOY://connect?node={identity.NodeId}&nick={identity.Nickname}&key={identity.Checksum}";
        _currentQR = _qrGenerator.GenerateBitmap(payload);

        QRImage.Source = ImageSource.FromStream(() => new MemoryStream(_currentQR));
    }

    private void OnRegenerateClicked(object? sender, EventArgs e)
    {
        GenerateQR();
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Сканирование", "Наведите камеру на QR-код пира", "OK");
    }
}
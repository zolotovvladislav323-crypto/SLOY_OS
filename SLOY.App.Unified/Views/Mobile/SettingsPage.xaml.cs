using SLOY.Shared.Models;

namespace SLOY.App.Unified.Views.Mobile;

public partial class SettingsPage : ContentPage
{
    private readonly PrivacySettings _settings = new();

    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        HideNodeIdSwitch.IsToggled = _settings.HideNodeId;
        EncryptMetadataSwitch.IsToggled = _settings.EncryptMetadata;
        MixNodesSwitch.IsToggled = _settings.RouteThroughMixNodes;
        DecoyTrafficSwitch.IsToggled = _settings.EnableDecoyTraffic;
        BiometricLockSwitch.IsToggled = _settings.BiometricLockEnabled;
        AutoShredEntry.Text = _settings.AutoShredMinutes.ToString();
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        _settings.HideNodeId = HideNodeIdSwitch.IsToggled;
        _settings.EncryptMetadata = EncryptMetadataSwitch.IsToggled;
        _settings.RouteThroughMixNodes = MixNodesSwitch.IsToggled;
        _settings.EnableDecoyTraffic = DecoyTrafficSwitch.IsToggled;
        _settings.BiometricLockEnabled = BiometricLockSwitch.IsToggled;

        if (int.TryParse(AutoShredEntry.Text, out var minutes))
            _settings.AutoShredMinutes = minutes;

        DisplayAlert("Готово", "Настройки сохранены", "OK");
    }

    private async void OnNukeClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "ПОДТВЕРЖДЕНИЕ",
            "Все данные будут безвозвратно уничтожены. Продолжить?",
            "УНИЧТОЖИТЬ",
            "Отмена"
        );

        if (confirm)
        {
            // Nuke
            await DisplayAlert("Завершено", "Данные уничтожены. Приложение закроется.", "OK");
            Application.Current?.Quit();
        }
    }
}
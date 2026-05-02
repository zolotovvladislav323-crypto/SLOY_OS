using SLOY.Shared.Models;

namespace SLOY.App.Unified.Views.Shared;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage()
    {
        InitializeComponent();
        NicknameEntry.TextChanged += OnNicknameChanged;
    }

    private void OnNicknameChanged(object? sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue?.Trim() ?? "";
        var isValid = text.Length >= 2
                      && text.Length <= 30
                      && System.Text.RegularExpressions.Regex.IsMatch(text, @"^[\w\-\.]+$");

        ContinueButton.IsEnabled = isValid;
        ErrorLabel.IsVisible = text.Length > 0 && !isValid;
        ErrorLabel.Text = text.Length switch
        {
            < 2 => "Слишком коротко (минимум 2 символа)",
            _ => "Только буквы, цифры, _, -, ."
        };
    }

    private async void OnContinueClicked(object? sender, EventArgs e)
    {
        var nickname = NicknameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(nickname)) return;

        try
        {
            var identity = new Identity(nickname);

            // Сохраняем Identity в статический контекст приложения
            App.CurrentIdentity = identity;

            ContinueButton.IsEnabled = false;
            ContinueButton.Text = "✓";
            await Task.Delay(300);

            await Shell.Current.GoToAsync("//ChatPage");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
    }
}
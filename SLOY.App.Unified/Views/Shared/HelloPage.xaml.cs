using SLOY.Shared.Constants;

namespace SLOY.App.Unified.Views.Shared;

public partial class HelloPage : ContentPage
{
    public HelloPage()
    {
        InitializeComponent();
        StartAnimation();
    }

    private async void StartAnimation()
    {
        // Устанавливаем случайное приветствие
        GreetingLabel.Text = Defaults.GetRandomGreeting();

        // Анимация появления логотипа
        await Task.Delay(300);
        await LogoLabel.FadeTo(1, 800, Easing.CubicIn);
        await TitleLabel.FadeTo(1, 600, Easing.CubicIn);

        // Анимация приветствия
        await Task.Delay(200);
        await GreetingLabel.FadeTo(1, 800, Easing.CubicIn);

        // Имитация загрузки
        StatusLabel.Text = "Построение маршрутов...";
        await Task.Delay(700);
        StatusLabel.Text = "Синхронизация ключей...";
        await Task.Delay(700);
        StatusLabel.Text = "Маскировка трафика...";
        await Task.Delay(700);
        StatusLabel.Text = "Готово.";

        LoaderIndicator.IsRunning = false;
        await Task.Delay(400);

        // Переход на OnboardingPage
        await Shell.Current.GoToAsync("//OnboardingPage");
    }
}
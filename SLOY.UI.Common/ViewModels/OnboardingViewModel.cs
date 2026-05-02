using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using SLOY.Shared.Models;

namespace SLOY.UI.Common.ViewModels;

public class OnboardingViewModel : INotifyPropertyChanged
{
    private string _nickname = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isValid;
    private bool _hasError;

    public string Nickname
    {
        get => _nickname;
        set { _nickname = value; OnPropertyChanged(); Validate(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public bool IsValid
    {
        get => _isValid;
        set { _isValid = value; OnPropertyChanged(); }
    }

    public bool HasError
    {
        get => _hasError;
        set { _hasError = value; OnPropertyChanged(); }
    }

    public ICommand ContinueCommand { get; }

    public OnboardingViewModel()
    {
        ContinueCommand = new Command(async () => await OnContinue());
    }

    private void Validate()
    {
        var text = Nickname?.Trim() ?? "";
        if (text.Length < 2) { IsValid = false; HasError = text.Length > 0; ErrorMessage = "Слишком коротко"; return; }
        if (!Regex.IsMatch(text, @"^[\w\-\.]+$")) { IsValid = false; HasError = true; ErrorMessage = "Недопустимые символы"; return; }
        IsValid = true; HasError = false; ErrorMessage = string.Empty;
    }

    private async Task OnContinue()
    {
        if (!IsValid) return;
        var identity = new Identity(Nickname.Trim());
        await Shell.Current.GoToAsync("//ChatPage");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? p = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
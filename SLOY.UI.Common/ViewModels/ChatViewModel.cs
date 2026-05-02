using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SLOY.Shared.Models;

namespace SLOY.UI.Common.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    private string _messageText = string.Empty;
    private readonly Identity _identity;

    public ObservableCollection<Message> Messages { get; } = new();
    public ObservableCollection<Identity> Peers { get; } = new();

    public string MessageText
    {
        get => _messageText;
        set { _messageText = value; OnPropertyChanged(); }
    }

    public string Title => $"SLOY OS — {_identity.Nickname}";
    public string NodeId => _identity.NodeId;

    public ICommand SendCommand { get; }

    public ChatViewModel(Identity identity)
    {
        _identity = identity;

        SendCommand = new Command(SendMessage);

        Messages.Add(new Message
        {
            SenderNickname = "SLOY",
            Content = $"Добро пожаловать, {_identity.Nickname}. Сеть активирована.",
            IsMine = false
        });
    }

    private void SendMessage()
    {
        var text = MessageText?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        Messages.Add(new Message
        {
            SenderNickname = _identity.Nickname,
            SenderNodeId = _identity.NodeId,
            Content = text,
            IsMine = true
        });

        MessageText = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
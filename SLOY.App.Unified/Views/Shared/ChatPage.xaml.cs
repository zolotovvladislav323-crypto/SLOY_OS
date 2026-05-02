using System.Collections.ObjectModel;
using SLOY.Shared.Models;

namespace SLOY.App.Unified.Views.Shared;

public partial class ChatPage : ContentPage
{
    public ObservableCollection<Message> Messages { get; } = new();
    private readonly Identity _identity;

    public ChatPage()
    {
        InitializeComponent();
        BindingContext = this;

        _identity = App.CurrentIdentity!;
        TitleLabel.Text = $"SLOY OS";
        NodeIdLabel.Text = $"{_identity}";

        // Тестовое приветственное сообщение
        Messages.Add(new Message
        {
            SenderNickname = "SLOY",
            Content = $"Добро пожаловать, {_identity.Nickname}. Ты в защищённой сети.",
            IsMine = false
        });
    }

    private void OnSendClicked(object? sender, EventArgs e)
    {
        var text = MessageEntry.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        Messages.Add(new Message
        {
            SenderNickname = _identity.Nickname,
            SenderNodeId = _identity.NodeId,
            Content = text,
            IsMine = true
        });

        MessageEntry.Text = string.Empty;
    }
}
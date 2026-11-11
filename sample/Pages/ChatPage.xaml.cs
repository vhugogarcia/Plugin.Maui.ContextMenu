using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Plugin.Maui.ContextMenu.Sample.Pages;

public partial class ChatMessage : ObservableObject
{
    public ChatMessage(int id, string text, bool isUser)
    {
        Id = id;
        Text = text;
        IsUser = isUser;
    }

    public int Id { get; }
    public string Text { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBot))]
    bool isUser;

    public bool IsBot => !IsUser;
}

/// <summary>
/// Repro page for "the long-pressed view disappears after the menu closes" (modal pages)
/// and "Back with the menu open". Every menu action updates <see cref="Status"/>.
/// </summary>
public partial class ChatPage : ContentPage
{
    int _count;
    string _status = "No action yet";

    public ChatPage(string mode)
    {
        Mode = mode;
        for (var i = 1; i <= 12; i++)
        {
            var isUser = i % 2 == 1;
            Messages.Add(new ChatMessage(i, isUser ? $"User message #{i}" : $"Bot reply #{i}", isUser));
        }
        CopyCommand = new Command<ChatMessage>(m => SetStatus($"Copiar #{m?.Id}: {m?.Text}"));
        CopyPlainCommand = new Command(() => SetStatus("Copiar: plain border"));
        InitializeComponent();
        BindingContext = this;
    }

    public string Mode { get; }
    public ObservableCollection<ChatMessage> Messages { get; } = new();
    public ICommand CopyCommand { get; }
    public ICommand CopyPlainCommand { get; }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    void SetStatus(string text) => Status = $"[{++_count}] {text}";

    async void OnAutoCloseClicked(object sender, EventArgs e)
    {
        SetStatus("Closing in 3s - long-press something now");
        await Task.Delay(3000);
        OnCloseClicked(sender, e);
    }

    async void OnCloseClicked(object sender, EventArgs e)
    {
        if (Navigation.ModalStack.Count > 0 && (Navigation.ModalStack[^1] == this || Navigation.ModalStack[^1] == Parent))
        {
            await Navigation.PopModalAsync();
        }
        else
        {
            await Navigation.PopAsync();
        }
    }
}

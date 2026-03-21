namespace Plugin.Maui.ContextMenu.Sample.Pages;

public record DashboardItem(string Name, Color Color);

public partial class CrossPageLeakPage : ContentPage
{
    public CrossPageLeakPage()
    {
        InitializeComponent();
        BindingContext = new List<DashboardItem>
        {
            new("Event Alpha", Colors.SteelBlue),
            new("Event Beta", Colors.Teal),
            new("Event Gamma", Colors.SlateBlue),
        };
    }

    async void OnGoToVisitors(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VisitorsPage());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StatusLabel.Text = $"Page appeared at {DateTime.Now:HH:mm:ss}. Long-press an item — you should see 'Ver Invitados' and 'Remover'.";
    }
}

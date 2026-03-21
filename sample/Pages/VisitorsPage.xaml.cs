namespace Plugin.Maui.ContextMenu.Sample.Pages;

public record VisitorItem(string Name, Color Color);

public partial class VisitorsPage : ContentPage
{
    public VisitorsPage()
    {
        InitializeComponent();
        BindingContext = new List<VisitorItem>
        {
            new("Carlos Garcia", Colors.Coral),
            new("Maria Lopez", Colors.MediumOrchid),
            new("Juan Perez", Colors.DarkSeaGreen),
            new("Ana Martinez", Colors.IndianRed),
        };
    }
}

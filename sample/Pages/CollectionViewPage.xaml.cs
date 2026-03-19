using Plugin.Maui.ContextMenu.Sample.ViewModels;

namespace Plugin.Maui.ContextMenu.Sample.Pages;

public record struct CollectionItem(string Text, Color Color);

public partial class CollectionViewPage : ContentPage
{
    public CollectionViewPage()
	{
		BindingContext = new CollectionViewPageViewModel();
		InitializeComponent();
        SizeChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(CellHeight));
        };
	}

    public double CellHeight => Width / 3;
}

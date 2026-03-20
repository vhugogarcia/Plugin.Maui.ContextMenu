namespace Plugin.Maui.ContextMenu.Sample.Pages;

public partial class CollectionViewDetailPage : ContentPage
{
    public CollectionViewDetailPage(string itemText)
    {
        InitializeComponent();
        DetailLabel.Text = $"Navigated from the collectionview item {itemText}";
    }

    async void OnGoBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

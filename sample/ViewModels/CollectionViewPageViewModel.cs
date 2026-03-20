using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.ContextMenu.Sample.Pages;

namespace Plugin.Maui.ContextMenu.Sample.ViewModels;

public partial class CollectionViewPageViewModel : ObservableObject
{
    [ObservableProperty]
    bool isContextMenuVisible = true;
    public List<CollectionItem> Items { get; } = new List<CollectionItem>
    {
        new CollectionItem("green", Colors.GreenYellow),
        new CollectionItem("title", Colors.DeepPink),
        new CollectionItem("smiling", Colors.LightSkyBlue),
        new CollectionItem("sneeze", Colors.Salmon),
        new CollectionItem("big", Colors.MediumAquamarine),
        new CollectionItem("guitar", Colors.Khaki),
        new CollectionItem("nest", Colors.LightCoral),
        new CollectionItem("name", Colors.PaleGreen),
        new CollectionItem("toad", Colors.SandyBrown),
        new CollectionItem("absent", Colors.Plum),
        new CollectionItem("enchanting", Colors.LightSteelBlue),
        new CollectionItem("irritating", Colors.PeachPuff),
        new CollectionItem("clap", Colors.MediumOrchid),
        new CollectionItem("pretty", Colors.LightGreen),
        new CollectionItem("sable", Colors.Thistle),
        new CollectionItem("cast", Colors.Tan),
        new CollectionItem("wealth", Colors.CornflowerBlue),
        new CollectionItem("weather", Colors.LightPink),
        new CollectionItem("upbeat", Colors.Aquamarine),
        new CollectionItem("market", Colors.Gold),
        new CollectionItem("abiding", Colors.MediumPurple),
        new CollectionItem("boy", Colors.LightSalmon),
        new CollectionItem("umbrella", Colors.PaleTurquoise),
        new CollectionItem("tramp", Colors.YellowGreen),
    };

    [RelayCommand]
    void ToggleContextMenu() => IsContextMenuVisible = !IsContextMenuVisible;

    [RelayCommand]
    async Task Test(CollectionItem item)
    {
        await Application.Current!.Windows[0].Page!.DisplayAlert("Test", item.Text, "ok");
    }

    [RelayCommand]
    async Task Notify(CollectionItem item)
    {
        var page = Application.Current!.Windows[0].Page!;
        var navigation = page is NavigationPage navPage
            ? navPage.Navigation
            : page.Navigation;
        await navigation.PushAsync(new CollectionViewDetailPage(item.Text));
    }
}

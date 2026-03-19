using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.ContextMenu.Sample.Pages;

namespace Plugin.Maui.ContextMenu.Sample.ViewModels;

public partial class CollectionViewPageViewModel : ObservableObject
{
    public List<CollectionItem> Items { get; } = new List<CollectionItem>
    {
        new CollectionItem("green", Colors.GreenYellow),
        new CollectionItem("title", Colors.DeepPink),
        new CollectionItem("smiling", Colors.LightGoldenrodYellow),
        new CollectionItem("sneeze", Colors.LightGoldenrodYellow),
        new CollectionItem("big", Colors.LightGoldenrodYellow),
        new CollectionItem("guitar", Colors.LightGoldenrodYellow),
        new CollectionItem("nest", Colors.LightGoldenrodYellow),
        new CollectionItem("name", Colors.LightGoldenrodYellow),
        new CollectionItem("toad", Colors.LightGoldenrodYellow),
        new CollectionItem("absent", Colors.LightGoldenrodYellow),
        new CollectionItem("enchanting", Colors.LightGoldenrodYellow),
        new CollectionItem("irritating", Colors.LightGoldenrodYellow),
        new CollectionItem("clap", Colors.LightGoldenrodYellow),
        new CollectionItem("pretty", Colors.LightGoldenrodYellow),
        new CollectionItem("sable", Colors.LightGoldenrodYellow),
        new CollectionItem("cast", Colors.LightGoldenrodYellow),
        new CollectionItem("wealth", Colors.LightGoldenrodYellow),
        new CollectionItem("weather", Colors.LightGoldenrodYellow),
        new CollectionItem("upbeat", Colors.LightGoldenrodYellow),
        new CollectionItem("market", Colors.LightGoldenrodYellow),
        new CollectionItem("abiding", Colors.LightGoldenrodYellow),
        new CollectionItem("boy", Colors.LightGoldenrodYellow),
        new CollectionItem("umbrella", Colors.LightGoldenrodYellow),
        new CollectionItem("tramp", Colors.LightGoldenrodYellow),
    };

    [RelayCommand]
    async Task Test(CollectionItem item)
    {
        await Application.Current!.Windows[0].Page!.DisplayAlert("Test", item.Text, "ok");
    }

    [RelayCommand]
    async Task Notify(CollectionItem item)
    {
        await Application.Current!.Windows[0].Page!.DisplayAlert("Notice", item.Text, "ok");
    }
}

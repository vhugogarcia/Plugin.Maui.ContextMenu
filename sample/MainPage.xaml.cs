using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.ContextMenu.Sample.Pages;

namespace Plugin.Maui.ContextMenu.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	[RelayCommand]
	void GoToPage(Type page)
	{
		Navigation.PushAsync((Page)Activator.CreateInstance(page));
	}

	[RelayCommand]
	void OpenChat(string mode)
	{
		switch (mode)
		{
			case "push":
				Navigation.PushAsync(new ChatPage("Non-modal (PushAsync)"));
				break;
			case "modal":
				Navigation.PushModalAsync(new ChatPage("Modal (PushModalAsync)"));
				break;
			case "modalnav":
				Navigation.PushModalAsync(new NavigationPage(new ChatPage("Modal in NavigationPage")));
				break;
			case "cvmodal":
				Navigation.PushModalAsync(new NavigationPage(new CollectionViewPage()));
				break;
		}
	}
}

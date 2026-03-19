using CommunityToolkit.Mvvm.Input;

namespace Plugin.Maui.ContextMenu.Sample.Pages;

public partial class SimplePage : ContentPage
{
	public SimplePage()
	{
		InitializeComponent();
	}

	[RelayCommand]
	void RunAction(string msg)
	{
		DisplayAlert("Action", msg, "Ok");
	}
}

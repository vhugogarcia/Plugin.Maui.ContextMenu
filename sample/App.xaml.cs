namespace Plugin.Maui.ContextMenu.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new NavigationPage(new MainPage());
	}

#if IOS
	protected override void OnStart()
	{
		base.OnStart();
		if (SelfTest.Enabled)
		{
			_ = SelfTest.RunAsync(((NavigationPage)MainPage).CurrentPage);
		}
	}
#endif
}

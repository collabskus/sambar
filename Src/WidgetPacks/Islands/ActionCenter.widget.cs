public class ActionCenter : Widget
{
	public ActionCenter(WidgetEnv ENV) : base(ENV)
	{
		StackPanel panel = new();
		panel.Orientation = Orientation.Horizontal;

		//ENV.ASSETS_FOLDER = ENV.ASSETS_FOLDER.Replace("Islands", "Base");
		ENV.IS_IMPORTED = true;
		Sambar.api.Print($"imported: {ENV.IS_IMPORTED}, {ENV.IMPORTS_ASSETS_FOLDER}");

		NetworkManager wifi = new(ENV);
		Tray tray = new(ENV);
		Wallpapers wall = new(ENV);

		wifi.Init();
		tray.Init();
		wall.Init();

		panel.Children.Add(wifi);
		panel.Children.Add(tray);
		panel.Children.Add(wall);

		this.CornerRadius = new(8);
		this.Background = Utils.BrushFromHex("#1a1a1a");
		this.Content = panel;
	}
}

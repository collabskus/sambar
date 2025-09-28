public class NetworkManager : Widget
{
	public RoundedButton btn = new();
	public string iconFile = "wifi.svg";

	public NetworkManager(WidgetEnv ENV) : base(ENV) { }
	public override void Init()
	{
		if (File.Exists(Path.Join(ENV.ASSETS_FOLDER, iconFile)))
			btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, iconFile);
		else
			btn.ImageSrc = Path.Join(ENV.IMPORTS_ASSETS_FOLDER, iconFile);
		btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
		btn.Margin = Theme.BUTTON_MARGIN;
		btn.Height = Theme.BUTTON_HEIGHT;
		btn.Width = Theme.BUTTON_WIDTH;
		btn.IconWidth = 16;
		btn.IconHeight = 16;
		btn.FontFamily = Theme.FONT_FAMILY;
		btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
		btn.Background = Theme.BUTTON_BACKGROUND;
		btn.HoverEffect = false;
		btn.MouseDown += ButtonMouseDown;

		this.Background = Theme.WIDGET_BACKGROUND;
		this.Content = btn;
	}

	public void ButtonMouseDown(object? sender, MouseEventArgs e)
	{
		Task.Run(() => { Sambar.api.LaunchUri("ms-actioncenter:controlcenter/"); });
	}
}


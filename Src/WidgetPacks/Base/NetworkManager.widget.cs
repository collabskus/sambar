public class NetworkManager : Widget
{
	public NetworkManager() : base()
	{
		RoundedButton btn = new();
		//btn.Text = "W";
		btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
		btn.Margin = Theme.BUTTON_MARGIN;
		btn.Height = Theme.BUTTON_HEIGHT;
		btn.Width = Theme.BUTTON_WIDTH;
		//btn.ImageSrc = "C:\\Users\\Jayakuttan\\Downloads\\wireless-16.ico";
		//btn.ImageSrc = @"C:\Users\Jayakuttan\dev\sambar\Src\WidgetPacks\Base\assets\wifi.svg";
		btn.ImageSrc = Path.Join(Paths.assetsFolder, "wifi.svg");
		btn.IconWidth = 16;
		btn.IconHeight = 16;
		btn.FontFamily = Theme.FONT_FAMILY;
		btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
		btn.Background = Theme.BUTTON_BACKGROUND;
		btn.HoverEffect = false;
		btn.MouseDown += ButtonMouseDown;
		this.Content = btn;
		this.Background = Theme.WIDGET_BACKGROUND;

	}

	public void ButtonMouseDown(object? sender, MouseEventArgs e)
	{
		Task.Run(() => { Sambar.api.LaunchUri("ms-actioncenter:controlcenter/"); });
	}
}


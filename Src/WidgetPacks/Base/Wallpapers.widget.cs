public class Wallpapers : Widget
{
	public Wallpapers(WidgetEnv ENV) : base(ENV)
	{
		RoundedButton btn = new();
		btn.Height = Theme.BUTTON_HEIGHT;
		btn.Width = Theme.BUTTON_WIDTH;
		btn.Text = "W";
		btn.Foreground = Theme.TEXT_COLOR;
		btn.Background = Theme.BUTTON_BACKGROUND;
		btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
		btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
		btn.FontFamily = Theme.FONT_FAMILY;
		btn.HoverEffect = true;
		btn.MouseDown += ButtonMouseDown;
		this.Content = btn;
	}
	public void ButtonMouseDown(object? sender, MouseEventArgs e)
	{
		//Sambar.api.CreateMenu(0, 0, 300, 300, centerOffset: true);
		Window wnd = Sambar.api.CreateDesktopOverlay();
		SKElement skElement = new();
		skElement.PaintSurface += (s, e) =>
		{
			var canvas = e.Surface.Canvas;
			//canvas.Clear(SKColors.White);
			canvas.DrawImage(SKImage.FromEncodedData(@"C:\Users\Jayakuttan\Pictures\Wallpapers\shibainu.jpeg"), new SKPoint(0, 0));
		};
		wnd.Content = skElement;
		wnd.Show();
	}
}

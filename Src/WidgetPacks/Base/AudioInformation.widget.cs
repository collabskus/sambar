public class AudioInformation : Widget
{
	Window wnd;
	TextBlock textBlock = new();
	public AudioInformation(WidgetEnv ENV) : base(ENV)
	{
		wnd = Sambar.api.CreateWidgetWindow(600, 600, 200, 200);
		wnd.Content = textBlock;
		wnd.Show();

		Sambar.api.MEDIA_INFO_EVENT += (MediaInfo info) =>
		{
			wnd.Dispatcher.Invoke(() => textBlock.Text = info.Title);
		};
		Sambar.api.MEDIA_STOPPED_EVENT += () =>
		{
			wnd.Dispatcher.Invoke(() => textBlock.Text = "");
		};
	}
}

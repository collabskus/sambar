public class AudioVisualizer : Widget
{
	public AudioVisualizer(WidgetEnv ENV) : base(ENV)
	{
		(ThreadWindow t_wnd, WpfPlot plt, Signal signal) = Sambar.api.CreateAudioVisualizer(
			init: wnd =>
			{
				wnd.Height = 400;
				wnd.Width = 200;
				wnd.WindowStyle = WindowStyle.None;
				wnd.AllowsTransparency = true;
				wnd.Background = new SolidColorBrush(Colors.Transparent);
				wnd.ResizeMode = ResizeMode.NoResize;
			}
		);
		t_wnd.Run(() =>
		{
			StackPanel panel = new();
			Border pltBorder = new();

			signal.LineWidth = 2;
			plt.Plot.Layout.Frameless();
			plt.Plot.FigureBackground = new() { Color = ScottColors.Transparent };
			plt.Plot.Axes.Color(ScottColors.Transparent);
			plt.Plot.Axes.FrameColor(ScottColors.Transparent);
			plt.Plot.Grid.LineColor = ScottColors.Transparent;

			pltBorder.BorderBrush = new SolidColorBrush(Colors.Red);
			pltBorder.BorderThickness = new(2);

			pltBorder.Child = plt;
			panel.Children.Add(pltBorder);

			t_wnd.EnsureInitialized().wnd.Content = panel;
		});
	}
}

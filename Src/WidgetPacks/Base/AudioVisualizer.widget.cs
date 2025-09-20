public class AudioVisualizer : Widget
{
	public AudioVisualizer(WidgetEnv ENV) : base(ENV)
	{
		(ThreadWindow t_wnd, WpfPlot plt, FilledSignal signal) = Sambar.api.CreateAudioVisualizer(
			init: wnd =>
			{
				wnd.Left = 20;
				wnd.Top = 20;
				wnd.Width = 200;
				wnd.Height = 60;
				wnd.Background = new SolidColorBrush(Colors.Transparent);
			}
		);
		t_wnd.Run(() =>
		{
			StackPanel panel = new();
			Border pltBorder = new();

			signal.LineWidth = 2;
			signal.fillColor = System.Drawing.Color.Black;
			plt.Width = 200;
			plt.Height = 60;
			plt.Plot.Layout.Frameless();
			plt.Plot.FigureBackground = new() { Color = ScottColors.Transparent };
			plt.Plot.Axes.Color(ScottColors.Transparent);
			plt.Plot.Axes.FrameColor(ScottColors.Transparent);
			plt.Plot.Grid.LineColor = ScottColors.Transparent;

			pltBorder.BorderBrush = new SolidColorBrush(Colors.Red);
			pltBorder.BorderThickness = new(2);

			pltBorder.Child = plt;
			panel.Children.Add(pltBorder);

			t_wnd.wnd.Content = panel;
			t_wnd.wnd.Show();
		});
	}
}

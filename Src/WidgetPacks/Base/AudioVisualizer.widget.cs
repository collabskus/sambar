public class AudioVisualizer: Widget
{
    public AudioVisualizer()
    {
        (ThreadWindow t_wnd, WpfPlot plt) = Sambar.api.CreateAudioVisualizer(
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

            plt.Plot.FigureBackground = new() { Color = ScottColors.Transparent };
            plt.Plot.Axes.Color(ScottColors.Transparent);
            plt.Plot.Axes.FrameColor(ScottColors.Transparent);
            plt.Plot.Grid.LineColor = ScottColors.Transparent;
            //plt.Width = 200;
            //plt.Height = 100;

            pltBorder.BorderBrush = new SolidColorBrush(Colors.Red);
            pltBorder.BorderThickness = new(2);

            pltBorder.Child = plt;
            panel.Children.Add(pltBorder);
            
            t_wnd.EnsureInitialized().wnd.Content = panel;
        });
    }
}

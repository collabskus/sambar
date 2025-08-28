public class AudioVisualizer: Widget
{
    public AudioVisualizer()
    {
        (ThreadWindow t_wnd, WpfPlot plt) = Sambar.api.CreateAudioVisualizer();
        t_wnd.Run(() =>
        {
            t_wnd.EnsureInitialized().wnd.Content = plt;
        });
    }
}

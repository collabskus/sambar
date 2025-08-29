
public class Clock: Widget
{
    TextBlock textBlock = new();

    public Clock(WidgetEnv ENV): base(ENV)
    {
        textBlock.Foreground = Theme.TEXT_COLOR;
        textBlock.FontFamily = Theme.FONT_FAMILY;
        Sambar.api.CLOCK_TICKED += ClockTickedEventHandler;

        this.Content = textBlock;

    }

    public void ClockTickedEventHandler(Time time)
    {
        this.Thread.Invoke(() => {
            textBlock.Text = $"{time.hours}:{time.minutes}:{time.seconds} {time.day}-{time.month}-{time.year}";
        });
    }
}

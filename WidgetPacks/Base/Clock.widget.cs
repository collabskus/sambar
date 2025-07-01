
public class Clock: Widget
{
    TextBlock textBlock = new();

    public Clock()
    {
        this.Content = textBlock;

        Sambar.api.CLOCK_TICKED += ClockTickedEventHandler;
    }

    public void ClockTickedEventHandler(Time time)
    {
        this.Thread.Invoke(() => {
            textBlock.Text = $"{time.hours}:{time.minutes}:{time.seconds} {time.day}-{time.month}-{time.year}";
        });
    }
}

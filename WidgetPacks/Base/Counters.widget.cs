public class Counters: Widget
{
    TextBlock textBlock = new();
    long cpuUsage = 0, downSpeed = 0, upSpeed = 0;
    float memUsage = 0;
    public Counters()
    {
        textBlock.Foreground = Theme.TEXT_COLOR;
        textBlock.FontFamily = Theme.FONT_FAMILY;

        Sambar.api.CPU_PERFORMANCE_NOTIFIED += (_usage) => { cpuUsage = _usage[0]; };
        Sambar.api.NETWORK_SPEED_NOTIFIED += (_speeds) => { downSpeed = _speeds[0]; upSpeed = _speeds[1]; };
        Sambar.api.MEMORY_USAGE_NOTIFIED += (_usage) => { memUsage = _usage[1] - _usage[0]; };
        UpdateText();

        this.Content = textBlock; 
    }
    public void UpdateText()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                string text = $"N: {downSpeed} Kb/s {upSpeed} Kb/s, MEM: {memUsage:0.00} Gb, CPU: {cpuUsage}%";
                this.Thread.Invoke(() => { textBlock.Text = text; });
                await Task.Delay(1000);
            }
        });
    }
}

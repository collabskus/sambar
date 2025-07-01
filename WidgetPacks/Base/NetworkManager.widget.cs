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
        btn.ImageSrc = "C:\\Users\\Jayakuttan\\Downloads\\wireless-16.ico";
        btn.IconWidth = 13;
        btn.IconHeight = 13;
        btn.FontFamily = Theme.FONT_FAMILY;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.MouseDown += ButtonMouseDown;
        this.Content = btn;
        this.Background = Theme.WIDGET_BACKGROUND;

    }

    public void ButtonMouseDown(object? sender, MouseEventArgs e) { 
        Task.Run(() => { Sambar.api.LaunchUri("ms-actioncenter:controlcenter/"); });
    }
}


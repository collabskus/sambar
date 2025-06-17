

public class Tray : Widget
{
    private Api api;
    List<TrayIcon> trayIcons = new();
    public Tray() : base()
    {
        index = 3;
        api = Api.GetInstance();

        trayIcons = api.GetTrayIcons();
        WrapPanel panel = new();
        panel.Orientation = Orientation.Horizontal;
        panel.Width = 100;
        foreach (var icon in trayIcons)
        {
            RoundedButton iconBtn = new();
            iconBtn.Text = "A";
            iconBtn.Height = Theme.BUTTON_HEIGHT;
            iconBtn.Width = Theme.BUTTON_WIDTH;
            iconBtn.Margin = Theme.BUTTON_MARGIN;
            iconBtn.Foreground = Theme.TEXT_COLOR;
            iconBtn.Background = Theme.BUTTON_BACKGROUND;
            iconBtn.HoverColor = Theme.BUTTON_HOVER_COLOR;
            iconBtn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
            iconBtn.MouseDown += (s, e) =>
            {
                icon.RightClick();
            };
            panel.Children.Add(iconBtn);
        }

        RoundedButton btn = new();
        //btn.Text = "T";
        btn.ImageSrc = "C:\\Users\\Jayakuttan\\Downloads\\arrow-204-16.ico";
        btn.IconHeight = 13;
        btn.IconWidth = 13;
        btn.Height = Theme.BUTTON_HEIGHT;
        btn.Width = Theme.BUTTON_WIDTH;
        btn.Margin = Theme.BUTTON_MARGIN;
        btn.Foreground = Theme.TEXT_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
        btn.Margin = new(50, 0, 0, 0);
        bool open = false;
        btn.MouseDown += (s, e) =>
        {
            if(!open) api.CreateMenu(btn, panel);
            open = !open;
        };

        this.Content = btn;
        this.Background = Theme.WIDGET_BACKGROUND;
        this.CornerRadius = Theme.WIDGET_CORNER_RADIUS;

    }
}

//return new Tray();
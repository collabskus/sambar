
public class Tray : Widget
{
    List<TrayIcon> trayIcons = new();
    public Tray() : base()
    {
        index = 3;

        
        trayIcons = Api.GetTrayIcons();
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
        btn.Text = "T";
        btn.Height = Theme.BUTTON_HEIGHT;
        btn.Width = Theme.BUTTON_WIDTH;
        btn.Margin = Theme.BUTTON_MARGIN;
        btn.Foreground = Theme.TEXT_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
        btn.MouseDown += (s, e) =>
        {
            Api.CreateMenu(btn, panel);
        };

        this.Content = btn;
        this.Background = Theme.WIDGET_BACKGROUND;
        this.CornerRadius = Theme.WIDGET_CORNER_RADIUS;

    }
}

//return new Tray();
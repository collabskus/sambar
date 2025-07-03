public class Tray : Widget
{
    List<TrayIcon> trayIcons = new();
    public Tray() : base()
    {
        trayIcons = Sambar.api.GetTrayIcons();
        WrapPanel panel = new();
        panel.Orientation = Orientation.Horizontal;
        //panel.Width = 100;
        foreach (var trayIcon in trayIcons)
        {
            RoundedButton iconBtn = new();
            //iconBtn.Text = "A";
            iconBtn.Height = Theme.BUTTON_HEIGHT;
            iconBtn.Width = Theme.BUTTON_WIDTH;
            iconBtn.Margin = Theme.BUTTON_MARGIN;
            iconBtn.Foreground = Theme.TEXT_COLOR;
            iconBtn.Background = Theme.BUTTON_BACKGROUND;
            iconBtn.HoverColor = Theme.BUTTON_HOVER_COLOR;
            iconBtn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
            iconBtn.FontFamily = Theme.FONT_FAMILY;
            iconBtn.Icon = trayIcon.icon;
            iconBtn.HoverEffect = false;
            iconBtn.Margin = new(10, 10, 0, 0);
            iconBtn.MouseDown += (s, e) =>
            {
                trayIcon.RightClick();
            };
            panel.Children.Add(iconBtn);
        }

        RoundedButton btn = new();
        //btn.Text = "T";
        btn.ImageSrc = @"C:\Users\Jayakuttan\dev\sambar\WidgetPacks\Base\assets\arrow_down.svg";
        btn.IconHeight = 16;
        btn.IconWidth = 16;
        btn.Height = Theme.BUTTON_HEIGHT;
        btn.Width = Theme.BUTTON_WIDTH;
        btn.Margin = Theme.BUTTON_MARGIN;
        btn.Foreground = Theme.TEXT_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
        //btn.Margin = new(50, 0, 0, 0);
        btn.HoverEffect = false;
        btn.MouseDown += (s, e) =>
        {
            Sambar.api.CreateMenu(btn, panel, 100, 100);
        };

        this.Content = btn;
        this.Background = Theme.WIDGET_BACKGROUND;
        this.CornerRadius = Theme.WIDGET_CORNER_RADIUS;

    }
}

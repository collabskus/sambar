public class StartButton : Widget
{
    public StartButton() 
    {
        RoundedButton btn = new();
        btn.Height = 16;
        btn.Width = 16;
        btn.Foreground = Theme.TEXT_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
        btn.FontFamily = Theme.FONT_FAMILY;
        btn.HoverEffect = false;
        btn.ImageSrc = @"C:\Users\Jayakuttan\dev\sambar\WidgetPacks\Base\assets\start.svg";
        btn.MouseDown += ButtonMouseDown;
        this.Content = btn;
    }

    public void ButtonMouseDown(object sender, MouseEventArgs e) 
    {
        Sambar.api.StartMenu();
    }
}

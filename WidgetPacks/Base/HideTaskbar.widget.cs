public class HideTaskbar : Widget
{
    public HideTaskbar() : base()
    {
        RoundedButton btn = new();
        
        btn.Height = Theme.BUTTON_HEIGHT;
        btn.Width = Theme.BUTTON_WIDTH;
        btn.Text = "H";
        btn.Foreground = Theme.TEXT_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
        btn.FontFamily = Theme.FONT_FAMILY;
        btn.HoverEffect = true;
        btn.MouseDown += ButtonMouseDown;
        this.Content = btn;
    }
    public void ButtonMouseDown(object? sender, MouseEventArgs e) {
        Sambar.api.HideTaskbar();    
    }
}

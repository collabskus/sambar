public class StartButton : Widget
{
    public StartButton() 
    {
        RoundedButton btn = new();
        btn.Text = "Start";
        btn.Height = Theme.BUTTON_HEIGHT;
        btn.Width = Theme.BUTTON_WIDTH;
        btn.Foreground = Theme.TEXT_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
        btn.FontFamily = Theme.FONT_FAMILY;
        btn.HoverEffect = true;
        btn.MouseDown += ButtonMouseDown;
        this.Content = btn;
    }

    public void ButtonMouseDown(object sender, MouseEventArgs e) 
    {
        Sambar.api.StartMenu();
    }
}

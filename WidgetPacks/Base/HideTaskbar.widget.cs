


public class HideTaskbar : Widget
{
    private Api api;
    public HideTaskbar() : base()
    {
        index = 2;
        RoundedButton btn = new();
        api = Api.GetInstance();
        
        btn.Height = Theme.BUTTON_HEIGHT;
        btn.Width = Theme.BUTTON_WIDTH;
        btn.Text = "H";
        btn.Foreground = Theme.TEXT_COLOR;
        btn.Background = Theme.BUTTON_BACKGROUND;
        btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
        btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
        btn.MouseDown += ButtonMouseDown;
        this.Content = btn;
    }
    public void ButtonMouseDown(object? sender, MouseEventArgs e) {
        api.HideTaskbar();    
    }
}

//return new HideTaskbar();
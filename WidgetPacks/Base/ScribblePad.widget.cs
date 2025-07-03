public class ScribblePad: Widget
{
    InkCanvas inkCanvas = new();
    RoundedButton btn = new();
    public ScribblePad()
    {
        DrawingAttributes drawingAttributes = new();
        drawingAttributes.Color = Colors.White;
        inkCanvas.DefaultDrawingAttributes = drawingAttributes;

        btn.Height = Theme.BUTTON_HEIGHT;
        btn.Width = Theme.BUTTON_WIDTH;
        btn.Text = "S";
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
        Sambar.api.CreateMenu(btn, inkCanvas, 300, 300); 
    }
}

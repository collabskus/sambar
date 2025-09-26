// GLOBAL RESOURCES USED BY YOUR THEME
// ALL VARIABLES MUST BE STATIC

// Fluent Icons
// https://fluenticons.co/
public class Theme
{
	public static Brush TEXT_COLOR = Utils.BrushFromHex("#000000");
	public static FontFamily FONT_FAMILY = new("JetBrains Mono");

	public static Brush BUTTON_FOREGROUND = new SolidColorBrush(Colors.White);
	public static Brush BUTTON_BACKGROUND = new SolidColorBrush(Colors.Transparent);
	public static Brush BUTTON_PRESSED_BACKGROUND = Utils.BrushFromHex("#575555");
	public static Brush BUTTON_BORDER_COLOR = new SolidColorBrush(Colors.Black);
	public static Brush BUTTON_HOVER_COLOR = Utils.BrushFromHex("#828181");
	public static CornerRadius BUTTON_CORNER_RADIUS = new(0);
	public static Thickness BUTTON_MARGIN = new(0);
	public static int ICON_WIDTH = 14;
	public static int ICON_HEIGHT = 14;
	public static int BUTTON_WIDTH = 19;
	public static int BUTTON_HEIGHT = 19;
	public static Thickness BUTTON_BORDER_THICKNESS = new(0);

	public static Brush WIDGET_BACKGROUND = new SolidColorBrush(Colors.Transparent);
	public static CornerRadius WIDGET_CORNER_RADIUS = new(5);
	public static int WIDGET_GAP = 5;
}

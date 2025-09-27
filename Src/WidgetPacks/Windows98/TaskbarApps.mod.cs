using System.Windows;
using System.Windows.Media;

return (dynamic taskbarApps, dynamic ENV) =>
{
	taskbarApps.BUTTON_PRESSED_BORDER_COLOR = new SolidColorBrush(Colors.Black);
	taskbarApps.BUTTON_PRESSED_BORDER_THICKNESS = new Thickness(1);
	taskbarApps.BUTTON_WIDTH = 18;
	taskbarApps.BUTTON_HEIGHT = 18;
};

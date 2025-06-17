using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Diagnostics;

namespace sambar;
public partial class Api
{
    private void DropdownMenuInit()
    {

    }
    Window? menu;
    public async void CreateMenu(UserControl callingElement, UIElement menuContent, int width = 100, int height = 100)
    {
        Debug.WriteLine($"ContextMenu requested");
        menu?.Close();
        
        menu = new();
        menu.WindowStyle = WindowStyle.None;
        menu.Topmost = true;
        menu.AllowsTransparency = true;
        menu.ResizeMode = ResizeMode.NoResize;
        menu.Width = width;
        menu.Height = height;
        menu.Left = callingElement.PointToScreen(new Point(callingElement.Width/2, callingElement.Height/2)).X - (width/2);
        menu.Top = config.marginYTop + config.height + 5;
        menu.Content = menuContent;

        IntPtr hWnd= new WindowInteropHelper(menu).EnsureHandle();
        uint exStyles = Win32.GetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE);
        Win32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WindowStyles.WS_EX_TOOLWINDOW));
		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		Win32.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

        menu.Show();
    }
}




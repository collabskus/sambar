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
    static Window menu;
    public static void CreateMenu(UserControl callingElement, UIElement menuContent, int width = 100, int height = 100)
    {
        Debug.WriteLine($"ContextMenu requested");
        if(menu != null) { menu.Close(); menu = null; }

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

        Api.barWindow.MouseEnter += (s, e) => { Api.barWindow.Activate(); };
        Api.barWindow.MouseLeave += (s, e) => { if(menu != null) menu.Activate(); };
        FOCUS_CHANGED_EVENT += DestroyMenu;

        IntPtr hWnd = new WindowInteropHelper(menu).EnsureHandle();
        uint exStyles = Win32.GetWindowLong(hWnd, -20);
        Win32.SetWindowLong(hWnd, -20, (int)(exStyles | (uint)WindowStyles.WS_EX_TOOLWINDOW));
		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		Win32.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

        menu.Show();
    }

    static async void DestroyMenu(FocusChangedMessage msg)
    {
        if(msg.name == "Desktop")
        {
            FOCUS_CHANGED_EVENT -= DestroyMenu;
            await Task.Delay(50);
            barWindow.Dispatcher.Invoke(() => menu.Close());
            menu = null;
        }
    }
}




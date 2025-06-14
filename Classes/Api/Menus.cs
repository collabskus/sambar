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
    static IntPtr menu_hWnd;
    public static async void CreateMenu(UserControl callingElement, UIElement menuContent, int width = 100, int height = 100)
    {
        Debug.WriteLine($"ContextMenu requested");
       
        if(menu != null) 
        { 
            Debug.WriteLine("destroying menu because another is being created");
            DestroyMenu(); 
        }
        
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

        //Api.barWindow.MouseEnter += (s, e) => { Api.barWindow.Activate(); };
        //Api.barWindow.MouseLeave += (s, e) => { if(menu != null) menu.Activate(); };

        menu_hWnd = new WindowInteropHelper(menu).EnsureHandle();
        uint exStyles = Win32.GetWindowLong(menu_hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE);
        Win32.SetWindowLong(menu_hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WindowStyles.WS_EX_TOOLWINDOW));
		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		Win32.DwmSetWindowAttribute(menu_hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

        menu.Show();
        await Task.Delay(100); 
        FOCUS_CHANGED_EVENT += MenuFocusLostHandler;
    }

    static async void MenuFocusLostHandler(FocusChangedMessage msg)
    {
        Debug.WriteLine($"focus changed, name: {msg.name}, class: {msg.className}, hWnd: {msg.hWnd}");
        //if(msg.name == "Desktop")
        if (msg.hWnd != 0)
        {
            if (Utils.IsContextMenu(msg.hWnd)) { Console.WriteLine("returning because context"); return; }
            if (msg.hWnd == menu_hWnd) { Console.WriteLine("returning because itself"); return; }
            if (!Utils.IsWindowVisible(msg.hWnd)) { Console.WriteLine("returning because not visible"); return; }
        }
        else
        {
            IntPtr hWnd = Win32.GetForegroundWindow();
            Debug.WriteLine($"hWnd 0 so using GFW: {hWnd}");
            if (Utils.IsContextMenu(hWnd)) return;
            if (!Utils.IsWindowVisible(hWnd)) return;
            if (hWnd == menu_hWnd) return;
        }

        Debug.WriteLine($"Closing menu due to, window: {msg.name}, class: {msg.className}");
        FOCUS_CHANGED_EVENT -= MenuFocusLostHandler;
        await Task.Delay(50);
        barWindow.Dispatcher.Invoke(() => 
        {
            Debug.WriteLine("destroying menu from focus lost");
            DestroyMenu();
        });
    }

    public static void DestroyMenu()
    {
        if (menu == null) return;
        menu.Close(); 
        menu = null;
    }
}




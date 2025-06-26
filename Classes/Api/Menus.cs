using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;

namespace sambar;
public partial class Api
{
    private void DropdownMenuInit()
    {

    }
    Window? menu;
    private IntPtr hWnd;
    public async void CreateMenu(UserControl callingElement, UIElement menuContent, int width = 100, int height = 100)
    {
        Debug.WriteLine($"ContextMenu requested");
        menu?.Close();
        
        menu = new();
        menu.Title = "sambarContextMenu";
        menu.WindowStyle = WINDOWSTYLE.None;
        menu.Topmost = true;
        menu.AllowsTransparency = true;
        menu.ResizeMode = ResizeMode.NoResize;
        menu.Width = width;
        menu.Height = height;
        menu.Left = callingElement.PointToScreen(new Point(callingElement.Width/2, callingElement.Height/2)).X - (width/2);
        menu.Top = config.marginYTop + config.height + 5;
        menu.Content = menuContent;

        hWnd= new WindowInteropHelper(menu).EnsureHandle();
        uint exStyles = Win32.GetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE);
        Win32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WINDOWSTYLE.WS_EX_TOOLWINDOW));
		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		Win32.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

        menu.Show();
        FOCUS_CHANGED_EVENT += MenuFocusChangedHandler;
    }

    private async void MenuFocusChangedHandler(FocusChangedMessage msg)
    {
        Debug.WriteLine($"FocusChanged, name: {msg.name}, class: {msg.className}, controlType: {msg.controlType}");
        // dont respond to focus events if menu not open
        if (menu == null) return;
        // Filter using ControlType
        if ( 
            msg.controlType == ControlType.MENU ||
            msg.controlType == ControlType.MENUITEM ||
            msg.controlType == ControlType.LIST ||
            msg.controlType == ControlType.LISTITEM
        ) return;
        // if focus changed to itself ignore
        if (msg.name == "sambarContextMenu") return;
        if (msg.name == "Bar" && msg.className == "Window") return;
        if (Utils.IsContextMenu(msg.hWnd)) return;
        if (msg.hWnd == 0) return;
        // wait for trayIconMenuChildren to get filled if icon children havent been retrieved
        // and also wait so that focus changed event is not consumed when menu it is opening
        await Task.Delay(300);
        if (!trayIconMenuChildren.ContainsKey(rightClickedTrayIconIndex))
        {
           Debug.WriteLine($"Closing menu by losing focus to non-menu item: {msg.name}, {msg.className}");
           barWindow.Dispatcher.Invoke(() => { menu?.Close(); });
           menu = null;
           return;
        }

        var classNames = trayIconMenuChildren[rightClickedTrayIconIndex].Select(_msg => _msg.className);
        var names = trayIconMenuChildren[rightClickedTrayIconIndex].Select(_msg => _msg.name);
        bool classMatch = classNames.Contains(msg.className);
        bool namesMatch = names.Contains(msg.name);
        if (!classMatch && !namesMatch)
        {
            Debug.WriteLine($"Closing menu by losing focus to non-menu item: {msg.name}, {msg.className}");
            barWindow.Dispatcher.Invoke(() => { menu?.Close(); });
            menu = null;
        }
    }
}




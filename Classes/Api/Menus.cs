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
    public async void CreateMenu(UserControl callingElement, UIElement menuContent, int width = 100, int height = 100)
    {
        Menu menu = new(Sambar.api, callingElement, menuContent, width: 100, height: 100);
    }
}

public class Menu: Window
{
    Api api;
    nint hWnd;
    public Menu(Api api, UserControl callingElement, UIElement menuContent, int width = 100, int height = 100)
    {
        this.api = api;

        this.Title = "sambarContextMenu";
        this.WindowStyle = WindowStyle.None;
        this.Topmost = true;
        this.AllowsTransparency = true;
        this.ResizeMode = ResizeMode.NoResize;
        this.Width = width;
        this.Height = height;
        this.Left = callingElement.PointToScreen(new Point(callingElement.Width/2, callingElement.Height/2)).X - (width/2);
        this.Top = Sambar.api.config.marginYTop + Sambar.api.config.height + 5;
        this.Content = menuContent;
        

        hWnd = new WindowInteropHelper(this).EnsureHandle();
        uint exStyles = User32.GetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE);
        User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WINDOWSTYLE.WS_EX_TOOLWINDOW));
        int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        Dwmapi.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

        this.Show();
        Task.Delay(100);
        Api.FOCUS_CHANGED_EVENT += MenuFocusChangedHandler;
    }

    private async void MenuFocusChangedHandler(FocusChangedMessage msg)
    {
        Debug.WriteLine($"MenuFocusChanged, name: {msg.name}, class: {msg.className}, controlType: {msg.controlType}");
        // Filter using ControlType
        if ( 
            msg.controlType == ControlType.MENU ||
            msg.controlType == ControlType.MENUITEM ||
            msg.controlType == ControlType.LIST ||
            msg.controlType == ControlType.LISTITEM ||
            msg.controlType == ControlType.BUTTON
        ) return;
        //// if focus changed to itself ignore
        if (msg.name == "sambarContextMenu") return;
        if (msg.name == "Bar" && msg.className == "Window") return;
        if (Utils.IsContextMenu(msg.hWnd)) return;
        //if (msg.hWnd == 0) return;
        // wait for trayIconMenuChildren to get filled if icon children havent been retrieved
        // and also wait so that focus changed event is not consumed when menu it is opening
        await Task.Delay(api.WINDOW_CAPTURE_DURATION);
        if (!api.capturedWindows.Select(_msg => _msg.className).Contains(msg.className))
        {
           Debug.WriteLine($"Closing menu by losing focus to non-menu item: {msg.name}, {msg.className}");
           api.barWindow.Dispatcher.Invoke(() => { this.Close(); });
        }
    }
}




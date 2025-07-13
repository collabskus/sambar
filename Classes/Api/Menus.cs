using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using Windows.ApplicationModel.VoiceCommands;

namespace sambar;
public partial class Api
{
    private void DropdownMenuInit()
    {

    }
    public Menu CreateMenu(UserControl callingElement, int width = 100, int height = 100)
    {
        Menu menu = new(callingElement, width, height);
        return menu;
    }
}

public class Menu: Window
{
    nint hWnd;
    int _left, _top, _right, _bottom;
    public Menu(UserControl callingElement, int width, int height)
    {
        this.Title = "sambarContextMenu";
        this.WindowStyle = WindowStyle.None;
        this.Topmost = true;
        this.AllowsTransparency = true;
        this.ResizeMode = ResizeMode.NoResize;
        this.Width = width;
        this.Height = height;
        this.Left = callingElement.PointToScreen(new Point(callingElement.Width/2, callingElement.Height/2)).X - (width/2);
        this.Left = this.Left < Sambar.api.config.marginXLeft ? Sambar.api.config.marginXLeft : this.Left;
        this.Top = Sambar.api.config.marginYTop + Sambar.api.config.height + 5;

        hWnd = new WindowInteropHelper(this).EnsureHandle();
        uint exStyles = User32.GetWindowLong(hWnd, GETWINDOWLONG.GWL_EXSTYLE);
        User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WINDOWSTYLE.WS_EX_TOOLWINDOW));
        int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        Dwmapi.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

        this.Show();
        Task.Delay(100);

        _left = (int)this.Left;
        _top = (int)this.Top;
        _right = _left + (int)this.Width;
        _bottom = _top + (int)this.Height;
        Logger.Log($"Menu, L: {_left}, T: {_top}, R: {_right}, B: {_bottom}");

        Api.FOCUS_CHANGED_EVENT += MenuFocusChangedHandler;
    }

    private async void MenuFocusChangedHandler(FocusChangedMessage msg)
    {
        Logger.Log($"MenuFocusChanged, name: {msg.name}, class: {msg.className}, controlType: {msg.controlType}");
        if (msg.name == "Desktop") Sambar.api.barWindow.Dispatcher.Invoke(() => AnimatedClose());
        // if cursor inside menu
        User32.GetCursorPos(out POINT cursorPos);
        if (cursorPos.X > _left && cursorPos.X < _right)
            if (cursorPos.Y > _top && cursorPos.Y < _bottom)
                return;
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
        await Task.Delay(Sambar.api.WINDOW_CAPTURE_DURATION);
        if (!Sambar.api.capturedWindows.Select(_msg => _msg.className).Contains(msg.className))
        {
           Logger.Log($"Closing menu by losing focus to non-menu item: {msg.name}, {msg.className}");
            Sambar.api.barWindow.Dispatcher.Invoke(() =>  AnimatedClose());
        }
    }
    
    public void AminatedShow()
    {
        this.Show();
    }
    public void AnimatedClose()
    {
        this.Close();
    }

}




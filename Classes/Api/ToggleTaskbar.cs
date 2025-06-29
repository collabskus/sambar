using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace sambar;

public partial class Api {

    // Constructor
    bool _isToggleTaskbarInitRun = false;
    private void ToggleTaskbarInit()
    {
        taskbar_hWnd = User32.FindWindow("Shell_TrayWnd", null);
        WINDOWPLACEMENT lpwndpl = new();
        User32.GetWindowPlacement(taskbar_hWnd, ref lpwndpl);
        Debug.WriteLine($"ToggleTaskBarInit: {lpwndpl.showCmd}");
        bool _isToggleTaskbarInitRun = true;
    }

    IntPtr taskbar_hWnd;
    bool shown = true;
    private void ToggleTaskbar()
    {
        if (taskbar_hWnd == null) return;
        if (shown) {
            SetTaskbarState(APPBARSTATE.AutoHide);
            User32.ShowWindow(taskbar_hWnd, SHOWWINDOW.SW_HIDE);
            shown = false; 
        }
        else { 
            SetTaskbarState(APPBARSTATE.AlwaysOnTop);
            User32.ShowWindow(taskbar_hWnd, SHOWWINDOW.SW_SHOW);
            shown = true;
        }
    } 

    private void SetTaskbarState(APPBARSTATE state) {
        APPBARDATA msgData = new();
        msgData.cbSize = (uint)Marshal.SizeOf<APPBARDATA>();
        msgData.hWnd = taskbar_hWnd;
        msgData.lParam = (uint)state;
        Shell32.SHAppBarMessage((uint)APPBARMESSAGE.SetState, ref msgData);
    }

    // API Endpoint
    public void HideTaskbar()
    {
        if(!_isToggleTaskbarInitRun) ToggleTaskbarInit();
        ToggleTaskbar();
    }
}



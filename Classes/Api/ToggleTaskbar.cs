using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace sambar;

public partial class Api {

    // Constructor
    static bool _isToggleTaskbarInitRun = false;
    private void ToggleTaskbarInit()
    {
        taskbar_hWnd = Win32.FindWindow("Shell_TrayWnd", null);
        WINDOWPLACEMENT lpwndpl = new();
        Win32.GetWindowPlacement(taskbar_hWnd, ref lpwndpl);
        Debug.WriteLine($"ToggleTaskBarInit: {lpwndpl.showCmd}");
        bool _isToggleTaskbarInitRun = true;
    }

    IntPtr taskbar_hWnd;
    bool shown = true;
    private void ToggleTaskbar()
    {
        if (taskbar_hWnd == null) return;
        if (shown) {
            SetTaskbarState(AppBarStates.AutoHide);
            Win32.ShowWindow(taskbar_hWnd, SHOWWINDOW.SW_HIDE);
            shown = false; 
        }
        else { 
            SetTaskbarState(AppBarStates.AlwaysOnTop);
            Win32.ShowWindow(taskbar_hWnd, SHOWWINDOW.SW_SHOW);
            shown = true;
        }
    } 

    private void SetTaskbarState(AppBarStates state) {
        APPBARDATA msgData = new();
        msgData.cbSize = (uint)Marshal.SizeOf<APPBARDATA>();
        msgData.hWnd = taskbar_hWnd;
        msgData.lParam = (uint)state;
        Win32.SHAppBarMessage((uint)AppBarMessages.SetState, ref msgData);
    }

    // API Endpoint
    public static void HideTaskbar()
    {
        if(!_isToggleTaskbarInitRun) api.ToggleTaskbarInit();
        api.ToggleTaskbar();
    }
}



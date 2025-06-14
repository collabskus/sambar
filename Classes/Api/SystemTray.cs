using System.Diagnostics;
using System.Runtime.InteropServices;
using Interop.UIAutomationClient;
using Microsoft.VisualStudio.Shell.Interop;

namespace sambar;

public partial class Api {

    private List<TrayIcon> trayIcons = new();

    // Constructor
    static bool _isSystemTrayInitRun = false;
    private void SystemTrayInit()
    {
        IntPtr hWnd_Overflow = Win32.FindWindow("TopLevelWindowForOverflowXamlIsland", null);
        IntPtr hWnd_IconContainer = Win32.FindWindowEx(hWnd_Overflow, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null);

        var innerIconContainer = ui.ElementFromHandle(hWnd_IconContainer);
        int classPropertyId = 30012;
        var icons = innerIconContainer.FindAll(TreeScope.TreeScope_Children, ui.CreateTrueCondition());

        for(int i = 0; i < icons.Length; i++) {
            var icon = icons.GetElement(i);
            TrayIcon trayIcon = new(hWnd_Overflow, icon as IUIAutomationElement3);
            trayIcon.name = icon.CurrentName;
            trayIcons.Add(trayIcon);
        }

        STRUCTURE_CHANGED_EVENT += (msg) => {
            MoveContextMenuToCursor(msg.hWnd);
            Debug.WriteLine($"Moving context menu: {msg.name}, {msg.className}, {msg.hWnd}");
        };

        _isSystemTrayInitRun = true;
    }
    
    // API Endpoint
    public static List<TrayIcon> GetTrayIcons()
    {
        if(!_isSystemTrayInitRun) api.SystemTrayInit();
        return api.trayIcons;
    }

    private void MoveContextMenuToCursor(IntPtr hWnd)
    {
        if (menu == null) return;
        if (!Utils.IsContextMenu(hWnd)) return;
        if (!Utils.IsWindowVisible(hWnd)) return;
        
        Win32.GetCursorPos(out POINT cursorPos);
        bool result = Win32.SetWindowPos(hWnd, IntPtr.Zero, cursorPos.X, cursorPos.Y, 0, 0, (uint)SETWINDOWPOS.SWP_NOSIZE);
    }
}

public class TrayIcon {
    public string name;
    public IntPtr hWnd_Overflow;
    public IUIAutomationElement3 element;
    public int centerX, centerY, centerWithinContainerX, centerWithinContainerY;

    public TrayIcon(IntPtr hWnd_Overflow, IUIAutomationElement3 element)
    {
        this.hWnd_Overflow = hWnd_Overflow;
        this.element = element;
    }

    public void RightClick()
    {
        Win32.ShowWindowAsync(hWnd_Overflow, SHOWWINDOW.SW_SHOW);
        element.ShowContextMenu();
        Win32.ShowWindowAsync(hWnd_Overflow, SHOWWINDOW.SW_HIDE);
    }
}

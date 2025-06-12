using System.Diagnostics;
using System.Runtime.InteropServices;
using Interop.UIAutomationClient;

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
            TrayIcon trayIcon = new();
            trayIcon.name = icon.CurrentName;
            trayIcon.hWnd = hWnd_Overflow;
            trayIcon.element = icon as IUIAutomationElement3;
            trayIcons.Add(trayIcon);
        }
        _isSystemTrayInitRun = true;
    }
    
    // API Endpoint
    public static List<TrayIcon> GetTrayIcons()
    {
        if(!_isSystemTrayInitRun) api.SystemTrayInit();
        return api.trayIcons;
    }
}

public class TrayIcon {
    public string name;
    public IntPtr hWnd;
    public IUIAutomationElement3 element;
    public void RightClick()
    {
        Win32.ShowWindowAsync(hWnd, SHOWWINDOW.SW_SHOW);
        element.ShowContextMenu();
        Win32.ShowWindowAsync(hWnd, SHOWWINDOW.SW_HIDE);
    }
}

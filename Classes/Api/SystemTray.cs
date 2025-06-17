using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.TextFormatting;
using Interop.UIAutomationClient;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sambar;

public partial class Api {

    private List<TrayIcon> trayIcons = new();

    // Constructor
    bool _isSystemTrayInitRun = false;
    private void SystemTrayInit()
    {
        IntPtr hWnd_Overflow = Win32.FindWindow("TopLevelWindowForOverflowXamlIsland", null);
        IntPtr hWnd_IconContainer = Win32.FindWindowEx(hWnd_Overflow, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null);

        var innerIconContainer = ui.ElementFromHandle(hWnd_IconContainer);
        int classPropertyId = 30012;
        var icons = innerIconContainer.FindAll(TreeScope.TreeScope_Children, ui.CreateTrueCondition());
        var trayMenuDimensions = Utils.GetWindowDimensions(hWnd_Overflow);
        
        for(int i = 0; i < icons.Length; i++) {
            var icon = icons.GetElement(i);
            TrayIcon trayIcon = new(i, api, hWnd_Overflow, icon as IUIAutomationElement3);
            trayIcon.name = icon.CurrentName;
            trayIcons.Add(trayIcon);
        }

        STRUCTURE_CHANGED_EVENT += CaptureMenuChildren;

        _isSystemTrayInitRun = true;
    }

    private bool trayIconRightClicked = false;
    private Dictionary<int, List<StructureChangedMessage>> trayIconMenuChildren = new();
    private int rightClickedTrayIconIndex; 
    public async void StartCapturingMenuChildren(int iconIndex)
    { 
        trayIconRightClicked = true;
        rightClickedTrayIconIndex = iconIndex;
        if (trayIconMenuChildren.ContainsKey(rightClickedTrayIconIndex))
        {
            trayIconMenuChildren[rightClickedTrayIconIndex].ForEach(msg => Utils.MoveWindowToCursor(msg.hWnd));
            return;
        }
        await Task.Delay(2000);
        Debug.WriteLine("Collection finished");
        trayIconRightClicked = false;
        if (trayIconMenuChildren.ContainsKey(rightClickedTrayIconIndex))
        {
            trayIconMenuChildren[rightClickedTrayIconIndex]
                .Add(new StructureChangedMessage() { name = "END" }); // signify collection finished
        }
    } 
    private void CaptureMenuChildren(StructureChangedMessage msg)
    {
        Debug.WriteLine($"StructureChanged, name: {msg.name}, class: {msg.className}, type: {msg.controlType}");
        if (msg.className == "sambarContextMenu") return;
        if (trayIconRightClicked)
        {
            Utils.MoveWindowToCursor(msg.hWnd);
            if (!trayIconMenuChildren.ContainsKey(rightClickedTrayIconIndex))
            {
                trayIconMenuChildren[rightClickedTrayIconIndex] = new();
                trayIconMenuChildren[rightClickedTrayIconIndex].Add(msg); 
                return;
            }

            if (trayIconMenuChildren[rightClickedTrayIconIndex].Last().name != "END")
            {
                trayIconMenuChildren[rightClickedTrayIconIndex].Add(msg); 
            }
        }
    }

    // API Endpoint
    public List<TrayIcon> GetTrayIcons()
    {
        if(!_isSystemTrayInitRun) api.SystemTrayInit();
        return api.trayIcons;
    }
}

public class TrayIcon
{
    private Api api;
    public int index;
    public string name;
    public IntPtr hWnd_Overflow;
    public IUIAutomationElement3 element;

    public TrayIcon(int index, Api api, IntPtr hWnd_Overflow, IUIAutomationElement3 element)
    {
        this.index = index;
        this.api = api;
        this.hWnd_Overflow = hWnd_Overflow;
        this.element = element;
    }

    public void RightClick()
    {
        Task.Run(() => api.StartCapturingMenuChildren(index));
        Win32.ShowWindowAsync(hWnd_Overflow, SHOWWINDOW.SW_SHOW);
        Utils.MoveWindow(hWnd_Overflow, 0, 0);
        element.ShowContextMenu();
        Win32.ShowWindowAsync(hWnd_Overflow, SHOWWINDOW.SW_HIDE);
    }
}
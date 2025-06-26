using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using Interop.UIAutomationClient;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32;

namespace sambar;

public partial class Api {

    private List<TrayIcon> trayIcons = new();
    private RegistryKey trayIconsRegistryKeyRoot;
    private List<TrayIconRegKey> UIOrderListRegKeys = new();

    // Constructor
    bool _isSystemTrayInitRun = false;
    private void SystemTrayInit()
    {
        // for trayIcon's images
        trayIconsRegistryKeyRoot = Registry.CurrentUser.OpenSubKey("Control Panel").OpenSubKey("NotifyIconSettings");
        byte[] raw_UIOrderList = (byte[])trayIconsRegistryKeyRoot.GetValue("UIOrderList");
        string fullHex = Convert.ToHexStringLower(raw_UIOrderList);
        Debug.WriteLine($"UIOrderList: {fullHex}");
        fullHex.Chunk(16).ToList().ForEach(chunk => {
            string hex = new(chunk);
            char[][] chunks = hex.Chunk(2).ToArray();
            chunks = chunks.Reverse().ToArray();
            string reverse = "";
            chunks.ToList().ForEach(_c => reverse += new string(_c));
            ulong num = (ulong)Int64.Parse(reverse, System.Globalization.NumberStyles.HexNumber);
            //Debug.WriteLine($"chunk: {hex}, reverse: {reverse}, decimal: {num}");
            TrayIconRegKey key = new();
            key.parentKey = $"{num}";
            key.ExecutablePath = (string)trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("ExecutablePath");
            key.IconGuid = (string)trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("IconGuid");
            key.IconSnapshot = (byte[])trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("IconSnapshot");
            key.UID = unchecked((uint?)(int?)trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("UID"));
            UIOrderListRegKeys.Add(key);
            Debug.WriteLine($"{num}, {key.ExecutablePath}");
        });

        Process[] runningProcesses = Process.GetProcesses();
        runningProcesses.ToList().ForEach(p => Debug.WriteLine("executable path: " + p.ProcessName));
        List<string> trayProcesses = new();
        // filter out the non running ones
        UIOrderListRegKeys = UIOrderListRegKeys
            .Where(
                key => runningProcesses
                        .Select(_p => _p.ProcessName)
                        .ToList()
                        .Contains(key.ExecutablePath
                                        .Split(@"\")
                                        .Last()
                                        .Replace(".exe", "")
            ))
            .ToList();


        //trayIconRegKeys.ForEach(item => Debug.WriteLine($"running: {item.ExecutablePath}"));

        // from these use either IconGuid or hWnd + UID to figure out which is in overflow
        // tray by Shell_NotifyIconGetRect()
        List<GUIProcess> allWindows = Utils.EnumWindowProcesses();
        List<TrayIconRegKey> actuallyInTrayRegKeys = new();
        foreach(var key in UIOrderListRegKeys)
        {
            _NOTIFYICONIDENTIFIER identifier = new();
            identifier.cbSize = (uint)Marshal.SizeOf<_NOTIFYICONIDENTIFIER>();
            long result;    
            // IconGuid exists
            if(key.IconGuid != null)
            {
                Debug.WriteLine($"{key.ExecutablePath}, IconGuid, parent: {key.parentKey}");
                identifier.guidItem = new(key.IconGuid);
                if((result = Shell32.Shell_NotifyIconGetRect(ref identifier, out RECT iconLocation)) != 0) 
                {
                    Debug.WriteLine($"failed: {result}");
                }
                if (
                    iconLocation.Left != 0 ||
                    iconLocation.Top != 0 ||
                    iconLocation.Right != 0 ||
                    iconLocation.Bottom != 0
                ) actuallyInTrayRegKeys.Add(key);
            }
            // UID + hWnd
            else if(key.UID != null)
            {
                identifier.UID = (uint)key.UID;
                string exePath = key.ExecutablePath;
                string processName = exePath.Split(@"\").Last().Replace(".exe", "");
                if (processName == "Taskmgr") identifier.UID = 0;
                var process = runningProcesses.Where(_p => _p.ProcessName == processName).First();
                GUIProcess guiProcess = allWindows.Where(gui_p => gui_p.name == process.ProcessName).First();
                foreach (var window in guiProcess.windows)
                {
                    identifier.hWnd = window.hWnd;
                    if((result = Shell32.Shell_NotifyIconGetRect(ref identifier, out RECT iconLocation)) != 0) 
                    {
                        Debug.WriteLine($"failed: {result}");
                    }
                    if (
                        iconLocation.Left != 0 ||
                        iconLocation.Top != 0 ||
                        iconLocation.Right != 0 ||
                        iconLocation.Bottom != 0
                    )
                    {
                        actuallyInTrayRegKeys.Add(key);
                        break;
                    }
                }
                //nint hWnd = Utils.GetHWNDFromPID(process.Id);
                Debug.WriteLine($"{key.ExecutablePath}, UID: {key.UID}, handle: {identifier.hWnd}, parent: {key.parentKey}");
            }
            else
            {
                Debug.WriteLine($"BOTH NULL, path: {key.ExecutablePath}, UID: {key.UID}, parent: {key.parentKey}");
            }
        }

        actuallyInTrayRegKeys.ForEach(key => Debug.WriteLine("regInTrayFound: " + key.ExecutablePath));

        // Enumerate TrayOverflowMenu
        IntPtr hWnd_Overflow = User32.FindWindow("TopLevelWindowForOverflowXamlIsland", null);
        IntPtr hWnd_IconContainer = User32.FindWindowEx(hWnd_Overflow, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null);

        var innerIconContainer = ui.ElementFromHandle(hWnd_IconContainer);
        var icons = innerIconContainer.FindAll(TreeScope.TreeScope_Children, ui.CreateTrueCondition());
        
        for(int i = 0; i < icons.Length; i++) {
            var icon = icons.GetElement(i);
            TrayIcon trayIcon = new(i, api, hWnd_Overflow, icon as IUIAutomationElement3);
            trayIcon.name = icon.CurrentName;
            GetTrayIconImage(trayIcon.name);
            trayIcons.Add(trayIcon);
        }
        
        STRUCTURE_CHANGED_EVENT += CaptureMenuChildren;
        
        // icons obtained through registry enumeration must be same as that obtained
        // from IUIAutomation
        //Trace.Assert(actuallyInTrayRegKeys.Count == icons.Length);

        // flag for external API
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
        // 2s capturing window for CaptureMenuChildren()
        await Task.Delay(2000);
        Debug.WriteLine("Collection finished");
        trayIconRightClicked = false;
        if (trayIconMenuChildren.ContainsKey(rightClickedTrayIconIndex))
        {
            trayIconMenuChildren[rightClickedTrayIconIndex]
                .Add(new StructureChangedMessage() { name = "END" }); // mark list as completed
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

    void GetTrayIconImage(string tooltipText)
    {
        var subKeys = trayIconsRegistryKeyRoot.GetSubKeyNames();
        string? iconKeyName = subKeys
            .ToList()
            .Where(key =>
            {
                return 
                    (string?)trayIconsRegistryKeyRoot
                    ?.OpenSubKey(key)
                    ?.GetValue("InitialTooltip") == tooltipText;
            })
            ?.FirstOrDefault();
        Debug.WriteLine($"regkey found: {iconKeyName}");
    }
}

public class TrayIcon
{
    private Api api;
    // index (position) of the icon in the tray
    public int index;
    public string name;
    // hWnd of the xaml overflow window
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
        User32.ShowWindowAsync(hWnd_Overflow, SHOWWINDOW.SW_SHOW);
        element.ShowContextMenu();
        User32.ShowWindowAsync(hWnd_Overflow, SHOWWINDOW.SW_HIDE);
    }
}

public class TrayIconRegKey
{
    public string parentKey;
    public string ExecutablePath;
    public string IconGuid;
    public byte[] IconSnapshot;
    public uint? UID;
}




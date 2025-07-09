using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;

namespace sambar;

public partial class Api
{
    TaskbarInterceptor interceptor;
    public void TaskbarInterceptorInit()
    {
        interceptor = new();
    }
    Dictionary<ICONACTION, List<WINDOWMESSAGE>> ICONACTION_MAP_V0_3 = new()
    {
        { ICONACTION.RIGHT_CLICK, [ WINDOWMESSAGE.WM_RBUTTONDOWN, WINDOWMESSAGE.WM_RBUTTONUP ] },
    };
    Dictionary<ICONACTION, List<WINDOWMESSAGE>> ICONACTION_MAP_V3 = new()
    {
        { ICONACTION.RIGHT_CLICK, [ WINDOWMESSAGE.WM_CONTEXTMENU ] },

    };
    public void ImpersonateTrayEvent(NOTIFYICONDATA nid, ICONACTION msg)
    {
        User32.GetWindowThreadProcessId((nint)nid.hWnd, out uint processId);
        int result = User32.AllowSetForegroundWindow(processId);
        Debug.WriteLine($"ImpersonateTrayEvent(): {processId}, result: {result}, win32: {Marshal.GetLastWin32Error()}");
        if(nid.uTimeoutOrVersion.uVersion <= 3)
        {
            foreach(var winmsg in ICONACTION_MAP_V0_3[msg])
            {
                User32.SendMessage(
                    (nint)nid.hWnd, 
                    nid.uCallbackMessage, 
                    (nint)nid.uID, 
                    Utils.MAKELPARAM((short)winmsg, 0)
                );
            }
            
        } 
        else
        {
            foreach(var winmsg in ICONACTION_MAP_V3[msg])
            {
                User32.GetCursorPos(out POINT cursorPos);
                User32.SendMessage(
                    (nint)nid.hWnd, 
                    nid.uCallbackMessage, 
                    Utils.MAKEWPARAM((short)cursorPos.X, (short)cursorPos.Y), 
                    Utils.MAKELPARAM((short)winmsg, (short)nid.uID)
                );
            }
        }
    }
    public List<TrayIcon> GetTrayIcons()
    {
        var icons = interceptor.GetTrayIcons();
        Debug.WriteLine($"GetTrayIcons(): {icons.Count()}");
        return icons;
    }

}
/// <summary>
/// An invisible window to intercept(or listen to) taskbar messages.
/// We create a window with the preexisting Taskbar class
/// "Shell_TrayWnd". Any messages intended for the real taskbar
/// will be captured by our invisible window
/// </summary>
public class TaskbarInterceptor
{
    nint hWnd;
    public nint originalTray_hWnd;
    CancellationTokenSource cts;
    public TaskbarInterceptor() 
    {
        // capture the original taskbar hWnd before creating the interceptor window
        // with the same class name so as to forward messages to it. why is this necessary ?
        // well its not just for wanting to use the original taskbar alongside sambar but also
        // in instances where for example certain context menus are natively created by the taskbar
        // and not by the actual tray icon application
        originalTray_hWnd = User32.FindWindow("Shell_TrayWnd", null);
        cts = new();
        Task.Run(() =>
        {
            WNDCLASSEX wc = new();
            wc.cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>();
            wc.lpfnWndProc = WndProc;
            // run GetModuleHandle() in the same thread as CreateWindowEx()
            wc.hInstance = Kernel32.GetModuleHandle(null);
            wc.lpszClassName = "Shell_TrayWnd";

            ushort result = User32.RegisterClassEx(ref wc);
            if (result == 0)
            {
                Debug.WriteLine($"RegisterClassEx() failed: {Marshal.GetLastWin32Error()}");
            }
            else
            {
                Debug.WriteLine($"RegisterClassEx() success !");
            }

            hWnd = User32.CreateWindowEx(
                WINDOWSTYLE.WS_EX_TOPMOST | WINDOWSTYLE.WS_EX_TOOLWINDOW,
                wc.lpszClassName,
                null,
                WINDOWSTYLE.WS_POPUP | WINDOWSTYLE.WS_CLIPCHILDREN | WINDOWSTYLE.WS_CLIPSIBLINGS,
                0,
                0,
                0,
                0,
                nint.Zero,
                nint.Zero,
                nint.Zero,
                nint.Zero
            );
            
            // Set window as topmost first and set a timer that keeps on doing just that
            if (User32.SetWindowPos(hWnd, (nint)(-1), 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE) == 0)
            {
                Debug.WriteLine($"SetWindowPos() failed: {Marshal.GetLastWin32Error()}");
            }

            User32.SetTimer(hWnd, 1, 100, null);

            RefreshTaskbar();
            
            while (User32.GetMessage(out MSG msg, hWnd, 0, 0) > 0)
            {
                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }
        }, cts.Token);
    }

    public TrayIconsManager trayIconsManager = new();
    //public List<TrayIcon> overflowIcons = new();
    List<string> NON_OVERFLOW_CLASSES = 
    [
        "ATL:00007FFE3066B050", // SPEAKER
        "BluetoothNotificationAreaIconWindowClass",
        "ASYNCUI_NOTIFYICON_WINDOW_CLASS"
    ];
    /// <summary>
    /// WndProc for our taskbar interceptor
    /// </summary>
    /// <returns>
    /// Data specific to the Window Message it recieves.
    /// The return code is sent to whoever sents the window a window message.
    /// </returns>
    nint WndProc(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam)
    {
        Debug.WriteLine($"Message: {uMsg}");
        switch (uMsg)
        {
            case WINDOWMESSAGE.WM_CLOSE:
                User32.DestroyWindow(hWnd);
                break;
            case WINDOWMESSAGE.WM_DESTROY:
                User32.PostQuitMessage(0);
                break;
            case WINDOWMESSAGE.WM_TIMER:
                User32.SetWindowPos(hWnd, (nint)(-1), 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE);
                break;
            case WINDOWMESSAGE.WM_COPYDATA:
                COPYDATASTRUCT copydata = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);
                if (copydata.cbData == 0) return 0;
                switch ((SHELLTRAYMESSAGE)copydata.dwData)
                {
                    case SHELLTRAYMESSAGE.ICONUPDATE:
                        SHELLTRAYICONUPDATEDATA iconData = Marshal.PtrToStructure<SHELLTRAYICONUPDATEDATA>(copydata.lpData);
                        NOTIFYICONDATA nid = iconData.nid;
                        switch((ICONUPDATEACTION)iconData.dwMessage)
                        {
                            case ICONUPDATEACTION.NIM_ADD:
                                trayIconsManager.Add(nid);
                                break;
                            case ICONUPDATEACTION.NIM_MODIFY:
                            case ICONUPDATEACTION.NIM_SETVERSION:
                                //AddNidSafely(nid); 
                                trayIconsManager.Update(nid);
                                break;
                        }
                        // Filter out non overflow icons to build the overflow icons collection
                        //overflowIcons = trayIconsManager.icons.Where(icon => !NON_OVERFLOW_CLASSES.Contains(icon.className)).ToList();
                        Debug.WriteLine($"ICONUPDATEACTION: {(ICONUPDATEACTION)(iconData.dwMessage)}, uid: {nid.uID}, hWnd: {nid.hWnd}, nids: {trayIconsManager.icons.Count}, class: {Utils.GetClassNameFromHWND((nint)nid.hWnd)}, version: {nid.uTimeoutOrVersion.uVersion}, callback: {nid.uCallbackMessage}, hIcon: {nid.hIcon}");
                        //notifiedIcons.ForEach(icon => Debug.WriteLine($"class: {Utils.GetClassNameFromHWND((nint)icon.hWnd)}, exe: {Utils.GetExePathFromHWND((nint)icon.hWnd)}"));
                        //overflowIcons.ForEach(icon => Debug.WriteLine($"class: {icon.className}, exe: {icon.exePath}"));
                        break;

                    // a tray icon's process is querying icon position using Shell_NotifyIconGetRect()
                    // windows can only communicate by returning data through their WndProcs, so
                    // assemble the data here and return it below. Certain tray apps for example glazewm
                    // queries Shell_NotifyIconGetRect() right before displaying its trayicon's context
                    // menu probably to get the icon coordinates for some reason, so handling it is imoportant
                    case SHELLTRAYMESSAGE.TRAYICONPOSITION:
                        _NOTIFYICONIDENTIFIERINTERNAL iconIdentifier = Marshal.PtrToStructure<_NOTIFYICONIDENTIFIERINTERNAL>(copydata.lpData);
                        User32.GetCursorPos(out POINT cursorPos);
                        switch(iconIdentifier.msg)
                        {
                            // request for left, top positions of the icon rect, case 1
                            case 1:
                                return Utils.MAKELPARAM((short)cursorPos.X, (short)cursorPos.Y);
                            // request for the right, bottom of the icon rect
                            case 2:
                                return Utils.MAKELPARAM((short)(cursorPos.X + 1), (short)(cursorPos.Y - 1));
                        }
                        return 0;
                }
                break;
            default:
                User32.SendMessage(originalTray_hWnd, (uint)uMsg, wParam, lParam);
                return User32.DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
        return 0;
    }

    public void RefreshTaskbar()
    {
        uint taskbarCreatedMsg = User32.RegisterWindowMessage("TaskbarCreated");
        // 0xffff := HWND_BROADCAST
        User32.SendNotifyMessage((nint)0xffff, taskbarCreatedMsg, 0, 0);
    }

    public List<TrayIcon> GetTrayIcons()
    {
        return trayIconsManager.icons.Where(icon => !NON_OVERFLOW_CLASSES.Contains(icon.className)).ToList();
    }


    public void Destroy()
    {
        User32.DestroyWindow(hWnd);
        cts.Cancel();
    }
    ~TaskbarInterceptor() 
    {
        Destroy();
    }
}

public class TrayIcon
{
    public NOTIFYICONDATA nid;
    public string? className;
    public string? exePath;
    public uint old_uVersion;
    public BitmapSource icon;

    public TrayIcon(NOTIFYICONDATA nid)
    {
        this.nid = nid;
        this.className = Utils.GetClassNameFromHWND((nint)nid.hWnd);
        this.exePath = Utils.GetExePathFromHWND((nint)nid.hWnd);
        
        // get actual icon from hIcon
        this.icon = Imaging.CreateBitmapSourceFromHIcon((nint)nid.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        this.icon.Freeze();
    }

    public void RightClick()
    {
        NOTIFYICONDATA validatedNid = nid;

        // validate uVersion. If uVersion is exorbitantly large that means that it probably 
        // isnt uVersion but uTimeout
        if(validatedNid.uTimeoutOrVersion.uVersion > 4)
        {
            old_uVersion = nid.uTimeoutOrVersion.uVersion;
            validatedNid.uTimeoutOrVersion.uVersion = 0;
        }

        // check if uCallbackMessage is still invalid, for most user applications it wont be, but
        // for certain system tray apps it still could, if so use uid as callback message
        // eg. SystemTray_Main
        if(
            validatedNid.uCallbackMessage == 0 && 
            !validatedNid.uFlags.ContainsFlag((uint)NOTIFYICONDATAVALIDITY.NIF_MESSAGE)
        )
        {
            validatedNid.uCallbackMessage = nid.uID;
        }

        // handling special classes
        // [ATL:00007FFE197FD000] SecurityHealthSystray.exe
        if(this.className == "ATL:00007FFE197FD000")
        {
            validatedNid.uTimeoutOrVersion.uVersion = 4;
        } 

        //RightClick, class: tray_icon_app, exe: C:\Program Files\glzr.io\GlazeWM\glazewm.exe, hWnd: 3802056, uVersion: 0, callback: 6002 
        Debug.WriteLine($"RightClick, class: {className}, exe: {exePath}, hWnd: {validatedNid.hWnd}, uid: {validatedNid.uID}, uVersion: {validatedNid.uTimeoutOrVersion.uVersion}, callback: {validatedNid.uCallbackMessage}, callbackValid: {(validatedNid.uFlags & 0x00000001) != 0},  old_uid: {old_uVersion}");
        Sambar.api.ImpersonateTrayEvent(validatedNid, ICONACTION.RIGHT_CLICK);
    }
}

/// <summary>
/// Adds nids to notifiedIcons so that it doesnt contain repeated elements and newer nids 
/// of already present tray icons when recieved are only added while removing the older one. 
/// However additional care has to be taken while adding newer nids because certain apps such
/// as Taskmgr for example wont have a valid uCallbackMessage when sending subsequent nids through
/// NIM_MODIFY, so we dont want to add the nid directly as then we would lose our uCallbackMessage
/// </summary>
public class TrayIconsManager 
{
    public List<TrayIcon> icons = new();
    public TrayIconsManager()
    {
    }

    public void Add(NOTIFYICONDATA nid)
    {
        var indexedIcons = icons.Index().ToList();
        var repIndexedIcons = indexedIcons.Where(indexedIcon => indexedIcon.Item.nid.hWnd == nid.hWnd);
        if(repIndexedIcons.Count() == 0)
        {
            icons.Add(new(nid));
            return;
        }
        Debug.WriteLine($"icon already exists, use Update() to modify");
    }
    public void Update(NOTIFYICONDATA nid)
    {
        var indexedIcons = icons.Index().ToList();
        var repIndexedIcons = indexedIcons.Where(indexedIcon => indexedIcon.Item.nid.hWnd == nid.hWnd);
        if(repIndexedIcons.Count() > 0)
        {
            // if new nids contain invalid uCallbackMessage replace them with the old ones
            if (nid.uCallbackMessage == 0)
            {
                nid.uCallbackMessage = repIndexedIcons.First().Item.nid.uCallbackMessage;
            }
            // if new nids contain invalid hIcon replace them with old ones
            if(!nid.uFlags.ContainsFlag((int)NOTIFYICONDATAVALIDITY.NIF_ICON))
            {
                nid.hIcon = repIndexedIcons.First().Item.nid.hIcon;
            }

            icons[repIndexedIcons.First().Index] = new(nid);
            return;
        }
        Debug.WriteLine($"icon does not exist, use Add() instead");

    }
    public void Delete()
    {

    }
}

public enum ICONACTION 
{
    RIGHT_CLICK
}


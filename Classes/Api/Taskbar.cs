using System.Runtime.InteropServices;
using System.Diagnostics;

namespace sambar;

public partial class Api
{
    public void TaskbarInit()
    {
        TaskbarInterceptor interceptor = new();
    }
}
/// <summary>
/// An invisible window to intercept taskbar messages.
/// We create a window with the preexisting Taskbar class
/// "Shell_TrayWnd". Any messages intended for the real taskbar
/// will be captured by our invisible window
/// </summary>
public class TaskbarInterceptor
{
    nint hWnd;
    CancellationTokenSource cts;
    List<NOTIFYICONDATA> notifiedIcons = new();
    public TaskbarInterceptor() 
    { 
        cts = new();
        Task.Run(() =>
        {
            WNDCLASSEX wc = new();
            wc.cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>();
            wc.lpfnWndProc = WndProc;
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

            if (User32.SetWindowPos(hWnd, (nint)(-1), 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE) == 0)
            {
                Debug.WriteLine($"SetWindowPos() failed: {Marshal.GetLastWin32Error()}");
            }

            RefreshTaskbar();
            
            while (User32.GetMessage(out MSG msg, hWnd, 0, 0) > 0)
            {
                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }
        }, cts.Token);
    }

    nint WndProc(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam)
    {
        //Debug.WriteLine($"Message: {uMsg}");
        switch (uMsg)
        {
            case WINDOWMESSAGE.WM_CLOSE:
                User32.DestroyWindow(hWnd);
                return 0;
            case WINDOWMESSAGE.WM_DESTROY:
                User32.PostQuitMessage(0);
                return 0;
            case WINDOWMESSAGE.WM_COPYDATA:
                COPYDATASTRUCT copydata = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);
                if (copydata.cbData == 0) return 0;
                switch ((SHELLTRAYMESSAGE)copydata.dwData)
                {
                    case SHELLTRAYMESSAGE.ICONUPDATE:
                        SHELLTRAYDATA shellTrayData = Marshal.PtrToStructure<SHELLTRAYDATA>(copydata.lpData);
                        NOTIFYICONDATA nid = shellTrayData.nid;
                        if (notifiedIcons.Where(icon => icon.hWnd == nid.hWnd).Count() == 0) notifiedIcons.Add(nid);
                        Debug.WriteLine($"uid: {nid.uID}, hWnd: {nid.hWnd}, nids: {notifiedIcons.Count}");
                        notifiedIcons.ForEach(icon => Debug.WriteLine($"class: {Utils.GetClassNameFromHWND((nint)icon.hWnd)}, exe: {Utils.GetExePathFromHWND((nint)icon.hWnd)}"));
                        break;
                }
                return 0;
            default:
                return User32.DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }

    public void RefreshTaskbar()
    {
        uint taskbarCreatedMsg = User32.RegisterWindowMessage("TaskbarCreated");
        // 0xffff := HWND_BROADCAST
        User32.SendNotifyMessage((nint)0xffff, taskbarCreatedMsg, 0, 0);
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

using System.Runtime.InteropServices;
using System.Text;

namespace sambar;

public class User32
{
	// user32
	[DllImport("user32.dll", SetLastError = true)]
	public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetSystemMetrics(int nIndex);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern nint FindWindow(string className, string windowName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int ShowWindow(nint hWnd, SHOWWINDOW nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindowEx(nint hWndParent, nint hWndChildAfter, string className, string windowName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int ShowWindowAsync(nint hWnd, SHOWWINDOW nCmdShow);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowPlacement(nint hWnd, ref WINDOWPLACEMENT lpwndpl);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetCursorPos(out POINT cursorPos);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowRect(nint hWnd, out RECT windowRect);
	
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetClientRect(nint hWnd, out RECT clientRect);

	[DllImport("user32.dll")]
    public static extern nint GetForegroundWindow();

	[DllImport("user32.dll")]
	public static extern nint WindowFromPoint(POINT Point);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool EnumWindows(EnumWindowProc enumWindowProc, nint lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool EnumChildWindows(nint hWndParent, EnumWindowProc enumWindowProc, nint lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowThreadProcessId(nint hWnd, out int processId);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CreateWindowEx(
        WINDOWSTYLE dwExStyle,
        string lpClassName,
        string lpWindowName,
        WINDOWSTYLE dwStyle,
        int X,
        int Y,
        int nWidth,
        int nHeight,
        nint hWndParent,
        nint hMenu,
        nint hInstance,
        nint lpParam
    );

	[DllImport("user32.dll", SetLastError = true)]
	public static extern ushort RegisterClassEx(ref WNDCLASSEX wc);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool UnregisterClass(string className, nint hInstance);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint GetModuleHandle(string moduleName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint DefWindowProc(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int DestroyWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetMessage(out MSG msg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool TranslateMessage(ref MSG msg);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool DispatchMessage(ref MSG msg);

    [DllImport("user32.dll", SetLastError = true)]
    static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint RegisterWindowMessage(string message);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int SendNotifyMessage(
        nint hWnd,
        uint msg,
        nint wParam,
        nint lParam
    );

	[DllImport("user32.dll", SetLastError = true)]
	public static extern void SetTimer(nint hWnd, nint nIdEvent, uint uElapse, TIMERPROC timerProc);

}

public class Shell32
{
    [DllImport("shell32.dll", SetLastError = true)]
    public static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

	[DllImport("shell32.dll", SetLastError = true)]
	public static extern long Shell_NotifyIconGetRect(ref _NOTIFYICONIDENTIFIER identifier, out RECT iconLocation); 
}

public class Kernel32
{
    [DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AttachConsole(int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern nint GetModuleHandle(string moduleName);
}

public class Dwmapi
{
    [DllImport("dwmapi.dll", SetLastError = true)]
	public static extern int DwmSetWindowAttribute(nint hWnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);
}


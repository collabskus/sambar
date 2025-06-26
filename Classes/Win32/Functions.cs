using System.Runtime.InteropServices;
using System.Text;

namespace sambar;

public class User32
{
	// user32
	[DllImport("user32.dll", SetLastError = true)]
	public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetSystemMetrics(int nIndex);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindow(string className, string windowName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int ShowWindow(IntPtr hWnd, SHOWWINDOW nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string className, string windowName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int ShowWindowAsync(IntPtr hWnd, SHOWWINDOW nCmdShow);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetCursorPos(out POINT cursorPos);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowRect(IntPtr hWnd, out RECT windowRect);
	
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetClientRect(IntPtr hWnd, out RECT clientRect);

	[DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	public static extern IntPtr WindowFromPoint(POINT Point);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool EnumWindows(EnumWindowProc enumWindowProc, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool EnumChildWindows(nint hWndParent, EnumWindowProc enumWindowProc, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern ushort RegisterClassEx(WNDCLASSEX wc);

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
}

public class Dwmapi
{
    [DllImport("dwmapi.dll", SetLastError = true)]
	public static extern int DwmSetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);
}


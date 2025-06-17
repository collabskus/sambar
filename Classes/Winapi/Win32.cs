using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;

namespace sambar;


public class Win32
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

	// shell32
    [DllImport("shell32.dll")]
    public static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

	// kernel32
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AttachConsole(int processId);

	// dwmapi
	[DllImport("dwmapi.dll", SetLastError = true)]
	public static extern int DwmSetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);
}

public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);

public enum WindowStyles : uint
{
	WS_OVERLAPPED = 0x00000000,
	WS_POPUP = 0x80000000,
	WS_CHILD = 0x40000000,
	WS_MINIMIZE = 0x20000000,
	WS_VISIBLE = 0x10000000,
	WS_DISABLED = 0x08000000,
	WS_CLIPSIBLINGS = 0x04000000,
	WS_CLIPCHILDREN = 0x02000000,
	WS_MAXIMIZE = 0x01000000,
	WS_BORDER = 0x00800000,
	WS_DLGFRAME = 0x00400000,
	WS_VSCROLL = 0x00200000,
	WS_HSCROLL = 0x00100000,
	WS_SYSMENU = 0x00080000,
	WS_THICKFRAME = 0x00040000,
	WS_GROUP = 0x00020000,
	WS_TABSTOP = 0x00010000,

	WS_MINIMIZEBOX = 0x00020000,
	WS_MAXIMIZEBOX = 0x00010000,

	WS_CAPTION = WS_BORDER | WS_DLGFRAME,
	WS_TILED = WS_OVERLAPPED,
	WS_ICONIC = WS_MINIMIZE,
	WS_SIZEBOX = WS_THICKFRAME,
	WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

	WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
	WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
	WS_CHILDWINDOW = WS_CHILD,

	//Extended Window Styles

	WS_EX_DLGMODALFRAME = 0x00000001,
	WS_EX_NOPARENTNOTIFY = 0x00000004,
	WS_EX_TOPMOST = 0x00000008,
	WS_EX_ACCEPTFILES = 0x00000010,
	WS_EX_TRANSPARENT = 0x00000020,

	//#if(WINVER >= 0x0400)

	WS_EX_MDICHILD = 0x00000040,
	WS_EX_TOOLWINDOW = 0x00000080,
	WS_EX_WINDOWEDGE = 0x00000100,
	WS_EX_CLIENTEDGE = 0x00000200,
	WS_EX_CONTEXTHELP = 0x00000400,

	WS_EX_RIGHT = 0x00001000,
	WS_EX_LEFT = 0x00000000,
	WS_EX_RTLREADING = 0x00002000,
	WS_EX_LTRREADING = 0x00000000,
	WS_EX_LEFTSCROLLBAR = 0x00004000,
	WS_EX_RIGHTSCROLLBAR = 0x00000000,

	WS_EX_CONTROLPARENT = 0x00010000,
	WS_EX_STATICEDGE = 0x00020000,
	WS_EX_APPWINDOW = 0x00040000,

	WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
	WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),

	//#endif /* WINVER >= 0x0400 */

	//#if(WIN32WINNT >= 0x0500)

	WS_EX_LAYERED = 0x00080000,

	//#endif /* WIN32WINNT >= 0x0500 */

	//#if(WINVER >= 0x0500)

	WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
	WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring

	//#endif /* WINVER >= 0x0500 */

	//#if(WIN32WINNT >= 0x0500)

	WS_EX_COMPOSITED = 0x02000000,
	WS_EX_NOACTIVATE = 0x08000000

	//#endif /* WIN32WINNT >= 0x0500 */
}

public enum DWMWINDOWATTRIBUTE : uint
{
	DWMWA_NCRENDERING_ENABLED,
	DWMWA_NCRENDERING_POLICY,
	DWMWA_TRANSITIONS_FORCEDISABLED,
	DWMWA_ALLOW_NCPAINT,
	DWMWA_CAPTION_BUTTON_BOUNDS,
	DWMWA_NONCLIENT_RTL_LAYOUT,
	DWMWA_FORCE_ICONIC_REPRESENTATION,
	DWMWA_FLIP3D_POLICY,
	DWMWA_EXTENDED_FRAME_BOUNDS,
	DWMWA_HAS_ICONIC_BITMAP,
	DWMWA_DISALLOW_PEEK,
	DWMWA_EXCLUDED_FROM_PEEK,
	DWMWA_CLOAK,
	DWMWA_CLOAKED,
	DWMWA_FREEZE_REPRESENTATION,
	DWMWA_PASSIVE_UPDATE_MODE,
	DWMWA_USE_HOSTBACKDROPBRUSH,
	DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
	DWMWA_WINDOW_CORNER_PREFERENCE = 33,
	DWMWA_BORDER_COLOR,
	DWMWA_CAPTION_COLOR,
	DWMWA_TEXT_COLOR,
	DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
	DWMWA_SYSTEMBACKDROP_TYPE,
	DWMWA_LAST
}

public enum DWM_WINDOW_CORNER_PREFERENCE
{
	DWMWCP_DEFAULT = 0,
	DWMWCP_DONOTROUND = 1,
	DWMWCP_ROUND = 2,
	DWMWCP_ROUNDSMALL = 3
}

public enum AppBarMessages
{
    New              = 0x00,
    Remove           = 0x01,
    QueryPos         = 0x02,
    SetPos           = 0x03,
    GetState         = 0x04,
    GetTaskBarPos    = 0x05,
    Activate         = 0x06,
    GetAutoHideBar   = 0x07,
    SetAutoHideBar   = 0x08,
    WindowPosChanged = 0x09,
    SetState         = 0x0a
}

[StructLayout(LayoutKind.Sequential)]
public struct APPBARDATA
{
    public uint cbSize;
    public IntPtr hWnd;
    public uint uCallbackMessage;
    public uint uEdge;
    public Rectangle rc;
    public uint lParam;
}

public enum AppBarStates
{
    AutoHide    = 0x01,
    AlwaysOnTop = 0x02
}

public enum SHOWWINDOW 
{
	SW_HIDE = 0,
	SW_SHOWNORMAL = 1,
	SW_MAXIMIZE = 3,
	SW_SHOW = 5
}

[StructLayout(LayoutKind.Sequential)]
public struct POINT 
{
	public int X;
	public int Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
	public int Left;
	public int Top;
	public int Right;
	public int Bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct WINDOWPLACEMENT
{
	public uint length;
	public uint flags;
	public uint showCmd;
	public POINT ptMinPosition;
	public POINT ptMaxPosition;
	public RECT rcNormalPosition;
	public RECT rcDevice;

}

public enum SETWINDOWPOS : uint
{
	SWP_NOSIZE = 0x0001
}

public enum GETWINDOWLONG : int
{
	GWL_STYLE = -16,
	GWL_EXSTYLE = -20
}

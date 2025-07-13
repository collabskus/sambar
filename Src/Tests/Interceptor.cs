using System.Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind.Sequential)]
public struct WNDCLASSEX
{
	public uint cbSize;
	public uint style;
	public Program.WNDPROC lpfnWndProc;
	public int cbClsExtra;
	public int cbWndExtra;
	public nint hInstance;
	public nint hIcon;
	public nint hCursor;
	public nint hbrBackground;
	public string lpszMenuName;
	public string lpszClassName;
	public nint hIconSm;
}

public enum WINDOWSTYLE : ulong
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


[StructLayout(LayoutKind.Sequential)]
struct POINT
{
	public long X;
	public long Y;
}

struct MSG
{
	public nint hwnd;
	public WINDOWMESSAGE message;
	public nint wParam;
	public nint lParam;
	public ulong time;
	public POINT pt;
	public ulong lPrivate;
}

public class Program
{
	[DllImport("user32.dll", SetLastError = true)]
	static extern ushort RegisterClassEx(ref WNDCLASSEX wc);

	[DllImport("user32.dll", SetLastError = true)]
	static extern bool UnregisterClass(string className, nint hInstance);

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint GetModuleHandle(string moduleName);

	public delegate nint WNDPROC(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam);

	[DllImport("user32.dll", SetLastError = true)]
	static extern nint DefWindowProc(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam);

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
	static extern int ShowWindow(nint hWnd, int nCmdShow);

	[DllImport("user32.dll", SetLastError = true)]
	static extern int SetWindowPos(
		nint hWnd,
		nint hWndAfter,
		int X,
		int Y,
		int cx,
		int cy,
		SETWINDOWPOS uFlags
	);

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


	public delegate void TIMERPROC(nint hWnd, uint param2, nint param3, ulong param4);

	[DllImport("user32.dll", SetLastError = true)]
	static extern void SetTimer(nint hWnd, nint nIdEvent, uint uElapse, TIMERPROC timerProc);

	[DllImport("user32.dll", SetLastError = true)]
	static extern void GetClassName(nint hWnd, StringBuilder className, int maxCount);

	static string _GetClassName(nint hWnd)
	{
		StringBuilder builder = new(256);
		GetClassName(hWnd, builder, builder.Capacity);
		return builder.ToString();
	}

	static nint WndProc(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam)
	{
		Console.WriteLine($"Message: {uMsg}");
		switch (uMsg)
		{
			case WINDOWMESSAGE.WM_CLOSE:
				DestroyWindow(hWnd);
				return 0;
			case WINDOWMESSAGE.WM_DESTROY:
				PostQuitMessage(0);
				return 0;
			case WINDOWMESSAGE.WM_TIMER:
				SetWindowPos(hWnd, (nint)(-1), 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE);
				return 0;
			case WINDOWMESSAGE.WM_COPYDATA:
				COPYDATASTRUCT copydata = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);

				if (copydata.cbData == 0) return 0;

				SHELLTRAYDATA shellTrayData = Marshal.PtrToStructure<SHELLTRAYDATA>(copydata.lpData);

				NOTIFYICONDATA notifyIconData = shellTrayData.nid;

				switch ((TASKBARMESSAGE)copydata.dwData)
				{
					case TASKBARMESSAGE.NIM_ADD:
						Console.WriteLine($"Icon add");
						return 0;
					case TASKBARMESSAGE.NIM_MODIFY:
						Console.WriteLine($"Icon modify, {copydata.cbData}");
						Console.WriteLine($"uid: {notifyIconData.uID}, hWnd: {notifyIconData.hWnd}, tip: {notifyIconData.szTip}, tipCount: {notifyIconData.szTip.Count()}, cbSize: {notifyIconData.cbSize}");
						return 0;
				}
				return 0;
			default:
				return DefWindowProc(hWnd, uMsg, wParam, lParam);
		}
	}

	static void Main()
	{
		WNDCLASSEX wc = new();
		wc.cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>();
		wc.lpfnWndProc = WndProc;
		wc.hInstance = GetModuleHandle(null);
		wc.lpszClassName = "Shell_TrayWnd";

		ushort result = RegisterClassEx(ref wc);
		if (result == 0)
		{
			Console.WriteLine($"RegisterClassEx() failed: {Marshal.GetLastWin32Error()}");
		}
		else
		{
			Console.WriteLine($"RegisterClassEx() success !");
		}

		nint hWnd = CreateWindowEx(
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

		//ShowWindow(hWnd, 5);
		if (SetWindowPos(hWnd, (nint)(-1), 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE) == 0)
		{
			Console.WriteLine($"SetWindowPos() failed: {Marshal.GetLastWin32Error()}");
		}

		//SetTimer(hWnd, 1, 100, null);
		uint taskbarCreatedMsg = RegisterWindowMessage("TaskbarCreated");
		SendNotifyMessage((nint)0xffff, taskbarCreatedMsg, 0, 0);

		while (GetMessage(out MSG msg, hWnd, 0, 0) > 0)
		{
			TranslateMessage(ref msg);
			DispatchMessage(ref msg);
		}

		//UnregisterClass(wc.lpszClassName, nint.Zero);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct COPYDATASTRUCT
{
	public ulong dwData;
	public ulong cbData;
	public nint lpData;
}

[StructLayout(LayoutKind.Explicit)]
public struct TimeoutVersionUnion
{
	[FieldOffset(0)]
	public uint uTimeout;
	[FieldOffset(0)]
	public uint uVersion;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct NOTIFYICONDATA
{
	public uint cbSize;
	public uint hWnd;
	public uint uID;
	public uint uFlags;
	public uint uCallbackMessage;
	public uint hIcon;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string szTip;
	public uint dwState;
	public uint dwStateMask;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string szInfo;
	public TimeoutVersionUnion uTimeoutOrVersion;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string szInfoTitle;
	public uint dwInfoFlags;
	public Guid guidItem;
	public uint hBalloonIcon;
}

[StructLayout(LayoutKind.Sequential)]
public struct SHELLTRAYDATA
{
	public int dwHz;
	public uint dwMessage;
	public NOTIFYICONDATA nid;
}

public enum SETWINDOWPOS : uint
{
	SWP_NOSIZE = 0x0001,
    SWP_NOMOVE = 0x0002,
    SWP_NOACTIVATE = 0x0010
}

public enum WINDOWMESSAGE : uint
{

	// Window Message Constants
	WM_ACTIVATE = 0x0006,
	WM_ACTIVATEAPP = 0x001C,
	WM_AFXFIRST = 0x0360,
	WM_AFXLAST = 0x037F,
	WM_APP = 0x8000,
	WM_ASKCBFORMATNAME = 0x030C,
	WM_CANCELJOURNAL = 0x004B,
	WM_CANCELMODE = 0x001F,
	WM_CAPTURECHANGED = 0x0215,
	WM_CHANGECBCHAIN = 0x030D,
	WM_CHANGEUISTATE = 0x0127,
	WM_CHAR = 0x0102,
	WM_CHARTOITEM = 0x002F,
	WM_CHILDACTIVATE = 0x0022,
	WM_CLEAR = 0x0303,
	WM_CLOSE = 0x0010,
	WM_COMMAND = 0x0111,
	WM_COMPACTING = 0x0041,
	WM_COMPAREITEM = 0x0039,
	WM_CONTEXTMENU = 0x007B,
	WM_COPY = 0x0301,
	WM_COPYDATA = 0x004A,
	WM_CREATE = 0x0001,
	WM_CTLCOLORBTN = 0x0135,
	WM_CTLCOLORDLG = 0x0136,
	WM_CTLCOLOREDIT = 0x0133,
	WM_CTLCOLORLISTBOX = 0x0134,
	WM_CTLCOLORMSGBOX = 0x0132,
	WM_CTLCOLORSCROLLBAR = 0x0137,
	WM_CTLCOLORSTATIC = 0x0138,
	WM_CUT = 0x0300,
	WM_DEADCHAR = 0x0103,
	WM_DELETEITEM = 0x002D,
	WM_DESTROY = 0x0002,
	WM_DESTROYCLIPBOARD = 0x0307,
	WM_DEVICECHANGE = 0x0219,
	WM_DEVMODECHANGE = 0x001B,
	WM_DISPLAYCHANGE = 0x007E,
	WM_DRAWCLIPBOARD = 0x0308,
	WM_DRAWITEM = 0x002B,
	WM_DROPFILES = 0x0233,
	WM_ENABLE = 0x000A,
	WM_ENDSESSION = 0x0016,
	WM_ENTERIDLE = 0x0121,
	WM_ENTERMENULOOP = 0x0211,
	WM_ENTERSIZEMOVE = 0x0231,
	WM_ERASEBKGND = 0x0014,
	WM_EXITMENULOOP = 0x0212,
	WM_EXITSIZEMOVE = 0x0232,
	WM_FONTCHANGE = 0x001D,
	WM_GETDLGCODE = 0x0087,
	WM_GETFONT = 0x0031,
	WM_GETHOTKEY = 0x0033,
	WM_GETICON = 0x007F,
	WM_GETMINMAXINFO = 0x0024,
	WM_GETOBJECT = 0x003D,
	WM_GETTEXT = 0x000D,
	WM_GETTEXTLENGTH = 0x000E,
	WM_HANDHELDFIRST = 0x0358,
	WM_HANDHELDLAST = 0x035F,
	WM_HELP = 0x0053,
	WM_HOTKEY = 0x0312,
	WM_HSCROLL = 0x0114,
	WM_HSCROLLCLIPBOARD = 0x030E,
	WM_ICONERASEBKGND = 0x0027,
	WM_IME_CHAR = 0x0286,
	WM_IME_COMPOSITION = 0x010F,
	WM_IME_COMPOSITIONFULL = 0x0284,
	WM_IME_CONTROL = 0x0283,
	WM_IME_ENDCOMPOSITION = 0x010E,
	WM_IME_KEYDOWN = 0x0290,
	WM_IME_KEYLAST = 0x010F,
	WM_IME_KEYUP = 0x0291,
	WM_IME_NOTIFY = 0x0282,
	WM_IME_REQUEST = 0x0288,
	WM_IME_SELECT = 0x0285,
	WM_IME_SETCONTEXT = 0x0281,
	WM_IME_STARTCOMPOSITION = 0x010D,
	WM_INITDIALOG = 0x0110,
	WM_INITMENU = 0x0116,
	WM_INITMENUPOPUP = 0x0117,
	WM_INPUTLANGCHANGE = 0x0051,
	WM_INPUTLANGCHANGEREQUEST = 0x0050,
	WM_KEYDOWN = 0x0100,
	WM_KEYFIRST = 0x0100,
	WM_KEYLAST = 0x0108,
	WM_KEYUP = 0x0101,
	WM_KILLFOCUS = 0x0008,
	WM_LBUTTONDBLCLK = 0x0203,
	WM_LBUTTONDOWN = 0x0201,
	WM_LBUTTONUP = 0x0202,
	WM_MBUTTONDBLCLK = 0x0209,
	WM_MBUTTONDOWN = 0x0207,
	WM_MBUTTONUP = 0x0208,
	WM_MDIACTIVATE = 0x0222,
	WM_MDICASCADE = 0x0227,
	WM_MDICREATE = 0x0220,
	WM_MDIDESTROY = 0x0221,
	WM_MDIGETACTIVE = 0x0229,
	WM_MDIICONARRANGE = 0x0228,
	WM_MDIMAXIMIZE = 0x0225,
	WM_MDINEXT = 0x0224,
	WM_MDIREFRESHMENU = 0x0234,
	WM_MDIRESTORE = 0x0223,
	WM_MDISETMENU = 0x0230,
	WM_MDITILE = 0x0226,
	WM_MEASUREITEM = 0x002C,
	WM_MENUCHAR = 0x0120,
	WM_MENUCOMMAND = 0x0126,
	WM_MENUDRAG = 0x0123,
	WM_MENUGETOBJECT = 0x0124,
	WM_MENURBUTTONUP = 0x0122,
	WM_MENUSELECT = 0x011F,
	WM_MOUSEACTIVATE = 0x0021,
	WM_MOUSEFIRST = 0x0200,
	WM_MOUSEHOVER = 0x02A1,
	WM_MOUSELAST = 0x020D,
	WM_MOUSELEAVE = 0x02A3,
	WM_MOUSEMOVE = 0x0200,
	WM_MOUSEWHEEL = 0x020A,
	WM_MOUSEHWHEEL = 0x020E,
	WM_MOVE = 0x0003,
	WM_MOVING = 0x0216,
	WM_NCACTIVATE = 0x0086,
	WM_NCCALCSIZE = 0x0083,
	WM_NCCREATE = 0x0081,
	WM_NCDESTROY = 0x0082,
	WM_NCHITTEST = 0x0084,
	WM_NCLBUTTONDBLCLK = 0x00A3,
	WM_NCLBUTTONDOWN = 0x00A1,
	WM_NCLBUTTONUP = 0x00A2,
	WM_NCMBUTTONDBLCLK = 0x00A9,
	WM_NCMBUTTONDOWN = 0x00A7,
	WM_NCMBUTTONUP = 0x00A8,
	WM_NCMOUSEHOVER = 0x02A0,
	WM_NCMOUSELEAVE = 0x02A2,
	WM_NCMOUSEMOVE = 0x00A0,
	WM_NCPAINT = 0x0085,
	WM_NCRBUTTONDBLCLK = 0x00A6,
	WM_NCRBUTTONDOWN = 0x00A4,
	WM_NCRBUTTONUP = 0x00A5,
	WM_NCXBUTTONDBLCLK = 0x00AD,
	WM_NCXBUTTONDOWN = 0x00AB,
	WM_NCXBUTTONUP = 0x00AC,
	WM_NCUAHDRAWCAPTION = 0x00AE,
	WM_NCUAHDRAWFRAME = 0x00AF,
	WM_NEXTDLGCTL = 0x0028,
	WM_NEXTMENU = 0x0213,
	WM_NOTIFY = 0x004E,
	WM_NOTIFYFORMAT = 0x0055,
	WM_NULL = 0x0000,
	WM_PAINT = 0x000F,
	WM_PAINTCLIPBOARD = 0x0309,
	WM_PAINTICON = 0x0026,
	WM_PALETTECHANGED = 0x0311,
	WM_PALETTEISCHANGING = 0x0310,
	WM_PARENTNOTIFY = 0x0210,
	WM_PASTE = 0x0302,
	WM_PENWINFIRST = 0x0380,
	WM_PENWINLAST = 0x038F,
	WM_POWER = 0x0048,
	WM_POWERBROADCAST = 0x0218,
	WM_PRINT = 0x0317,
	WM_PRINTCLIENT = 0x0318,
	WM_QUERYDRAGICON = 0x0037,
	WM_QUERYENDSESSION = 0x0011,
	WM_QUERYNEWPALETTE = 0x030F,
	WM_QUERYOPEN = 0x0013,
	WM_QUEUESYNC = 0x0023,
	WM_QUIT = 0x0012,
	WM_RBUTTONDBLCLK = 0x0206,
	WM_RBUTTONDOWN = 0x0204,
	WM_RBUTTONUP = 0x0205,
	WM_RENDERALLFORMATS = 0x0306,
	WM_RENDERFORMAT = 0x0305,
	WM_SETCURSOR = 0x0020,
	WM_SETFOCUS = 0x0007,
	WM_SETFONT = 0x0030,
	WM_SETHOTKEY = 0x0032,
	WM_SETICON = 0x0080,
	WM_SETREDRAW = 0x000B,
	WM_SETTEXT = 0x000C,
	WM_SETTINGCHANGE = 0x001A,
	WM_SHOWWINDOW = 0x0018,
	WM_SIZE = 0x0005,
	WM_SIZECLIPBOARD = 0x030B,
	WM_SIZING = 0x0214,
	WM_SPOOLERSTATUS = 0x002A,
	WM_STYLECHANGED = 0x007D,
	WM_STYLECHANGING = 0x007C,
	WM_SYNCPAINT = 0x0088,
	WM_SYSCHAR = 0x0106,
	WM_SYSCOLORCHANGE = 0x0015,
	WM_SYSCOMMAND = 0x0112,
	WM_SYSDEADCHAR = 0x0107,
	WM_SYSKEYDOWN = 0x0104,
	WM_SYSKEYUP = 0x0105,
	WM_TCARD = 0x0052,
	WM_TIMECHANGE = 0x001E,
	WM_TIMER = 0x0113,
	WM_UNDO = 0x0304,
	WM_UNINITMENUPOPUP = 0x0125,
	WM_USER = 0x0400,
	WM_USERCHANGED = 0x0054,
	WM_VKEYTOITEM = 0x002E,
	WM_VSCROLL = 0x0115,
	WM_VSCROLLCLIPBOARD = 0x030A,
	WM_WINDOWPOSCHANGED = 0x0047,
	WM_WINDOWPOSCHANGING = 0x0046,
	WM_WININICHANGE = 0x001A,
	WM_XBUTTONDBLCLK = 0x020D,
	WM_XBUTTONDOWN = 0x020B,
	WM_XBUTTONUP = 0x020C,
}

public enum TASKBARMESSAGE : int
{
	NIM_ADD = 0x00000000,
	NIM_MODIFY = 0x00000001,
	NIM_DELETE = 0x00000002,
	NIM_SETFOCUS = 0x00000003,
	NIM_SETVERSION = 0x00000004
}

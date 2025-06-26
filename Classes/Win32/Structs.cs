using System.Runtime.InteropServices;
using System.Drawing;

namespace sambar;

/// <summary>
/// DWORD := uint
/// HWND  := nint
/// PVOID := nint
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct APPBARDATA
{
    public uint cbSize;
    public nint hWnd;
    public uint uCallbackMessage;
    public uint uEdge;
    public Rectangle rc;
    public uint lParam;
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

[StructLayout(LayoutKind.Sequential)]
public struct _NOTIFYICONIDENTIFIER {
	public uint cbSize;
	public nint hWnd;
	public uint UID;
	public Guid guidItem;
}

[StructLayout(LayoutKind.Sequential)]
public struct WNDCLASSEX
{
	public uint cbSize;
	public uint style;
	public WNDPROC lpfnWndProc;
	public int cbClsExtra;
	public int cbWndExtra;
	public nint hInstance;
	public nint hIcon;
	public nint hCurosr;
	public nint hbrBackground;
	public string lpszMenuName;
	public string lpszClassName;
	public nint hIconSm;
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

[StructLayout(LayoutKind.Sequential)]
public struct MSG
{
	public nint hwnd;
	public WINDOWMESSAGE message;
	public nint wParam;
	public nint lParam;
	public uint time;
	public POINT pt;
	public uint lPrivate;
}


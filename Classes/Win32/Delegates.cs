namespace sambar;

public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);
public delegate nint WNDPROC(nint hWnd, uint uMsg, nint wParam, nint lParam);
public delegate void TIMERPROC(nint hWnd, uint param2, nint param3, ulong param4);



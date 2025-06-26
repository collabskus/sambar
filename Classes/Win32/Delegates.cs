namespace sambar;

public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);
public delegate nint WNDPROC(nint hWnd, uint uMsg, nint wParam, nint lParam);


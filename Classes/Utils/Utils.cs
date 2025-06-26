using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpVectors.Dom.Stylesheets;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace sambar;

public partial class Utils
{
   	public static Brush BrushFromHex(string hexColorString) {
		if (hexColorString == "transparent") {
			return new SolidColorBrush(Colors.Transparent);	
		}
		System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hexColorString);
		return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
	} 

	public static List<string> GetStyleListFromUInt(uint styleUInt)
	{
		WINDOWSTYLE styles = (WINDOWSTYLE)styleUInt;
		List<string> styleList = new();
		foreach(WINDOWSTYLE style in Enum.GetValues(typeof(WINDOWSTYLE)))
		{
			if(styles.HasFlag(style))
			{
				styleList.Add(style.ToString());
			}
		}
		return styleList;
	}

	public static List<string> GetStylesFromHwnd(IntPtr hWnd)
	{
		uint stylesUInt = User32.GetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_STYLE);
		//Debug.WriteLine($"GetStylesFromHwnd(): {Marshal.GetLastWin32Error()}");
		return GetStyleListFromUInt(stylesUInt);
	}

	public static bool IsContextMenu(IntPtr hWnd)
	{
		var styleList = Utils.GetStylesFromHwnd(hWnd);
		//Debug.WriteLine($"IsContextMenu(): {Marshal.GetLastWin32Error()}");
        if (styleList.Contains("WS_POPUP")) return true;
        
        string className = Utils.GetClassNameFromHWND(hWnd);
        if (className == "#32768") return true;
        if (className == "#32770") return true;
        if (className == "SysListView32") return true;
        if (className == "SysShadow") return true;
        if (className == "TrayiconMessageWindow") return true;
        if (className == "tray_icon_app") return true;
        return false;
	}

	public static bool IsWindowVisible(IntPtr hWnd)
	{
		var styleList = Utils.GetStylesFromHwnd(hWnd);
		//Debug.WriteLine($"IsWindowVisible(): {Marshal.GetLastWin32Error()}");
        if (styleList.Contains("WS_VISIBLE")) return true;
        return false;
	}

	public static IntPtr GetWindowUnderCursor()
	{
		User32.GetCursorPos(out POINT pt);
		return User32.WindowFromPoint(pt);
	}

	public static void MoveWindowToCursor(IntPtr hWnd, int offsetX = 0, int offsetY = 0)
	{
		User32.GetCursorPos(out POINT cursorPos);
		User32.SetWindowPos(hWnd, IntPtr.Zero, cursorPos.X + offsetX, cursorPos.Y + offsetY, 0, 0, SETWINDOWPOS.SWP_NOSIZE);
	}

	public static void MoveWindow(IntPtr hWnd, int x, int y)
	{
		User32.SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SETWINDOWPOS.SWP_NOSIZE);
	}

	public static string GetClassNameFromHWND(IntPtr hWnd)
	{
		StringBuilder str = new(256);
		User32.GetClassName(hWnd, str, str.Capacity);
		return str.ToString();
	}

	public static (int, int) GetWindowDimensions(IntPtr hWnd)
	{
		User32.GetWindowRect(hWnd, out RECT rect);
		int Width = rect.Right - rect.Left;
		int Height = rect.Bottom - rect.Top;
		return (Width, Height);
	}

    public static IntPtr GetHWNDFromPID(int processId)
    {
        IntPtr found_hWnd = new();
        EnumWindowProc enumWindowProc = (IntPtr hWnd, IntPtr lParam) => {
            User32.GetWindowThreadProcessId(hWnd, out int _processId);	
            if(_processId == processId) {
                found_hWnd = hWnd;	
                return false;
            }
            return true;
        };
        User32.EnumWindows(enumWindowProc, (IntPtr)processId);
        return found_hWnd;
    }

    public static List<GUIProcess> EnumWindowProcesses()
    {
        List<GUIProcess> guiProcesses = new();
        EnumWindowProc enumWindowProc = (nint hWnd, nint lParam) =>
        {
            User32.GetWindowThreadProcessId(hWnd, out int processId);
            Process process = Process.GetProcessById(processId);
            GUIProcess guiProcess;
            if((guiProcess = guiProcesses.Where(_p => _p.name == process.ProcessName).FirstOrDefault()) == null) {
                guiProcess = new() { name = process.ProcessName };
                guiProcesses.Add(guiProcess);
            }
            guiProcess.process = process;
            _Window window = new();
            window.hWnd = hWnd;
            window.className = GetClassNameFromHWND(hWnd);
            guiProcess.windows.Add(window);
            EnumWindowProc enumChildWindowProc = (nint c_hWnd, nint lParam) =>
            {
                _Window c_window = new();
                c_window.hWnd = c_hWnd;
                c_window.className = GetClassNameFromHWND(c_hWnd);
                guiProcess.windows.Add(c_window);
                return true;
            };
            User32.EnumChildWindows(hWnd, enumChildWindowProc, nint.Zero);
            return true;
        };
        User32.EnumWindows(enumWindowProc, nint.Zero);
        return guiProcesses;
    }	
}

public class _Window
{
	public string name;
	public string className;
	public nint hWnd;
}

public class GUIProcess
{
	public string name;
	public Process process;
	public List<_Window> windows = new();
}


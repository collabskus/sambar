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
		WindowStyles styles = (WindowStyles)styleUInt;
		List<string> styleList = new();
		foreach(WindowStyles style in Enum.GetValues(typeof(WindowStyles)))
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
		uint stylesUInt = Win32.GetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_STYLE);
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
		Win32.GetCursorPos(out POINT pt);
		return Win32.WindowFromPoint(pt);
	}

	public static void MoveWindowToCursor(IntPtr hWnd, int offsetX = 0, int offsetY = 0)
	{
		Win32.GetCursorPos(out POINT cursorPos);
		Win32.SetWindowPos(hWnd, IntPtr.Zero, cursorPos.X + offsetX, cursorPos.Y + offsetY, 0, 0, (uint)SETWINDOWPOS.SWP_NOSIZE);
	}

	public static void MoveWindow(IntPtr hWnd, int x, int y)
	{
		Win32.SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, (uint)SETWINDOWPOS.SWP_NOSIZE);
	}

	public static string GetClassNameFromHWND(IntPtr hWnd)
	{
		StringBuilder str = new(256);
		Win32.GetClassName(hWnd, str, str.Capacity);
		return str.ToString();
	}

	public static (int, int) GetWindowDimensions(IntPtr hWnd)
	{
		Win32.GetWindowRect(hWnd, out RECT rect);
		int Width = rect.Right - rect.Left;
		int Height = rect.Bottom - rect.Top;
		return (Width, Height);
	}
}

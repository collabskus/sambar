using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpVectors.Dom.Stylesheets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;
using System.IO;

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

	public static List<string> GetStylesFromHwnd(nint hWnd)
	{
		uint stylesUInt = User32.GetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_STYLE);
		//Debug.WriteLine($"GetStylesFromHwnd(): {Marshal.GetLastWin32Error()}");
		return GetStyleListFromUInt(stylesUInt);
	}

	public static bool IsContextMenu(nint hWnd)
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

	public static bool IsWindowVisible(nint hWnd)
	{
		var styleList = Utils.GetStylesFromHwnd(hWnd);
		//Debug.WriteLine($"IsWindowVisible(): {Marshal.GetLastWin32Error()}");
        if (styleList.Contains("WS_VISIBLE")) return true;
        return false;
	}

	public static nint GetWindowUnderCursor()
	{
		User32.GetCursorPos(out POINT pt);
		return User32.WindowFromPoint(pt);
	}

	public static void MoveWindowToCursor(nint hWnd, int offsetX = 0, int offsetY = 0)
	{
		User32.GetCursorPos(out POINT cursorPos);
		User32.SetWindowPos(hWnd, nint.Zero, cursorPos.X + offsetX, cursorPos.Y + offsetY, 0, 0, SETWINDOWPOS.SWP_NOSIZE);
	}

	public static void MoveWindow(nint hWnd, int x, int y)
	{
		User32.SetWindowPos(hWnd, nint.Zero, x, y, 0, 0, SETWINDOWPOS.SWP_NOSIZE);
	}

	public static string GetClassNameFromHWND(nint hWnd)
	{
		StringBuilder str = new(256);
		User32.GetClassName(hWnd, str, str.Capacity);
		return str.ToString();
	}

	public static (int, int) GetWindowDimensions(nint hWnd)
	{
		User32.GetWindowRect(hWnd, out RECT rect);
		int Width = rect.Right - rect.Left;
		int Height = rect.Bottom - rect.Top;
		return (Width, Height);
	}

    public static nint GetHWNDFromPID(int processId)
    {
        nint found_hWnd = new();
        EnumWindowProc enumWindowProc = (nint hWnd, nint lParam) => {
            User32.GetWindowThreadProcessId(hWnd, out int _processId);	
            if(_processId == processId) {
                found_hWnd = hWnd;	
                return false;
            }
            return true;
        };
        User32.EnumWindows(enumWindowProc, (nint)processId);
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

	public static string? GetExePathFromHWND(nint hWnd)
	{
		User32.GetWindowThreadProcessId(hWnd, out int processId);

		if (Environment.IsPrivilegedProcess)
		{
			List<GUIProcess> allWindows = EnumWindowProcesses();
			Process? process = allWindows.Where(guiProcess => guiProcess.process.Id == processId).FirstOrDefault()?.process;
			return process?.MainModule?.FileName;
		}
		
		/// <summary>
        /// Getting module filenames without elevated privileges
        /// NtQuerySystemInformation() := undocumented internal API
        /// https://stackoverflow.com/a/75084784/14588925
        /// </summary>
		const uint SystemProcessIdInformation = 0x58;
        SYSTEM_PROCESS_ID_INFORMATION info = new() { ProcessId = processId, ImageName = new() { Length = 0, MaximumLength = 256, Buffer = Marshal.AllocHGlobal(512) } };
        int result = Ntdll.NtQuerySystemInformation(SystemProcessIdInformation, ref info, (uint)Marshal.SizeOf<SYSTEM_PROCESS_ID_INFORMATION>(), out uint returnLength);
        string exePath = Marshal.PtrToStringUni(info.ImageName.Buffer);
		Marshal.FreeHGlobal(info.ImageName.Buffer);

        // List all device paths
        List<string> driveDevicePaths = new();
        List<string> driveNames = new();
        Dictionary<string, string> devicePathToDrivePath = new();
        driveNames = DriveInfo.GetDrives().Select(drive => drive.Name.Substring(0, 2)).ToList();
        driveDevicePaths = driveNames.Select(drive => {
            StringBuilder str = new(256);
            Kernel32.QueryDosDevice(drive, str, (uint)str.Capacity);
            string devicePath = str.ToString();
            devicePathToDrivePath[devicePath] = drive;
            return devicePath;
        }).ToList();	

        //
        string exePathDeviceName = driveDevicePaths.Where(path => exePath.Contains(path)).FirstOrDefault();
        string exePathDriveName = devicePathToDrivePath[exePathDeviceName];

        string exeNtPath = Path.Join(exePathDriveName, exePath.Replace(exePathDeviceName, ""));
        return exeNtPath;
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


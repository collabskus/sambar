using System.Diagnostics;

namespace sambar;
public partial class Api
{
	Dictionary<string, IntPtr> preloadedWindows = new();
	public void WindowingInit()
	{
		preloadedWindows = GetAllWindows();
	}
	public Dictionary<string, IntPtr> GetAllWindows()
	{
		Win32.EnumWindows(
			(hWnd, lParam) =>
			{
				preloadedWindows[Utils.GetClassNameFromHWND(hWnd)] = hWnd;
				return true;
			},
			IntPtr.Zero
		);
		return preloadedWindows;
	}

	public bool IsWindowInPreloaded(IntPtr hWnd)
	{
		string className = Utils.GetClassNameFromHWND(hWnd);
		Debug.WriteLine($"windowInPreloaded: {className}");
		return preloadedWindows.ContainsKey(className);
	}

	public bool IsWindowContextMenuOfTray(IntPtr hWnd)
	{
		string className = Utils.GetClassNameFromHWND(hWnd);
		if (Utils.IsContextMenu((hWnd)))
		{
			Debug.WriteLine($"WindowInContextMenuOfTray: {className}, [WS_POPUP]");
			return true;
		}
		if (!preloadedWindows.ContainsKey(className) && className == "#32768")
		{
			Debug.WriteLine($"WindowInContextMenuOfTray(): {className}, [PRELOADED]");
			return true;	
		}
		if (className == "SysListView32")
		{
			Debug.WriteLine($"WindowInContextMenuOfTray(): {className}, [SYSLISTVIEW]");
			return true;		
		}
		if (className == "SysShadow")
		{
			Debug.WriteLine($"WindowInContextMenuOfTray(): {className}, [SYSSHADOW]");
			return true;		
		}
		if (className == "QMenu")
		{
			Debug.WriteLine($"WindowInContextMenuOfTray(): {className}, [Qmenu]");
			return true;				
		}
		return false;
	}
}

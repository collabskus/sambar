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
}

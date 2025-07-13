using System.Runtime.InteropServices;

public partial class Program
{
	const int GA_ROOTOWNER = 3;

	[DllImport("user32.dll", SetLastError = true)]
	extern static nint GetAncestor(
	  nint hwnd,
	  uint gaFlags
	);

	[DllImport("user32.dll", SetLastError = true)]
	extern static nint GetLastActivePopup(
		nint hWnd
	);

	[DllImport("user32.dll", SetLastError = true)]
	extern static bool IsWindowVisible(nint hWnd);

	// https://devblogs.microsoft.com/oldnewthing/20071008-00/?p=24863 
	public static bool IsWindowInTaskbar(nint hWnd)
	{
		// start at the owner window
		nint hWndWalk = GetAncestor(hWnd, GA_ROOTOWNER);

		nint hWndTry;
		// a window in taskbar / alt-tab is its own last popup window, so loop until hWnd walk becomes a popup window
		while ((hWndTry = GetLastActivePopup(hWndWalk)) != hWndWalk)
		{
			if (IsWindowVisible(hWndTry)) break;
			hWndWalk = hWndTry;
		}
		// once the walk is finished hWndWalk "is" the taskbarwindow in that owner chain, now check if the window you supplied is that window
		return hWnd == hWndWalk;
	}

	public static void Main()
	{
		Console.WriteLine("window in taskbar: " + IsWindowInTaskbar(0x270128));
	}
}

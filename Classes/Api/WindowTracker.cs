using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace sambar;
public partial class Api
{
	/// <summary>
	/// Tracks active windows and exposes events
	/// that fire when focus is changed 
	/// </summary>

	public delegate void ActiveWindowChangedHandler(RunningApp app);
	/// <summary>
	/// subscribe to this event to recieve active window
	/// changed messages
	/// </summary>
	public event ActiveWindowChangedHandler ACTIVE_WINDOW_CHANGED_EVENT = (app) => { };

	List<RunningApp> runningApps = new();
	public void WindowingInit()
	{
		RefreshRunningApps();
		FOCUS_CHANGED_EVENT += WindowFocusChangedHandler;
	}

	public void RefreshRunningApps()
	{
		List<nint> hWndsInTaskbar = Utils.GetAllTaskbarWindows();
		runningApps = new();
		foreach (nint hWnd in hWndsInTaskbar)
		{
			runningApps.Add(new(hWnd));
		}
	}

	public void WindowFocusChangedHandler(FocusChangedMessage msg)
	{
		// we dont use IUIAutomation's hWnd because it returns elements inner to native windows
		// which most often do not have a native win32 handle
		nint foreground_hWnd = User32.GetForegroundWindow();
		Debug.WriteLine($"FOCUS CHANGED, foreground_hWnd: {foreground_hWnd}, foreground_className: {Utils.GetClassNameFromHWND(foreground_hWnd)}");

		// refresh running apps every time to account for newer windows
		RefreshRunningApps();
		List<RunningApp> apps = new();
		if ((apps = runningApps.Where(app => app.hWnd == foreground_hWnd).ToList()).Count() > 0)
		{
			ACTIVE_WINDOW_CHANGED_EVENT(apps.First());
			Debug.WriteLine($"ACTIVE WINDOW CHANGED: {apps.First().title}");
		}
	}

	public List<RunningApp> GetTaskbarApps()
	{
		RefreshRunningApps();
		return runningApps;
	}
}

public class RunningApp
{
	public nint hWnd;
	public string title;
	public string exePath;
	public string className;
	public uint processId;
	public BitmapSource icon;
	public RunningApp(nint hWnd)
	{
		this.hWnd = hWnd;
		StringBuilder str = new(256);
		User32.GetWindowText(hWnd, str, str.Capacity);
		title = str.ToString();
		exePath = Utils.GetExePathFromHWND(hWnd);
		className = Utils.GetClassNameFromHWND(hWnd);
		User32.GetWindowThreadProcessId(hWnd, out processId);

		Shell32.ExtractIconEx(exePath, 0, out nint largeIcon, out nint smallIcon, 1);
		if (largeIcon != 0)
		{
			icon = Imaging.CreateBitmapSourceFromHIcon(largeIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			icon.Freeze();
		}
	}
}

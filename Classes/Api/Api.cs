using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Windows;
using Interop.UIAutomationClient;

namespace sambar;

/// <summary>
/// The entire Api class provides core functionalities that a widget developer
/// can use in their plugins so that they dont have to reinvent the wheel.
/// </summary>
public partial class Api
{

	public List<Task> initTasks = new();
	public Api()
	{
		EventsInit();
		ToggleTaskbarInit();
		WindowingInit();
		SystemTrayInit();
		TaskbarInterceptorInit();
		ClockInit();
		CountersInit();
		initTasks.Add(Task.Run(GlazeInit));
	}

	public void Print(string text)
	{
		Logger.Log(text);
	}

	// so that widgets and scripts can use it
	public Config config;

	// instance of mainWindow
	public Sambar barWindow;

	// IUIAutomation
	CUIAutomation ui = new();
}


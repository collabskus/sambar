using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.WebSockets;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using System.ComponentModel;

namespace sambar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class Sambar : Window
{
	private nint hWnd;
	public static Api api;
	Config config;
	string widgetPackName;
	public Sambar(string widgetPackName, Config config)
	{
		// Initialize the api
		api = new();

		this.Title = "Bar";
		this.WindowStyle = WindowStyle.None;
		this.AllowsTransparency = true;
		this.Topmost = true;
		this.widgetPackName = widgetPackName;
		this.config = config;

		// setting a copy of the config to the API
		api.config = config;

		SourceInitialized += (s, e) =>
		{
			hWnd = new WindowInteropHelper(this).Handle;
			WindowInit();
			AddWidgets();
		};

		api.barWindow = this;
	}

	bool barTransparent = false;
	public void WindowInit()
	{
		int screenWidth = User32.GetSystemMetrics(0);
		int screentHeight = User32.GetSystemMetrics(1);

		if (config.width == 0) { config.width = screenWidth - (config.marginXLeft + config.marginXRight); }

		this.Background = Utils.BrushFromHex(config.backgroundColor);
		if (this.Background.Equals(Colors.Transparent)) { barTransparent = true; }

		uint exStyles = User32.GetWindowLong(hWnd, GETWINDOWLONG.GWL_EXSTYLE);
		User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)sambar.WINDOWSTYLE.WS_EX_TOOLWINDOW));

		//Win32.SetWindowPos(hWnd, IntPtr.Zero, config.marginXLeft, config.marginYTop, config.width, config.height, 0x0400);
		this.Width = config.width;
		this.Height = config.height;
		this.Left = config.marginXLeft;
		this.Top = config.marginYTop;

		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		if (!barTransparent) Dwmapi.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
	}

	public async void AddWidgets()
	{
		// the api has some blocking init tasks (looking at you glazewm) in the constructor that widgets might request, so only load the widgets once they are finished
		await Task.WhenAll(api.initTasks);
		WidgetLoader widgetLoader = new(widgetPackName, this);
	}
}

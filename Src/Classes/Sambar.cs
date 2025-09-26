/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

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
using System.Windows.Forms;

namespace sambar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class Sambar : Window
{
	public nint hWnd;
	public static Api? api;
	Config config;
	public string widgetPackName;
	bool firstShow = true;
	internal Sambar(string widgetPackName, Config config)
	{
		// Initialize the following in order
		// 1. window 
		// 2. api
		// 3. widgets

		this.Title = "Bar";
		this.WindowStyle = WindowStyle.None;
		this.AllowsTransparency = true;
		this.Topmost = true;
		this.widgetPackName = widgetPackName;
		this.config = config;

		// WPF event sequence
		// https://memories3615.wordpress.com/2017/03/24/wpf-window-events-sequence/
		SourceInitialized += (s, e) =>
		{
			hWnd = new WindowInteropHelper(this).Handle;
			WindowInit(); // needs hWnd
		};

		Activated += (s, e) =>
		{
			if (firstShow)
			{
				api = new(this);
				api.config = config; //setting a copy of the config to the API 
				AddWidgets();
				firstShow = false;
			}
		};
	}

	public static double scale;
	bool barTransparent = false;
	public static int screenWidth;
	public static int screenHeight;
	public void WindowInit()
	{
		screenWidth = User32.GetSystemMetrics(0);
		screenHeight = User32.GetSystemMetrics(1);

		// get the scalefactor of the primary monitor
		scale = Utils.GetDisplayScaling();
		Logger.Log($"Scale factor: {scale}");
		screenWidth = (int)(screenWidth / scale);
		screenHeight = (int)(screenHeight / scale);

		if (config.width == 0) { config.width = screenWidth - (config.marginXLeft + config.marginXRight); }

		this.Background = Utils.BrushFromHex(config.backgroundColor);
		if (this.Background.Equals(Colors.Transparent)) { barTransparent = true; }

		// Make bar a toolwindow (appear always on top)
		// TODO: loses topmost to other windows when task manager is open
		uint exStyles = User32.GetWindowLong(hWnd, GETWINDOWLONG.GWL_EXSTYLE);
		User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)sambar.WINDOWSTYLE.WS_EX_TOOLWINDOW));

		Utils.HideWindowInAltTab(hWnd);

		//Win32.SetWindowPos(hWnd, IntPtr.Zero, config.marginXLeft, config.marginYTop, config.width, config.height, 0x0400);
		this.Width = config.width;
		this.Height = config.height;
		this.Left = config.marginXLeft;
		this.Top = config.marginYTop;

		this.BorderBrush = Utils.BrushFromHex(config.borderColor);
		this.BorderThickness = config.borderThickness;

		Logger.Log($"this.Width: {config.width}");

		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		if (!barTransparent && config.roundedCorners)
			Dwmapi.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

		// bar right click context menu
		this.SetContextMenu([
			("Exit", (s, e) => { Exit(); }),
		]);
	}

	public async void AddWidgets()
	{
		// the api has some blocking init tasks (looking at you glazewm) in the constructor that widgets might request, so only load the widgets once they are finished
		await Task.WhenAll(api!.initTasks);
		WidgetLoader widgetLoader = new();
		// since the api starts much earlier than the widgets, fire events to update state once
		// widgets are loaded
		Sambar.api.FlushEvents();
	}

	// cleanup and exit
	public void Exit()
	{
		Sambar.api?.Cleanup();
		_Main.app.Shutdown();
	}
}

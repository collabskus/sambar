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

namespace sambar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class Sambar : Window
{
	private IntPtr hWnd;
    public static Api api = new();
	string configFile = "C:\\Users\\Jayakuttan\\dev\\sambar\\sambar.json";
	BarConfig config = new();
	public Sambar()
	{
        InitializeComponent();
		api.barWindow = this;
		SourceInitialized += (s, e) =>
		{
			hWnd = new WindowInteropHelper(this).Handle;
			WindowInit();
            AddWidgets();
		};

	}
	
    bool barTransparent = false;
    public void WindowInit()
	{
		this.Topmost = true;

        int screenWidth = User32.GetSystemMetrics(0);
		int screentHeight = User32.GetSystemMetrics(1);

		config = new(screenWidth);

		if (File.Exists(configFile)) {
			config = JsonConvert.DeserializeObject<BarConfig>(File.ReadAllText(configFile));
			if(config.width == 0) { config.width = screenWidth - (config.marginXLeft + config.marginXRight);  }
        }
		
		// setting a copy of the config to the API
		api.config = config;

		this.Background = Utils.BrushFromHex(config.backgroundColor);
		if(this.Background.Equals(Colors.Transparent)) { barTransparent = true; }

		uint exStyles = User32.GetWindowLong(hWnd, -20);
        User32.SetWindowLong(hWnd, -20, (int)(exStyles | (uint)sambar.WINDOWSTYLE.WS_EX_TOOLWINDOW));

		//Win32.SetWindowPos(hWnd, IntPtr.Zero, config.marginXLeft, config.marginYTop, config.width, config.height, 0x0400);
		this.Width = config.width;
		this.Height = config.height;
		this.Left = config.marginXLeft;
		this.Top = config.marginYTop;

		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		if (!barTransparent) Dwmapi.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
	}

	public void AddWidgets()
	{
		WidgetLoader widgetLoader = new(config.widgetPack, this);
		//widgetLoader.widgets.ForEach(widget => WidgetStackPanel.Children.Add(widget));
		//this.Content = widgetLoader.layout.Structure;
	}
}

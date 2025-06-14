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
public partial class Bar : Window
{
	private IntPtr hWnd;

	string configFile = "C:\\Users\\Jayakuttan\\dev\\sambar\\sambar.json";
	BarConfig config = new();
	public Bar()
	{
        InitializeComponent();

		SourceInitialized += (s, e) =>
		{
			hWnd = new WindowInteropHelper(this).Handle;
			WindowInit();
            AddWidgets();
		};

		Api.barWindow = this;
	}
	
    bool barTransparent = false;
    public void WindowInit()
	{
		this.Topmost = true;

        int screenWidth = Win32.GetSystemMetrics(0);
		int screentHeight = Win32.GetSystemMetrics(1);

		config = new(screenWidth);

		if (File.Exists(configFile)) {
			config = JsonConvert.DeserializeObject<BarConfig>(File.ReadAllText(configFile));
			if(config.width == 0) { config.width = screenWidth - (config.marginXLeft + config.marginXRight);  }
        }
		
		// setting a copy of the config to the API
		Api.config = config;

		this.Background = Utils.BrushFromHex(config.backgroundColor);
		if(this.Background.Equals(Colors.Transparent)) { barTransparent = true; }

		uint exStyles = Win32.GetWindowLong(hWnd, -20);
		Win32.SetWindowLong(hWnd, -20, (int)(exStyles | (uint)WindowStyles.WS_EX_TOOLWINDOW));

		//Win32.SetWindowPos(hWnd, IntPtr.Zero, config.marginXLeft, config.marginYTop, config.width, config.height, 0x0400);
		this.Width = config.width;
		this.Height = config.height;
		this.Left = config.marginXLeft;
		this.Top = config.marginYTop;

		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		if (!barTransparent) Win32.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
	}

	public void AddWidgets()
	{
		WidgetLoader widgetLoader = new(config.widgetPack, this);
		//widgetLoader.widgets.ForEach(widget => WidgetStackPanel.Children.Add(widget));
		//this.Content = widgetLoader.layout.Structure;
	}
}

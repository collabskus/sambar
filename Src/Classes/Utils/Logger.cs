/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using System.DirectoryServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace sambar;

public class Logger
{
	public static bool DEBUG = true;
	public static bool CONSOLE = false;

	public static void Log(string? text)
	{
		if (DEBUG) Debug.WriteLine(text);
		if(CONSOLE) Console.WriteLine(text);
	}

	public static void Log(List<string> array) {
		foreach(var arr in array) Log(arr);
	}
}

public class LoggerWindow
{
	Window? wnd;
	FrameworkElement? content;
	TextBlock? debugConsole;
	bool initialized = false;
	public LoggerWindow(Type? contentType = null, params dynamic[] contentConstructorArgs)
	{
        // window runs on a separate thread so that UI heavy updates dont
        // slow down the main UI thread.
        Thread thread = new(() =>
        {
            wnd = new();
            wnd.Width= 800;
            wnd.Height = 400;

			StackPanel panel = new();	
            if (contentType != null)
            {
                // all this so that this windows content UIElement can be created in this thread
                content = (FrameworkElement?)Activator.CreateInstance(contentType, contentConstructorArgs);
				content!.Height = 3 * wnd.Height / 4;
				content!.Width= wnd.Width;
				wnd.SizeChanged += (s, e) => 
				{
                    content.Height = 3 * e.NewSize.Height / 4;
                    content.Width = e.NewSize.Width;
				};
                panel.Children.Add(content);
            }

			debugConsole = new();
			panel.Children.Add(debugConsole);

			wnd.Content = panel;
            wnd.Show();

            initialized = true;
            System.Windows.Threading.Dispatcher.Run();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
	}

	public UIElement? GetContent()
	{
		while(!initialized) Thread.Sleep(1);
		return content;
	}

	public void Log(string message) 
	{
		if (debugConsole == null) return;
		wnd?.Dispatcher.Invoke(() =>
		{
            debugConsole.Text += "\n" + message;
        });
	}
}


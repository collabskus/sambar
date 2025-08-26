/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using System.Windows;

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
	
	// window runs on a separate thread so that UI heavy updates dont
	// slow down the main UI thread.
    public static (Window?, UIElement?) NewWindow(Type? uiElementType = null, params dynamic[] uiElementInitializer)
	{
		Window? logWnd = null;
		UIElement? content = null;
		bool finished = false;
		Thread thread = new(() =>
		{
            logWnd = new();
            logWnd.Width= 800;
            logWnd.Height = 400;
			if (uiElementType != null)
			{
				// all this so that this windows content UIElement can be created in this thread
				logWnd.Content = content = (UIElement?)Activator.CreateInstance(uiElementType, uiElementInitializer);
			}
            logWnd.Show();
            finished = true;
            System.Windows.Threading.Dispatcher.Run();
        });
		thread.SetApartmentState(ApartmentState.STA);
		thread.IsBackground = true;
		thread.Start();
		while (!finished) Thread.Sleep(1);
		return (logWnd, content);
    }
}

public class LoggerWindow
{
	Window? wnd;
	UIElement? content;
	bool initialized = false;
	public LoggerWindow(Type? contentType = null, params dynamic[] contentConstructorArgs)
	{
        Thread thread = new(() =>
        {
            wnd = new();
            wnd.Width= 800;
            wnd.Height = 400;
            if (contentType != null)
            {
                // all this so that this windows content UIElement can be created in this thread
                wnd.Content = content = (UIElement?)Activator.CreateInstance(contentType, contentConstructorArgs);
            }
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
}


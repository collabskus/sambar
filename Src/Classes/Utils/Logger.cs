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

    public static Window NewWindow(UIElement? content = null)
	{
		Window logWnd = new();
		logWnd.Width= 800;
		logWnd.Height = 400;
		logWnd.Content = content;
		logWnd.Show();
		return logWnd;
	}
}

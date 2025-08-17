/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;

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
}

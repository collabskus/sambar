using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Target
{
	public static void Main()
	{
		Console.WriteLine("blocking...");
		Console.WriteLine($"processId: {Process.GetCurrentProcess().Id}");
		Console.ReadKey();
	}
}

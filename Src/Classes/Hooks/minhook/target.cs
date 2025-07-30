using System;
using System.Diagnostics;

class Target
{
	public static void Main()
	{
		Console.WriteLine("blocking...");
		Console.WriteLine($"processId: {Process.GetCurrentProcess().Id}");
		Console.ReadKey();
	}
}

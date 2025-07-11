using System.Windows;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace sambar;

public class Program
{
	[STAThread]
	static void Main(string[] args)
	{
		// evaluate the .init.cs to get the widget pack name
		if (!File.Exists(@"C:\Users\Jayakuttan\dev\sambar\WidgetPacks\.init.cs"))
		{
			Debug.WriteLine(".init.cs does not exist, exiting...");
			return;
		}
		string _initcs = File.ReadAllText(@"C:\Users\Jayakuttan\dev\sambar\WidgetPacks\.init.cs");
		string? widgetPackName = null;
		Thread _t = new(async () =>
		{
			try
			{
				widgetPackName = await CSharpScript.EvaluateAsync<string>(_initcs);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"unable to compile .init.cs");
				Debug.WriteLine(ex.Message);
			}
		});
		_t.Start();
		_t.Join();
		Debug.WriteLine($"widgetPackName: {widgetPackName}");
		if (widgetPackName == null) return;

		Debug.WriteLine($"Compiling config");
		Utils.CompileToDll($@"C:\Users\Jayakuttan\dev\sambar\WidgetPacks\{widgetPackName}\.config.cs", ".config");
		Assembly configAssembly = Assembly.LoadFile(@"C:\Users\Jayakuttan\dev\sambar\_.dll\.config.dll");
		Type configType = configAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Config))).First();
		Config config = (Config)Activator.CreateInstance(configType);

		// start the wpf bar window
		Application app = new();
		Sambar sambar = new(widgetPackName, config);
		app.Run(sambar);
	}
}

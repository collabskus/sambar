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
		if (!File.Exists(Paths.initCsFile))
		{
			Debug.WriteLine(".init.cs does not exist, exiting...");
			return;
		}
		string _initcs = File.ReadAllText(Paths.initCsFile);
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
		string configFile = Path.Join(Paths.widgetPacksFolder, widgetPackName, ".config.cs");
		if (!File.Exists(configFile))
		{
			Debug.WriteLine("widget pack does not contain .config.cs file");
			return;
		}
		Utils.CompileToDll(configFile, ".config");
		Assembly configAssembly = Assembly.LoadFile(Paths.configDll);
		Type configType = configAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Config))).First();
		Config config = (Config)Activator.CreateInstance(configType);

		// start the wpf bar window
		Application app = new();
		Sambar sambar = new(widgetPackName, config);
		app.Run(sambar);
	}
}

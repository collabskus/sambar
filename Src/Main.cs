/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

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
		// for logger
		Kernel32.AttachConsole(-1);

		Paths.CreateIfAbsent();

		// evaluate the .init.cs to get the widget pack name
		string? widgetPackName = WidgetLoader.GetObjectFromScript<string>(Paths.initCsFile);
		if (widgetPackName == null) return;

		Logger.Log($"Compiling config");
		string configFile = Path.Join(Paths.widgetPacksFolder, widgetPackName, ".config.cs");
		if (!File.Exists(configFile))
		{
			Logger.Log("widget pack does not contain .config.cs file");
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

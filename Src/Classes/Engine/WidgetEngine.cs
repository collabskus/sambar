/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Reflection;
using System.IO;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace sambar;

public class WidgetLoader
{
	public List<Widget> widgets = new();

	Dictionary<string, string> widgetToDllMap = new();

	public WidgetLoader(string widgetPackName, Window window)
	{
		var files = new DirectoryInfo(Path.Join(Paths.widgetPacksFolder, widgetPackName)).GetFiles();
		var widgetFiles = files.Where(file => file.Name.EndsWith(".widget.cs")).ToList();
		var cachedDlls = new DirectoryInfo(Paths.dllFolder).GetFiles();

		// read imports file and add widgets if any
		string importsFile = Path.Join(Paths.widgetPacksFolder, widgetPackName, ".imports.cs");
		WidgetImports? imports = null;
		if (Path.Exists(importsFile))
		{
			imports = GetObjectFromScript<WidgetImports>(importsFile);
		}

		Logger.Log(imports == null ? "No .imports.cs file found!" : $".imports.cs: {imports.ImportsPack}");

		// verify hashes and figure out which widgets to compile		
		List<FileInfo> widgetFilesToCompile = new();
		if (!File.Exists(Paths.hashesFile))
		{
			BuildWidgetHistory(widgetFiles);
			widgetFilesToCompile = widgetFiles;
		}
		else
		{
			widgetToHash = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Paths.hashesFile));
			foreach (var file in widgetFiles)
			{
				string hash = ComputeWidgetSriptHash(File.ReadAllText(file.FullName));

				// new script added
				if (!widgetToHash.ContainsKey(file.Name))
				{
					widgetFilesToCompile.Add(file);
				}
				else if (widgetToHash[file.Name] != hash)
				{
					widgetFilesToCompile.Add(file);
				}
			}
		}

		// add script to compile list if dll is missing
		widgetFiles.ForEach(file =>
		{
			string widgetName = file.Name.Replace(".widget.cs", "");
			widgetToDllMap[widgetName] = Path.Join(Paths.dllFolder, widgetName + ".widget.dll");
			if (!cachedDlls.Select(dll => dll.Name).ToList().Contains(file.Name.Replace(".cs", ".dll")))
			{
				widgetFilesToCompile.Add(file);
			}
		});

		Logger.Log($"To compile: {widgetFilesToCompile.Count()}");
		widgetFilesToCompile.ForEach(file => Logger.Log($"name: {file.Name}"));

		var themesFile = files.Where(file => file.Name == ".theme.cs").First();
		string widgetsPrefix = File.ReadAllText(themesFile.FullName);

		var layoutFile = files.Where(file => file.Name == ".layout.cs").First();
		string layoutFileContent = File.ReadAllText(layoutFile.FullName);

		Thread thread = new(() => { CompileToDll(layoutFileContent, ".layout"); });
		thread.Start();
		thread.Join();

		var layoutAssembly = Assembly.LoadFile(Path.Join(Paths.dllFolder, ".layout.dll"));
		Type layoutType = layoutAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Layout))).First();
		Layout layout = (Layout)Activator.CreateInstance(layoutType);

		// IMPORTANT
		window.Content = layout?.Container;

		widgetFilesToCompile
			.ForEach(
				file =>
				{
					Logger.Log($"Compiling {file.Name}");
					string fileContent = File.ReadAllText(file.FullName);
					string finalScript = widgetsPrefix + "\n" + fileContent;
					string dllName = file.Name.Replace(".cs", "");
					Thread thread = new(() => { CompileToDll(finalScript, $"{dllName}"); });
					thread.Start();
					thread.Join();
					widgetToDllMap[file.Name.Replace(".widget.cs", "")] = Path.Join(Paths.dllFolder, dllName + ".dll");
				}
			);

		// update hashes after compilation (if any)
		BuildWidgetHistory(widgetFiles);

		Logger.Log("Loading compiled dlls...");
		foreach (var widgetName in widgetToDllMap)
		{
			var assembly = Assembly.LoadFile(widgetName.Value);
			Type[] typesInAssembly = assembly.GetTypes();
			Type widgetType = typesInAssembly.Where(type => type.IsSubclassOf(typeof(Widget))).First();
			Widget widget = (Widget)Activator.CreateInstance(widgetType);
			if (widget != null) { widgets.Add(widget); }
		}

		widgets.ForEach(
			widget =>
			{
				layout.WidgetToContainerMap[widget.GetType().Name].Child = widget;
			}
		);

		GC.Collect();
	}

	public static void CompileToDll(string classCode, string dllName)
	{
		MetadataReference[] references =
		[
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Sambar).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Thread).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Application).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(UIElement).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Control).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Brush).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(JsonConvert).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Enum).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(DependencyObject).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(DrawingAttributes).Assembly.Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
		];

		string usingsPrefix =
"""
using sambar;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net.Http;
using System.Windows.Ink;
using Newtonsoft.Json;
""";

		string code = usingsPrefix + classCode;
		CSharpParseOptions parseOptions = new(LanguageVersion.Preview);
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions);
		CSharpCompilationOptions compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

		CSharpCompilation compilation = CSharpCompilation.Create(
			$"{dllName}.dll",
			[syntaxTree],
			references,
			compilationOptions
		);

		using var ms = new MemoryStream();
		var result = compilation.Emit(ms);

		if (!result.Success)
		{
			Logger.Log($"COMPILATION FAILED: {dllName}");
			foreach (Diagnostic err in result.Diagnostics)
			{
				Logger.Log(err.GetMessage());
			}
		}
		ms.Seek(0, SeekOrigin.Begin);
		var assembly = Assembly.Load(ms.ToArray());
		File.WriteAllBytes(Path.Join(Paths.dllFolder, $"{dllName}.dll"), ms.ToArray());
		Logger.Log("Types found: " + assembly.GetTypes().First().Name);
	}

	public string ComputeWidgetSriptHash(string widgetCode)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(widgetCode);
		MD5 md5 = MD5.Create();
		return Convert.ToHexStringLower(md5.ComputeHash(bytes));
	}

	Dictionary<string, string> widgetToHash = new();
	public void BuildWidgetHistory(List<FileInfo> widgetFiles)
	{
		if (File.Exists(Paths.hashesFile)) { File.Delete(Paths.hashesFile); }
		foreach (var file in widgetFiles)
		{
			string content = File.ReadAllText(file.FullName);
			string hash = ComputeWidgetSriptHash(content);
			widgetToHash[file.Name] = hash;
		}
		string historyFile = System.Text.Json.JsonSerializer.Serialize(widgetToHash);
		File.WriteAllTextAsync(Paths.hashesFile, historyFile);
	}

	/// <summary>
	/// Evaluate Script
	/// (Alternative to CompileDll)
	/// </summary>
	public static T? GetObjectFromScript<T>(string scriptPath)
	{
        if (!File.Exists(scriptPath))
		{
			Logger.Log($"{scriptPath} does not exist, exiting...");
			return default(T);
		}
		string script = File.ReadAllText(scriptPath);
		ScriptOptions options = ScriptOptions.Default
								.AddReferences(typeof(Sambar).Assembly)
								.WithImports("sambar");
		T? obj = default(T);
		Thread _t = new(async () =>
		{
			try
			{
				obj = await CSharpScript.EvaluateAsync<T>(script, options: options);
			}
			catch (Exception ex)
			{
				Logger.Log($"unable to compile {scriptPath}");
				Logger.Log(ex.Message);
			}
		});
		_t.Start();
		_t.Join();
		return obj;
	}

}


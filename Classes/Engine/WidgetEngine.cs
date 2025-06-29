using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Reflection;
using System.IO;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;
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

public enum WidgetVerticalPosition
{
	TOP, CENTER, BOTTOM
}

public class Widget : Border 
{
	public int index;

	public UIElement Content
	{
		get { return this.Child; }
		set { this.Child = value; }
	}

	public Dispatcher Thread
	{
		get { return this.Dispatcher; }
	}

	BarConfig config;	
	public Widget() {
		this.HorizontalAlignment = HorizontalAlignment.Left;
		config = Sambar.api.config;
	}
}

public class WidgetLoader
{
	public List<Widget> widgets = new();

	Dictionary<string, string> widgetToDllMap = new();

	string widgetsFolder = "C:\\Users\\Jayakuttan\\dev\\sambar\\WidgetPacks";

	string dllFolder = "C:\\Users\\Jayakuttan\\dev\\sambar\\_.dll";

	string hashesFile;

	public WidgetLoader(string widgetPack, Window window)
	{
		hashesFile = Path.Join(dllFolder, "hashes.json");

        var files = new DirectoryInfo(Path.Join(widgetsFolder, "Base")).GetFiles();
		var widgetFiles = files.Where(file => file.Name.EndsWith(".widget.cs")).ToList();
		var cachedDlls = new DirectoryInfo(dllFolder).GetFiles();

		// verify hashes and figure out which widgets to compile		
		List<FileInfo> widgetFilesToCompile = new();
		if(!File.Exists(hashesFile))
		{
			BuildWidgetHistory(widgetFiles);
			widgetFilesToCompile = widgetFiles;
		}
		else
		{
			widgetToHash = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(hashesFile));
			foreach(var file in widgetFiles) {
				string hash = ComputeWidgetSriptHash(File.ReadAllText(file.FullName));

				// new script added
				if(!widgetToHash.ContainsKey(file.Name))
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
        widgetFiles.ForEach(file => {
			string widgetName = file.Name.Replace(".widget.cs", "");
			widgetToDllMap[widgetName] = Path.Join(dllFolder, widgetName + ".widget.dll");
			if(!cachedDlls.Select(dll => dll.Name).ToList().Contains(file.Name.Replace(".cs", ".dll")))
			{
				widgetFilesToCompile.Add(file);
			}
        });

		Debug.WriteLine($"To compile: {widgetFilesToCompile.Count()}");
		widgetFilesToCompile.ForEach(file => Debug.WriteLine($"name: {file.Name}"));

        var themesFile = files.Where(file => file.Name == ".theme.cs").First();
		string widgetsPrefix = File.ReadAllText(themesFile.FullName);

        var layoutFile = files.Where(file => file.Name == ".layout.cs").First();
        string layoutFileContent = File.ReadAllText(layoutFile.FullName);

		Thread thread = new(() => { CompileToDll(layoutFileContent, ".layout"); });
		thread.Start();
		thread.Join();

		var layoutAssembly = Assembly.LoadFile(Path.Join(dllFolder, ".layout.dll"));
		Type layoutType = layoutAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Layout))).First();
		Layout layout = (Layout)Activator.CreateInstance(layoutType);
		
		// IMPORTANT
		window.Content = layout.Container;

        widgetFilesToCompile
			.ForEach(
				async file =>
				{
					Debug.WriteLine($"Compiling {file.Name}");
					string fileContent = File.ReadAllText(file.FullName);
					string finalScript = widgetsPrefix + "\n" + fileContent;
					string dllName = file.Name.Replace(".cs", "");
					Thread thread = new(() => { CompileToDll(finalScript, $"{dllName}"); });
					thread.Start();
					thread.Join();
					widgetToDllMap[file.Name.Replace(".widget.cs", "")] = Path.Join(dllFolder, dllName + ".dll");
				}
			);

		// update hashes after compilation (if any)
		BuildWidgetHistory(widgetFiles);

		Debug.WriteLine("Loading compiled dlls...");
		foreach(var widgetName in widgetToDllMap)
		{
			var assembly = Assembly.LoadFile(widgetName.Value);
			Type[] typesInAssembly = assembly.GetTypes();
			Type widgetType = typesInAssembly.Where(type => type.IsSubclassOf(typeof(Widget))).First();
			Widget widget = (Widget)Activator.CreateInstance(widgetType);
			if(widget != null) { widgets.Add(widget); }
		}

		widgets.ForEach(
			widget => 
			{
				layout.WidgetToContainerMap[widget.GetType().Name].Child = widget;
			}
		);

		GC.Collect();
	}

	public void CompileToDll(string classCode, string dllName)
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
using Newtonsoft.Json;
""";

        string code = usingsPrefix + classCode;
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        CSharpCompilation compilation = CSharpCompilation.Create(
            $"{dllName}.dll",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if(!result.Success)
        {
            Debug.WriteLine("COMPILATION FAILED");
            foreach(Diagnostic err in result.Diagnostics)
            {
                Debug.WriteLine(err);
            }
        }
        ms.Seek(0, SeekOrigin.Begin);
		var assembly = Assembly.Load(ms.ToArray());
		File.WriteAllBytes($"C:\\Users\\Jayakuttan\\dev\\sambar\\_.dll\\{dllName}.dll", ms.ToArray());
        Debug.WriteLine("Types found: " + assembly.GetTypes().First().Name);
	}

	public string ComputeWidgetSriptHash(string widgetCode) {
		byte[] bytes = Encoding.UTF8.GetBytes(widgetCode);
		MD5 md5 = MD5.Create();
		return Convert.ToHexStringLower(md5.ComputeHash(bytes));
	}


    Dictionary<string, string> widgetToHash = new();
	public void BuildWidgetHistory(List<FileInfo> widgetFiles)
	{
		if(File.Exists(hashesFile)) { File.Delete(hashesFile); }
		foreach(var file in widgetFiles)
		{
			string content = File.ReadAllText(file.FullName);
			string hash = ComputeWidgetSriptHash(content);
			widgetToHash[file.Name] = hash;
		}
		string historyFile = System.Text.Json.JsonSerializer.Serialize(widgetToHash);
		File.WriteAllTextAsync(hashesFile, historyFile);
	}

}


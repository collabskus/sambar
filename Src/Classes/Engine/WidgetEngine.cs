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
using Windows.UI.Composition;
using ScottPlot.WPF;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using SkiaSharp.Views.Desktop;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Windows.Media.Animation;

namespace sambar;

internal class WidgetLoader
{
	public List<Widget> widgets = new();

	Dictionary<string, string> widgetToDllMap = new();

	WidgetImports? imports = null;

	string? widgetPackName;

	bool catchWidgetExceptions = false;

	public WidgetLoader(Sambar bar)
	{
		widgetPackName = bar.widgetPackName;

		var files = new DirectoryInfo(Path.Join(Paths.widgetPacksFolder, widgetPackName)).GetFiles();
		var widgetFiles = files.Where(file => file.Name.EndsWith(".widget.cs")).ToList();
		var cachedDlls = new DirectoryInfo(Paths.dllFolder).GetFiles();

		// read imports file and add widgets if any
		string importsFile = Path.Join(Paths.widgetPacksFolder, widgetPackName, ".imports.cs");
		if (Path.Exists(importsFile))
		{
			imports = GetObjectFromScript<WidgetImports>(importsFile);
			if (!Directory.Exists(Path.Join(Paths.widgetPacksFolder, imports?.importsPack))) throw new Exception($"{imports?.importsPack} does not exist");
			imports?.widgets.ForEach(_widget => widgetFiles.Add(new(Path.Join(Paths.widgetPacksFolder, imports.importsPack, _widget + ".widget.cs"))));
		}

		Logger.Log(imports == null ? "No .imports.cs file found!" : $".imports.cs, pack: {imports.importsPack}");

		// verify hashes and figure out which widgets to compile		
		List<FileInfo> widgetFilesToCompile = new();
		if (!File.Exists(Paths.hashesFile))
		{
			BuildWidgetHistory(widgetFiles);
			widgetFilesToCompile = new(widgetFiles);
		}
		else
		{
			widgetToHash = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Paths.hashesFile));
			widgetFiles
				.ForEach(
				file =>
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
				});
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

		var themesFile = files.Where(file => file.Name == ".theme.cs").FirstOrDefault();
		string? widgetsPrefix = null;
		//bool compileWithThemes = false;
		if (themesFile != null)
		{
			//compileWithThemes = true;
			//widgetsPrefix = File.ReadAllText(themesFile.FullName);
			Utils.CompileFileToDll(themesFile.FullName, ".theme");
		}

		var layoutFile = files.Where(file => file.Name == ".layout.cs").FirstOrDefault();
		if (layoutFile == null) throw new Exception($".layout.cs missing in {widgetPackName}");
		Utils.CompileFileToDll(layoutFile.FullName, ".layout");

		var layoutAssembly = Assembly.LoadFile(Path.Join(Paths.dllFolder, ".layout.dll"));
		Type layoutType = layoutAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Layout))).First();

		// IMPORTANT
		Layout? layout = null;
		bar.UIThread(() =>
		{
			layout = (Layout?)Activator.CreateInstance(layoutType);
			Sambar.api!.bar.Content = layout?.Container;
		});

		widgetFilesToCompile
			.ForEach(
				file =>
				{
					Logger.Log($"Compiling {file.Name}");
					string fileContent = File.ReadAllText(file.FullName);
					//string finalScript = widgetsPrefix + "\n" + fileContent;
					string dllName = file.Name.Replace(".cs", "");
					Utils.CompileStringToDll(fileContent, $"{dllName}", [(Path.Join(Paths.dllFolder, ".theme.dll"), null)], wrapInTryCatch: catchWidgetExceptions);
					widgetToDllMap[file.Name.Replace(".widget.cs", "")] = Path.Join(Paths.dllFolder, dllName + ".dll");
				}
			);

		// update hashes after compilation (if any)
		BuildWidgetHistory(widgetFiles);

		Logger.Log("Loading compiled dlls...");
		// add _.dll to dependency search path so that if widgets contain dependency dlls
		// such as .theme.dll they are loaded
		AssemblyLoadContext.Default.Resolving += (assemblyLoadContext, name) =>
		{
			string _assemblyPath = Path.Join(Paths.dllFolder, name.Name);
			if (File.Exists(_assemblyPath)) return Assembly.LoadFrom(_assemblyPath);
			return null;
		};
		foreach (var widgetName in widgetToDllMap)
		{
			var assembly = Assembly.LoadFile(widgetName.Value);
			Type[] typesInAssembly = assembly.GetTypes();
			Type widgetType = typesInAssembly.Where(type => type.IsSubclassOf(typeof(Widget))).First();
			// prepare the ENV VARS for widget
			WidgetEnv env = PrepareEnvVarsForWidget(widgetName.Key);
			Widget? widget = null;

			bar.UIThread(() =>
			{
				try
				{
					// BUG: STARTBUTTON CRASHING
					widget = (Widget)Activator.CreateInstance(widgetType, [env])!; // where the widget actually runs

					// check if <widgetName.mod.cs> exists and apply 
					string modFile = Path.Join(Paths.widgetPacksFolder, widgetPackName, $"{widgetName.Key}.mod.cs");
					if (File.Exists(modFile))
					{
						Logger.Log($"modFile exists: {modFile}");
						//                                         Widget,  WidgetEnv
						var modAction = GetObjectFromScript<Action<dynamic, dynamic>>(modFile);
						Logger.Log($"modFile exists: {modFile}, modActionNull: {modAction == null}");
						modAction?.Invoke(widget, env);
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"[ WIDGET-LOADING/MOD-FAILED, {widgetName.Key} (exception at constructor) ]", ex: ex);
				}
			});
			if (widget != null) { widgets.Add(widget); }
		}

		// widgets currently being loaded
		Logger.Log($"[ Currently loaded widgets: {widgets.Count} ]");
		//Logger.Log(widgets.Select(widget => widget.Name).ToList());

		widgets.ForEach(
			widget =>
			{
				//layout.WidgetToContainerMap[widget.GetType().Name].Child = widget;
				Border? border = layout?.WidgetToContainerMap.GetValueOrDefault(widget.GetType().Name);
				if (border != null)
				{
					bar.UIThread(() =>
					{
						border.Child = widget;
					});
				}
			}
		);

		GC.Collect();
	}

	/// <summary>
	/// compiles code (primarily classes) into dlls. Used to compile widgets, themes, layouts etc
	/// Since assembly cache is not unloaded automatically this must be run on a separate thread
	/// and discarded, so use the wrapper defined inside Utils
	/// </summary>
	/// <param name="classCode"></param>
	/// <param name="dllName"></param>
	/// <param name="additionalDllsAndUsings"></param>
	public static void CompileToDll(string classCode, string dllName, List<(string, string?)>? additionalDllsAndUsings = null, bool wrapInTryCatch = false)
	{
		List<MetadataReference> references =
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
			MetadataReference.CreateFromFile(typeof(WpfPlot).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(ScottPlot.Colors).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(System.Drawing.Color).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(ScottPlot.Plottables.Signal).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(SKSurface).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(SKElement).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(SKPaintSurfaceEventArgs).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Storyboard).Assembly.Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
		];

		string usingsPrefix =
"""
using sambar;
using System;
using System.IO;
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
using ScottPlot.WPF;
using ScottColors = ScottPlot.Colors;
using ScottPlot.Plottables;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System.Windows.Media.Animation;
""";

		if (additionalDllsAndUsings != null)
		{
			additionalDllsAndUsings
				.ForEach(dllAndUsing =>
				{
					references.Add(MetadataReference.CreateFromFile(dllAndUsing.Item1));
					if (dllAndUsing.Item2 != null) usingsPrefix += "\n" + $"using {dllAndUsing.Item2}";
				}
			);
		}

		string code = usingsPrefix + classCode;
		CSharpParseOptions parseOptions = new(LanguageVersion.Preview);
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions);

		// we do this so that every method in our widget class is wrapped inside a
		// try catch block
		if (wrapInTryCatch)
		{
			code = TryCatchAdder(code);
			syntaxTree = CSharpSyntaxTree.ParseText(code);
			Logger.Log($"[ WIDGET-COMPILE-STRING ]\n{code}");
		}

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
				Logger.Log($"{err.Location.SourceSpan}: {err.GetMessage()}");
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
	/// Evaluate string
	/// </summary>
	public static T? GetObjectFromString<T>(string script, string? scriptPath = null)
	{
		ScriptOptions options = ScriptOptions.Default
								.AddReferences(typeof(Sambar).Assembly)
								.AddReferences(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly)
								.AddReferences(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly)
								.WithImports("sambar");
		T? obj = default(T);
		Thread _t = new(() =>
		{
			try
			{
				Logger.Log($"EvaluateAsync(): started, {scriptPath}");
				Logger.Log(script);
				obj = CSharpScript.EvaluateAsync<T>(script, options: options).GetAwaiter().GetResult();
				Logger.Log($"EvaluateAsync(): finished, {scriptPath}");
			}
			catch (Exception ex)
			{
				Logger.Log($"unable to compile {(scriptPath == null ? script : scriptPath)}");
				Logger.Log(ex.Message);
			}
		});
		_t.Start();
		Task.Run(async () =>
		{
			while (_t.ThreadState != System.Threading.ThreadState.Stopped)
			{
				Logger.Log($"ThreadState: {_t.ThreadState}, IsAlive: {_t.IsAlive}");
				await Task.Delay(100);
			}
		});
		_t.Join();
		return obj;
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
		T? obj = default(T);
		Logger.Log($"GetObjectFromString<T>() started: {scriptPath}");
		obj = GetObjectFromString<T>(script, scriptPath);
		return obj;
	}

	public WidgetEnv PrepareEnvVarsForWidget(string widgetName)
	{
		WidgetEnv env = new();
		env.ASSETS_FOLDER = Path.Join(Paths.widgetPacksFolder, widgetPackName, "assets");
		if (imports!.widgets.Contains(widgetName))
		{
			env.IS_IMPORTED = true;
			env.IMPORTS_ASSETS_FOLDER = Path.Join(Paths.widgetPacksFolder, imports.importsPack, "assets");
		}
		return env;
	}

	public static string TryCatchAdder(string classCode)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(classCode);

		var methods = tree.GetRoot()
						  .DescendantNodes()
						  .OfType<MethodDeclarationSyntax>()
						  .ToArray();

		int charsAdded = 0;
		foreach (var method in methods)
		{
			int startBrace = method.Body.OpenBraceToken.SpanStart + charsAdded;
			int endBrace = method.Body.CloseBraceToken.SpanStart + charsAdded;
			bool returnDefault = method.ReturnType.ToString() != "void";
			string _classCode = AddTryCatchToFunctionBlock(classCode, startBrace, endBrace, returnDefault);
			charsAdded += _classCode.Length - classCode.Length;
			classCode = _classCode;
		}

		return classCode;
	}

	/// <summary>
	///     | startBrace
	/// try {
	/// 	-- functionBody --
	/// }
	/// | endBrace
	/// </summary>
	public static string AddTryCatchToFunctionBlock(string classCode, int startBrace, int endBrace, bool returnDefault = false)
	{
		return
		  new string(classCode.Take(startBrace + 1).ToArray())
		+ "try {"
		+ new string(classCode.Skip(startBrace + 1).Take(endBrace - startBrace - 1).ToArray())
		+ "}"
		+ "catch(Exception ex) {"
		+ """Sambar.api.Print($"[ WIDGET-EXCEPTION ]\n{ex.Message}"); Sambar.api.MessageBox(ex.Message);"""
		+ (returnDefault ? "return default;" : "return;")
		+ "}"
		+ new string(classCode.TakeLast(classCode.Length - endBrace).ToArray());
	}
}


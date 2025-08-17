/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.IO;

namespace sambar;

public class Paths
{
	public static string rootFolder = Directory.GetCurrentDirectory();
	public static string widgetPacksFolder = Path.Join(rootFolder, "WidgetPacks");
	public static string initCsFile = Path.Join(widgetPacksFolder, ".init.cs");
	public static string dllFolder = Path.Join(rootFolder, "_.dll");
	public static string configDll = Path.Join(dllFolder, ".config.dll");
	public static string hashesFile = Path.Join(dllFolder, "hashes.json");

	public static void CreateIfAbsent() {
		if(!File.Exists(widgetPacksFolder)) Directory.CreateDirectory(widgetPacksFolder);
		if(!File.Exists(dllFolder)) Directory.CreateDirectory(dllFolder);
	}
}

/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.IO;

namespace sambar;

public class Paths
{
	public static readonly string rootFolder = Directory.GetCurrentDirectory();
	public static readonly string widgetPacksFolder = Path.Join(rootFolder, "WidgetPacks");
	public static readonly string initCsFile = Path.Join(widgetPacksFolder, ".init.cs");
	public static readonly string dllFolder = Path.Join(rootFolder, "_.dll");
	public static readonly string configDll = Path.Join(dllFolder, ".config.dll");
	public static readonly string hashesFile = Path.Join(dllFolder, "hashes.json");

	// need to be set by once widgetPack is determined 
	public static string assetsFolder { 
		get 
		{
			return Path.Combine(widgetPacksFolder, Sambar.api!.bar.widgetPackName, "assets");
		} 
	}

	internal static void CreateIfAbsent() {
		if(!File.Exists(widgetPacksFolder)) Directory.CreateDirectory(widgetPacksFolder);
		if(!File.Exists(dllFolder)) Directory.CreateDirectory(dllFolder);
	}
}

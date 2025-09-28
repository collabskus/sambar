WidgetImports imports = new()
{
	importsPack = "Base",
	widgets = ["Clock", "Counters", "TaskbarApps", "AudioVisualizer", "AudioInformation", "Workspaces", "HideTaskbar"]
};
imports.usings["ActionCenter"] = new();
imports.usings["ActionCenter"].Add(("Base", "Tray"));
imports.usings["ActionCenter"].Add(("Base", "NetworkManager"));
imports.usings["ActionCenter"].Add(("Base", "Wallpapers"));
return imports;

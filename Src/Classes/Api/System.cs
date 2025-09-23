/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using System.Text;

namespace sambar;

public partial class Api
{
	IActiveDesktop activeDesktop;

	public void SystemInit()
	{
		Type? activeDesktopClass = Type.GetTypeFromCLSID(CLSID.ActiveDesktop);
		activeDesktop = (IActiveDesktop)Activator.CreateInstance(activeDesktopClass);
	}

	public string GetWallpaper()
	{
		StringBuilder str = new(512);
		activeDesktop?.GetWallpaper(str, str.Capacity, 0x00000001);
		Logger.Log($"the wallpaper is {str.ToString()}");
		return str.ToString();
	}

	public void SetWallPaper(string imageFile)
	{
		activeDesktop?.SetWallpaper(imageFile, imageFile.Length);
	}

	public void SetWallpaper(string imageFile, WallpaperAnimation animation, int duration)
	{

	}
}

public enum WallpaperAnimation
{
	CORNER_BLOOM,
	NONE
}



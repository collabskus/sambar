/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

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

	//public void SetWallPaper(string imageFile)
	//{
	//	activeDesktop?.SetWallpaper(imageFile, imageFile.Length);
	//}

	public void SetWallpaper(string imageFile, WallpaperAnimation animation, int duration = 2)
	{
		Window? wnd = Sambar.api?.CreateDesktopOverlay();
		wnd!.Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
		Canvas canvas = new();

		Image img = new() { Source = GetImageSource(imageFile) };
		//Image img2 = new() { Source = new BitmapImage(new Uri(@"C:\Users\Jayakuttan\Pictures\Wallpapers\1360350.png")) };

		Utils.ScaleImage(img, (int)wnd.Width, (int)wnd.Height);
		//Utils.ScaleImage(img2, (int)wnd.Width, (int)wnd.Height);

		double final_radius = Math.Max(wnd.Width, wnd.Height);
		final_radius += 0.25 * final_radius;

		double radiusX_initial = 0, radiusX_final = final_radius;
		double radiusY_initial = 0, radiusY_final = final_radius;

		EllipseGeometry ellipse = new(new Point(0, 0), radiusX_initial, radiusY_initial);
		// register a name for the ellipse so it can be targetted for animations
		NameScope.SetNameScope(wnd, new NameScope());
		string ellipseName = "ellipse";
		wnd!.RegisterName(ellipseName, ellipse);

		GeometryDrawing geometryDrawing = new() { Geometry = ellipse, Brush = new SolidColorBrush(Colors.Black) };

		//https://stackoverflow.com/questions/14283528/positioning-an-opacitymask-in-wpf
		DrawingBrush drawingBrush = new()
		{
			Drawing = geometryDrawing,
			Stretch = Stretch.None,
			ViewboxUnits = BrushMappingMode.Absolute,
			AlignmentX = AlignmentX.Left,
			AlignmentY = AlignmentY.Top
		};

		img.OpacityMask = drawingBrush;

		// Animation
		DoubleAnimation doubleAnimationX = new()
		{
			From = radiusX_initial,
			To = radiusX_final,
			Duration = TimeSpan.FromSeconds(duration),
			AutoReverse = false
		};
		DoubleAnimation doubleAnimationY = new()
		{
			From = radiusX_initial,
			To = radiusY_final,
			Duration = TimeSpan.FromSeconds(duration),
			AutoReverse = false
		};
		Storyboard storyboard = new();

		Storyboard.SetTargetName(doubleAnimationX, ellipseName);
		Storyboard.SetTargetProperty(doubleAnimationX, new PropertyPath(EllipseGeometry.RadiusXProperty));

		Storyboard.SetTargetName(doubleAnimationY, ellipseName);
		Storyboard.SetTargetProperty(doubleAnimationY, new PropertyPath(EllipseGeometry.RadiusYProperty));

		storyboard.Children.Add(doubleAnimationX);
		storyboard.Children.Add(doubleAnimationY);
		storyboard.Completed += (s, e) => wnd.Close();

		// triggers animation at window load
		wnd.Loaded += (s, e) => { storyboard.Begin(wnd); };
		canvas.Children.Add(img);

		//canvas.Children.Add(img2);

		//
		wnd!.Content = canvas;
		wnd.Show();
	}
}

public enum WallpaperAnimation
{
	CORNER_BLOOM,
	NONE
}



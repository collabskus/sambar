/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

///<summary>
/// Custom windows that can be used by widgets to make detached plugins
/// such as clocks, audio visualizers or whatever one so desires. Because it
/// doesnt fit inside Menus.cs
///</summary>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace sambar;

public partial class Api
{
    private void CustomWindowsInit()
    {

    }
}

public class ThreadWindow
{
    Window? wnd;
    public nint hWnd;
    FrameworkElement? content;
    bool initialized = false;
    public ThreadWindow(
        Type? contentType = null, 
        dynamic[]? contentConstructorArgs = null, 
        Action<Window>? wndInit = null,
        int width = 800,
        int height = 400
    ) 
    {
        Thread thread = new(() =>
        {
            wnd = new();
            wnd.Width = width;
            wnd.Height = height;
            wndInit?.Invoke(wnd);
            if(contentType != null)
            {
                if (!contentType.IsSubclassOf(typeof(FrameworkElement))) return;
                content = (FrameworkElement?)Activator.CreateInstance(contentType!, contentConstructorArgs);
                wnd.Content = content;
            }
            initialized = true;

            wnd.SourceInitialized += (s, e) =>
            {
                hWnd = new WindowInteropHelper(wnd).EnsureHandle();
            };

            wnd.Show();
            // make thread a "UI Thread" by starting the message pump
            System.Windows.Threading.Dispatcher.Run();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
    }

    // to get a reference to the window's primary content to write to it
	public FrameworkElement? GetContent()
	{
		while(!initialized) Thread.Sleep(1);
		return content;
	}

    // to set the properties of windows primary content because it is owned by a separate thread
	public void SetContentProperty(Action<FrameworkElement?> contentPropertySetterLambda)
	{
		while(!initialized) Thread.Sleep(1);
		wnd?.Dispatcher.Invoke(() => 
		{
            contentPropertySetterLambda(content);
        });
	}
}

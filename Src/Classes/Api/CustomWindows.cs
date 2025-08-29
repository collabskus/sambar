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
using System.Windows.Media;
using ScottPlot.WPF;

namespace sambar;

public partial class Api
{
    private void CustomWindowsInit()
    {

    }

    public (ThreadWindow, WpfPlot) CreateAudioVisualizer(
        Action<Window>? init = null,
        int width = 400,
        int height = 200 
    ) 
    {
        ThreadWindow threadWnd = new(init, width, height);
        threadWnd.Run(() =>
        {
            audioVisPlot = new();
            // WpfPlot does not automatically get its dimensions when inside a container like
            // stackpanel or border, therefore width and height must be set manually
            audioVisPlot.Height = 100;
            audioVisPlot.Width = 200;
        });
        Utils.HideWindowInAltTab(threadWnd.EnsureInitialized().hWnd);
        return (threadWnd, audioVisPlot);
    }
}

public class ThreadWindow
{
    public Window? wnd;
    public nint hWnd;
    public FrameworkElement? content;
    bool initialized = false;
    internal ThreadWindow(
        Action<Window>? init = null,
        int width = 800,
        int height = 400
    ) 
    {
        Thread thread = new(() =>
        {
            wnd = new();
            wnd.Width = width;
            wnd.Height = height;
            init?.Invoke(wnd);
            wnd.SourceInitialized += (s, e) =>
            {
                hWnd = new WindowInteropHelper(wnd).EnsureHandle();
                initialized = true;
            };

            wnd.Show();
            // make thread a "UI Thread" by starting the message pump
            System.Windows.Threading.Dispatcher.Run();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
    }

    public ThreadWindow EnsureInitialized()
    {
		while(!initialized) Thread.Sleep(1);
        return this;
    }

    public void Run(Action runLambda)
    {
		while(!initialized) Thread.Sleep(1);
        wnd?.Dispatcher.Invoke(() => 
        {
            runLambda();
        });
    }
}



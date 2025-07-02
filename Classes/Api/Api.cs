using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Windows;
using Interop.UIAutomationClient;

namespace sambar;

/// <summary>
/// The entire Api class provides core functionalities that a widget developer
/// can use in their plugins so that they dont have to reinvent the wheel.
/// </summary>
public partial class Api {

    public Api()
    {
        EventsInit();
        WindowingInit();
        SystemTrayInit();
        TaskbarInterceptorInit();
        ClockInit();
    }

    public static void Print(string text) {
        Debug.WriteLine(text); 
    }
    
    // so that widgets and scripts can use it
    public BarConfig config;
    
    // instance of mainWindow
    public Window barWindow; 

    // IUIAutomation
    CUIAutomation ui = new();
    
    // Get Api object
    public static Api GetInstance()
    {
        return Sambar.api;
    }
}


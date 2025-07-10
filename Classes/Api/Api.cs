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

    /// <summary>
    /// PS: DO NOT initialize fields in here or in the other partial definitions of this 
    /// class elsewhere. Always initialize them in the constructor.
    /// </summary>

    public Api()
    {
        EventsInit();
        ToggleTaskbarInit();
        WindowingInit();
        SystemTrayInit();
        TaskbarInterceptorInit();
        ClockInit();
        CountersInit();
        //Task.Run(GlazeInit);
    }

    public void Print(string text) {
        Debug.WriteLine(text); 
    }
    
    // so that widgets and scripts can use it
    public Config config;
    
    // instance of mainWindow
    public Sambar barWindow; 

    // IUIAutomation
    CUIAutomation ui = new();
}


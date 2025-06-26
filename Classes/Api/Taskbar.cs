namespace sambar;

public partial class Api
{

}
/// <summary>
/// An invisible window to intercept taskbar messages.
/// We create a window with the preexisting Taskbar class
/// "Shell_TrayWnd". Any messages intended for the real taskbar
/// will be captured by our invisible window
/// </summary>
public class TaskbarInterceptor
{
    public TaskbarInterceptor() { }
}

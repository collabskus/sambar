namespace sambar;

public partial class Api
{
    public void TaskbarAppsInit()
    {
        List<nint> hWndsInTaskbar = Utils.GetAllTaskbarWindows();
    }
}

public class TaskbarApp
{
    string exePath;
    string className;
    public TaskbarApp(nint hWnd)
    {
        exePath = Utils.GetExePathFromHWND(hWnd);
        className = Utils.GetClassNameFromHWND(hWnd);
    }
}

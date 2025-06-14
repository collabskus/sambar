using System.Diagnostics;
using Interop.UIAutomationClient;

namespace sambar;

public partial class Api
{
    public delegate void FocusChangedEventHandler(FocusChangedMessage msg);
    public static event FocusChangedEventHandler FOCUS_CHANGED_EVENT;
    class FocusChangedEventHandlerClass: IUIAutomationFocusChangedEventHandler
    {
        public void HandleFocusChangedEvent(IUIAutomationElement sender)
        {
            //Debug.WriteLine($"focusChangedEvent, name: {sender.CurrentName}, windowClass: {sender.CurrentClassName}, type: {sender.CurrentItemType}");
            FocusChangedMessage msg = new();
            msg.className = sender.CurrentClassName;
            msg.type = sender.CurrentItemType;
            msg.name = sender.CurrentName;
            msg.hWnd = sender.CurrentNativeWindowHandle;
            FOCUS_CHANGED_EVENT(msg);
        }
    }

    public delegate void StructureChangedEventHandler(StructureChangedMessage msg);
    public static event StructureChangedEventHandler STRUCTURE_CHANGED_EVENT;
    class StructureChangedEventHandlerClass : IUIAutomationStructureChangedEventHandler
    {
        public void HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
        {
            //Debug.WriteLine($"structureChangedEvent, senderName: {sender.CurrentName}, class: {sender.CurrentClassName}, hWnd: {sender.CurrentNativeWindowHandle}, changeType: {changeType}");
            StructureChangedMessage msg = new();
            msg.className = sender.CurrentClassName;
            msg.type = changeType;
            msg.name = sender.CurrentName;
            msg.hWnd = sender.CurrentNativeWindowHandle;
            STRUCTURE_CHANGED_EVENT(msg);
        }
    }

    public void EventsInit()
    {
        FOCUS_CHANGED_EVENT = (msg) => { };
        FocusChangedEventHandlerClass focusChangedHandlerObject = new();
        ui.AddFocusChangedEventHandler(null, focusChangedHandlerObject);

        STRUCTURE_CHANGED_EVENT = (msg) => { };
        StructureChangedEventHandlerClass structureChangedHandlerObject = new();
        ui.AddStructureChangedEventHandler(ui.GetRootElement(), TreeScope.TreeScope_Children, null, structureChangedHandlerObject);
    }
}

public class FocusChangedMessage
{
    public string name;
    public string className;
    public IntPtr hWnd;
    public string type;
}

public class StructureChangedMessage
{
    public string name;
    public string className;
    public IntPtr hWnd;
    public StructureChangeType type;
}

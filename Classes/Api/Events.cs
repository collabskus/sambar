using System.Diagnostics;
using Interop.UIAutomationClient;

namespace sambar;

public partial class Api
{
    public delegate void FocusChangedEventHandler(FocusChangedMessage className);
    public static event FocusChangedEventHandler FOCUS_CHANGED_EVENT;
    
    class FocusChangedEventHandlerClass: IUIAutomationFocusChangedEventHandler
    {
        public void HandleFocusChangedEvent(IUIAutomationElement sender)
        {
            Debug.WriteLine($"focusChangedEvent, name: {sender.CurrentName}, windowClass: {sender.CurrentClassName}, type: {sender.CurrentItemType}");
            FocusChangedMessage msg = new();
            msg.className = sender.CurrentClassName;
            msg.type = sender.CurrentItemType;
            msg.name = sender.CurrentName;
            FOCUS_CHANGED_EVENT(msg);
        }
    }

    public void EventsInit()
    {
        FOCUS_CHANGED_EVENT = (msg) => { };
        FocusChangedEventHandlerClass focusChangedHandlerObject = new();
        ui.AddFocusChangedEventHandler(null, focusChangedHandlerObject);
    }
}

public class FocusChangedMessage
{
    public string name;
    public string className;
    public string type;

}

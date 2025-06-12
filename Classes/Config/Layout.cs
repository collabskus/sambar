using System.Windows;

namespace sambar;
using System.Windows.Controls;

public class Layout
{
    public Panel Container;
    public Dictionary<string, Border> WidgetToContainerMap = new();
    public Layout()
    {
    }
}

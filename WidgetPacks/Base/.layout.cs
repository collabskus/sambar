class BaseLayout : Layout
{
    public BaseLayout()
    {
        StackPanel panel = new();
        panel.Orientation = Orientation.Horizontal;
        
        Border border1 = new();
        Border border2 = new();
        Border border3 = new();
        Border border4 = new();
        Border border5 = new();
        border1.VerticalAlignment = VerticalAlignment.Center;
        border5.VerticalAlignment = VerticalAlignment.Center;
        panel.Children.Add(border1);
        panel.Children.Add(border2);
        panel.Children.Add(border3);
        panel.Children.Add(border4);
        panel.Children.Add(border5);

        this.Container = panel;
        this.WidgetToContainerMap["Workspaces"] = border1;
        this.WidgetToContainerMap["Tray"] = border2;
        this.WidgetToContainerMap["NetworkManager"] = border3;
        this.WidgetToContainerMap["HideTaskbar"] = border4;
        this.WidgetToContainerMap["Clock"] = border5;
    }
}

//return new BaseLayout();
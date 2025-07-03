class BaseLayout : Layout
{
    public BaseLayout()
    {
        /// <summary>
        /// Build the layout using borders
        /// </summary>
        //StackPanel panel = new();
        //panel.Orientation = Orientation.Horizontal;
        //Border border1 = new();
        //Border border2 = new();
        //Border border3 = new();
        //Border border4 = new();
        //Border border5 = new();
        //Border border6 = new();
        //Border border7 = new();
        //border1.VerticalAlignment = VerticalAlignment.Center;
        //border5.VerticalAlignment = VerticalAlignment.Center;
        //panel.Children.Add(border1);
        //panel.Children.Add(border2);
        //panel.Children.Add(border3);
        //panel.Children.Add(border4);
        //panel.Children.Add(border5);
        //panel.Children.Add(border6);
        //panel.Children.Add(border7);

        /// <summary>
        /// Create borders for each widget 
        /// </summary>
        Border Workspaces = new();
        Border Tray = new();
        Border NetworkManager = new();
        Border HideTaskbar = new();
        Border Clock = new();
        Border ScribblePad = new();
        Border StartButton = new();
        
        /// <summary>
        /// Build the layout after choosing a layout type
        /// <summary>
        Grid grid = new();

        ColumnDefinition _col1 = new() { Width = new GridLength(1, GridUnitType.Star)};
        ColumnDefinition _col2 = new() { Width = new GridLength(1, GridUnitType.Star)};
        ColumnDefinition _col3 = new() { Width = new GridLength(1, GridUnitType.Star)};

        grid.ColumnDefinitions.Add(_col1);
        grid.ColumnDefinitions.Add(_col2);
        grid.ColumnDefinitions.Add(_col3);

        StackPanel col1 = new();
        StackPanel col2 = new();
        StackPanel col3 = new();
        col1.Orientation = Orientation.Horizontal;
        col2.Orientation = Orientation.Horizontal;
        col3.Orientation = Orientation.Horizontal;
        col1.VerticalAlignment = VerticalAlignment.Center;
        col2.VerticalAlignment = VerticalAlignment.Center;
        col3.VerticalAlignment = VerticalAlignment.Center;

        Grid.SetColumn(col1, 0);
        Grid.SetColumn(col2, 1);
        Grid.SetColumn(col3, 2);
        
        // col1
        col1.Children.Add(Workspaces);
        // col2
        col2.Children.Add(Clock);
        // col3
        col3.Children.Add(Tray);
        col3.Children.Add(NetworkManager);
        col3.Children.Add(HideTaskbar);
        col3.Children.Add(ScribblePad);
        col3.Children.Add(StartButton);

        /// <summary>
        /// Add all borders to the layout type [Grid || StackPanel]
        /// </summary>
        grid.Children.Add(col1);
        grid.Children.Add(col2);
        grid.Children.Add(col3);
        /// <summary>
        /// Set the finished layout type as the container
        /// <summary>
        //this.Container = panel;
        this.Container = grid;
        /// <summary>
        /// Mark the borders to widgets since each widget has a parent border
        /// </summary>
        this.WidgetToContainerMap["Workspaces"] = Workspaces;
        this.WidgetToContainerMap["Tray"] = Tray;
        this.WidgetToContainerMap["NetworkManager"] = NetworkManager;
        this.WidgetToContainerMap["HideTaskbar"] = HideTaskbar;
        this.WidgetToContainerMap["Clock"] = Clock;
        this.WidgetToContainerMap["ScribblePad"] = ScribblePad;
        this.WidgetToContainerMap["StartButton"] = StartButton;
    }
}

public class Workspaces : Widget
{
	List<Workspace> workspaces = new();
	public List<RoundedButton> buttons = new();

	/*
	 * Made public so that they can be edited in a mod file (Workspaces.mod.cs)
	 * */

	public int BUTTON_HEIGHT = Theme.BUTTON_HEIGHT;
	public int BUTTON_WIDTH = Theme.BUTTON_WIDTH;

	public Brush BUTTON_BACKGROUND = Theme.BUTTON_BACKGROUND;
	public Brush BUTTON_PRESSED_BACKGROUND = Theme.BUTTON_PRESSED_BACKGROUND;

	public Workspaces(WidgetEnv ENV) : base(ENV) { }
	public override void Init()
	{
		workspaces = Sambar.api.workspaces;

		//this.CornerRadius = Theme.WIDGET_CORNER_RADIUS;
		StackPanelWithGaps panel = new(Theme.WIDGET_GAP, workspaces.Count);
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;
		panel.ClipToBounds = true;
		panel.Height = Sambar.api.config.height;

		for (int i = 1; i <= workspaces.Count; i++)
		{
			RoundedButton btn = new();
			btn.Text = $"{i}";
			btn.FontFamily = Theme.FONT_FAMILY;
			btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
			btn.Width = Theme.BUTTON_WIDTH;
			btn.Height = Theme.BUTTON_HEIGHT;
			btn.BorderThickness = Theme.BUTTON_BORDER_THICKNESS;
			btn.BorderBrush = Theme.BUTTON_BORDER_COLOR;
			btn.Foreground = Theme.TEXT_COLOR;
			btn.HoverColor = Theme.BUTTON_HOVER_COLOR;
			btn.Background = Theme.BUTTON_BACKGROUND;
			btn.HoverEffect = true;
			btn.MouseDown += WorkspaceButtonClicked;
			buttons.Add(btn);
			panel.Add(btn);
		}
		Sambar.api.Print($"workspaces: {workspaces.Count}, buttons: {buttons.Count}, index: {Sambar.api.currentWorkspace.index}");
		if (buttons.Count > 0)
			buttons[Sambar.api.currentWorkspace.index].Background = BUTTON_PRESSED_BACKGROUND;
		Sambar.api.GLAZE_WORKSPACE_CHANGED += (workspace) =>
		{
			RedrawButtons(workspace.index);
		};
		this.Content = panel;
	}

	public void RedrawButtons(int index)
	{
		this.Thread.Invoke(() =>
		{
			foreach (var button in buttons)
			{
				button.Background = BUTTON_BACKGROUND;
			}
			buttons[index].Background = BUTTON_PRESSED_BACKGROUND;
			buttons[index].HoverEffect = false;
		});
	}

	// for updating Glaze when buttons pressed
	bool buttonRedrawing = false;
	public void WorkspaceButtonClicked(object? sender, RoutedEventArgs e)
	{
		buttonRedrawing = true;
		var btn = sender as RoundedButton;
		string clickedBtnName = Convert.ToString(btn.Text);
		Workspace clickedWorkspace = workspaces.Where(wksp => wksp.name == clickedBtnName).First();
		int clickedBtnIndex = clickedWorkspace.index;
		RedrawButtons(clickedBtnIndex);
		Task.Run(async () =>
		{
			await Sambar.api.ChangeWorkspace(clickedWorkspace);
			await Task.Delay(3000);
			buttonRedrawing = false;
		});
	}
}


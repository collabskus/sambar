public class TaskbarApps : Widget
{
	public TaskbarApps()
	{
		var apps = Sambar.api.GetTaskbarApps();
		Sambar.api.Print($"TASKBARAPPS: {apps.Count()}");
		StackPanel panel = new();
		panel.Orientation = Orientation.Horizontal;
		foreach (var app in apps)
		{
			RoundedButton btn = new();
			btn.Icon = app.icon;
			btn.Height = Theme.BUTTON_HEIGHT;
			btn.Width = Theme.BUTTON_WIDTH;
			btn.Margin = Theme.BUTTON_MARGIN;
			btn.HoverEffect = false;

			panel.Children.Add(btn);
		}
		this.Content = panel;
	}
}

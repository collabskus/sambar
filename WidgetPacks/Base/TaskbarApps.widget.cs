public class TaskbarApps : Widget
{
	StackPanel panel = new();
	public TaskbarApps()
	{
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;

		Sambar.api.TASKBAR_APPS_EVENT += (msg) => UpdateTaskbarApps(msg);
		this.Content = panel;
	}

	public void UpdateTaskbarApps(TaskbarAppsMessage msg)
	{
		this.Thread.Invoke(() =>
		{
			panel.Children.Clear();
			Sambar.api.Print($"[ WIDGET ] {msg.runningApps.Count()}");
			foreach (var app in msg.runningApps)
			{
				RoundedButton btn = new();
				btn.Icon = app.icon;
				btn.Height = Theme.BUTTON_HEIGHT;
				btn.Width = Theme.BUTTON_WIDTH;
				btn.Margin = new(0, 0, 10, 0);
				btn.HoverEffect = false;

				panel.Children.Add(btn);
			}
		});
	}
}

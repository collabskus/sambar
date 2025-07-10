public class TaskbarApps : Widget
{
	StackPanel panel = new();
	public TaskbarApps()
	{
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;

		Task.Run(async () =>
		{
			while (true)
			{
				UpdateTaskbarApps();
				await Task.Delay(500);
			}
		});
		this.Content = panel;
	}

	public void UpdateTaskbarApps()
	{
		var apps = Sambar.api.GetTaskbarApps();
		this.Thread.Invoke(() =>
		{
			panel.Children.Clear();
			foreach (var app in apps)
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

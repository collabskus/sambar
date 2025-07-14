public class TaskbarApps : Widget
{
	StackPanel panel = new();
	RunningApp focusedApp;
	public TaskbarApps()
	{
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;

		Sambar.api.TASKBAR_APPS_EVENT += (apps) => UpdateTaskbarApps(apps);
		Sambar.api.ACTIVE_WINDOW_CHANGED_EVENT += (app) => { focusedApp = app; };
		this.Content = panel;
	}

	public void UpdateTaskbarApps(List<RunningApp> apps)
	{
		this.Thread.Invoke(() =>
		{
			panel.Children.Clear();
			foreach (var app in apps)
			{
				RoundedButton btn = new();
				btn.Icon = app.icon;
				btn.Height = Sambar.api.config.height;
				btn.Width = Sambar.api.config.height;
				btn.IconHeight = Theme.BUTTON_HEIGHT;
				btn.IconWidth = Theme.BUTTON_WIDTH;
				btn.Margin = new(0, 0, 5, 0);
				btn.HoverEffect = false;
				btn.MouseDown += (s, e) =>
				{
					app.FocusWindow();
				};
				if (focusedApp?.hWnd == app.hWnd)
				{
					btn.Background = Theme.BUTTON_PRESSED_BACKGROUND;
					btn.BorderBrush = Utils.BrushFromHex("#22F803");
					btn.BorderThickness = new(0, 5, 0, 0);
				}

				panel.Children.Add(btn);
			}
		});
	}

}

public class TaskbarApps : Widget
{
	StackPanel panel = new();
	RunningApp focusedApp;
	List<RoundedButton> btns = new();

	public TaskbarApps(WidgetEnv ENV) : base(ENV)
	{
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;

		Sambar.api.TASKBAR_APPS_EVENT += UpdateTaskbarApps;
		Sambar.api.ACTIVE_WINDOW_CHANGED_EVENT += UpdateFocusedApp;
		this.Content = panel;
	}

	List<RunningApp> apps = new();
	public void UpdateTaskbarApps(List<RunningApp> apps)
	{
		Sambar.api.Print($"UpdateTaskbarApps fired!");
		this.Thread.Invoke(() =>
		{
			panel.Children.Clear();
			btns = new();
			foreach (var app in apps)
			{
				RoundedButton btn = new();
				btn.Id = app.hWnd.ToString();
				btn.Icon = app.icon;
				btn.Height = Sambar.api.config.height;
				btn.Width = Sambar.api.config.height;
				btn.IconHeight = Theme.BUTTON_HEIGHT;
				btn.IconWidth = Theme.BUTTON_WIDTH;
				btn.Margin = new(0, 0, 5, 0);
				btn.HoverEffect = false;
				List<MenuButton> menuItems = new()
				{
				   new("close")
				};
				menuItems.ForEach(item =>
				{
					item.MouseDown += (s, e) => app.Kill();
				});
				btn.MouseDown += (s, e) =>
				{
					switch (e.ChangedButton)
					{
						case MouseButton.Left:
							app.FocusWindow();
							break;
						case MouseButton.Right:
							Sambar.api.CreateContextMenu(menuItems);
							break;
					}

				};
				if (focusedApp?.hWnd == app.hWnd)
				{
					UpdateFocusedApp(app);
				}

				panel.Children.Add(btn);
				btns.Add(btn);
			}
		});
	}

	public void UpdateFocusedApp(RunningApp app)
	{
		focusedApp = app;
		this.Thread.Invoke(() =>
		{
			if (btns == null) return;
			foreach (var btn in btns)
			{
				if (btn.Id == app.hWnd.ToString())
				{
					btn.Background = Theme.BUTTON_PRESSED_BACKGROUND;
					btn.BorderBrush = Utils.BrushFromHex("#22F803");
					btn.BorderThickness = new(0, 5, 0, 0);
				}
				else
				{
					btn.Background = Theme.BUTTON_BACKGROUND;
					btn.BorderBrush = Theme.BUTTON_BORDER_COLOR;
					btn.BorderThickness = new(0);
				}
			}
		});
	}
}

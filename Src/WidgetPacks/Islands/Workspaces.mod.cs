return (dynamic workspaces, dynamic ENV) =>
{
	workspaces.Background = Utils.BrushFromHex("#1a1a1a");
	workspaces.CornerRadius = new CornerRadius(8);
	workspaces.BUTTON_PRESSED_BACKGROUND = Utils.BrushFromHex("#d856f5");
	foreach (var btn in workspaces.buttons)
	{
		btn.CornerRadius = new CornerRadius(10);
	}
};


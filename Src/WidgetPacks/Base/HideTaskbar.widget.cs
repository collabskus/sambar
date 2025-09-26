public class HideTaskbar : Widget
{
	public HideTaskbar(WidgetEnv ENV) : base(ENV)
	{
		Sambar.api.ToggleTaskbar();
	}
}

return (dynamic clock, dynamic ENV) =>
{
	clock.Background = Utils.BrushFromHex("#1a1a1a");
	clock.CornerRadius = new CornerRadius(8);
	clock.timeString = (Func<Time, string>)((time) =>
	{
		return $"{time.hours}:{time.minutes} {time.day} {time.monthName} {time.year}";
	});
};

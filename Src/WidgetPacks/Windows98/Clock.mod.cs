using System.Windows;
using System.Windows.Media;

return (dynamic clock) =>
{
	clock.textBlock.FontWeight = FontWeights.Bold;
	clock.textBlock.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Aliased);
	clock.textBlock.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
};


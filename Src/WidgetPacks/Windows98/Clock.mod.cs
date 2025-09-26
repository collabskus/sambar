/*
 * Mofidies the base clock widget (Base\Clock.widget.cs)
 * */

using System.Windows;
using System.Windows.Media;

return (dynamic clock) =>
{
	clock.textBlock.FontWeight = FontWeights.SemiBold;
	clock.textBlock.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Aliased);
	clock.textBlock.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
};


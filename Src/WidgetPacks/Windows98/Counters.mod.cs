/*
 * Mofidies the base clock widget (Base\Counters.widget.cs)
 * */

using System.Windows;
using System.Windows.Media;

return (dynamic counters) =>
{
	counters.textBlock.FontWeight = FontWeights.SemiBold;
	counters.textBlock.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Aliased);
	counters.textBlock.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
};

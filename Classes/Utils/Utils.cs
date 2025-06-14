using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpVectors.Dom.Stylesheets;

namespace sambar;

public partial class Utils
{
   	public static Brush BrushFromHex(string hexColorString) {
		if (hexColorString == "transparent") {
			return new SolidColorBrush(Colors.Transparent);	
		}
		System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hexColorString);
		return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
	} 

	public static List<string> GetStyleListFromUInt(uint styleUInt)
	{
		WindowStyles styles = (WindowStyles)styleUInt;
		List<string> styleList = new();
		foreach(WindowStyles style in Enum.GetValues(typeof(WindowStyles)))
		{
			if(styles.HasFlag(style))
			{
				styleList.Add(style.ToString());
			}
		}
		return styleList;
	}
}

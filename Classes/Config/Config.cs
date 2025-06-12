using Newtonsoft.Json;

namespace sambar;

public class BarConfig
{
	public int height = 40;
	public int width = 0;
	public int marginXLeft = 10;
	public int marginXRight = 10;
	public int marginYTop = 10;
	public int paddingXLeft = 0;
	public int paddingXRight = 0;
	public int paddingYTop = 0;
	public int paddingYDown = 0;
	public string backgroundColor = "";
	public bool roundedCorners = true;
	public string borderColor = "";
	public int borderThickness = 0;
	public string widgetPack = "Base";

	[JsonConstructor]
	public BarConfig(
		int? height,
		int? width,
		int? marginXLeft,
		int? marginXRight,
		int? marginYTop,
		int? paddingXLeft,
		int? paddingXRight,
		int? paddingYTop,
		int? paddingYDown,
		string? backgroundColor,
		bool? roundedCorners,
		string? borderColor,
		int? borderThickness,
		string? widgetPack
	) {

		this.height = height ?? 40;
		this.width = width ?? 0;
		this.marginXLeft = marginXLeft ?? 10;
		this.marginXRight = marginXRight ?? 10;
		this.marginYTop = marginYTop ?? 10;
		this.paddingXLeft = paddingXLeft ?? 0;
		this.paddingXRight = paddingXRight ?? 0;
		this.paddingYTop = paddingYTop ?? 0;
		this.paddingYDown = paddingYDown ?? 0;
		this.backgroundColor = backgroundColor ?? "#ffffff";
		this.borderColor = borderColor ?? "#ffffff";
		this.borderThickness = borderThickness ?? 0;
		this.widgetPack = widgetPack ?? "Base";
	}

	public BarConfig(int screenWidth) {
		width = screenWidth - (marginXLeft + marginXRight);	
	}

	public BarConfig() { }
}



public class BaseConfig : Config
{
	public BaseConfig()
	{
		this.height = 25;
		this.width = 0;
		this.marginXLeft = 10;
		this.marginXRight = 10;
		this.marginYTop = 10;
		this.paddingXLeft = 0;
		this.paddingXRight = 0;
		this.paddingYTop = 0;
		this.paddingYDown = 0;
		this.backgroundColor = "transparent";
		this.borderColor = "#ffffff";
		this.borderThickness = new(0);
		this.roundedCorners = false;
	}
}

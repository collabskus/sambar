public class BaseConfig : Config
{
	public BaseConfig()
	{
		this.height = 20;
		this.width = 0;
		this.marginXLeft = 0;
		this.marginXRight = 0;
		this.marginYTop = 0;
		this.paddingXLeft = 0;
		this.paddingXRight = 0;
		this.paddingYTop = 0;
		this.paddingYDown = 0;
		this.backgroundColor = "transparent";
		this.borderColor = "transparent";
		this.borderThickness = new(0);
		this.roundedCorners = false;
	}
}

using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpVectors;
using SharpVectors.Converters;
using Brush = System.Windows.Media.Brush;
using Image = System.Windows.Controls.Image;

namespace sambar;

public class RoundedButton : UserControl
{
	Border RoundedButtonBorder = new();
	TextBlock RoundedButtonTextBlock = new();
	Image RoundedButtonImage = new();
	BitmapImage bi = new();
	SvgViewbox RoundedButtonSvgImage = new();
	public string Text
	{
		get { return this.RoundedButtonTextBlock.Text; }
		set 
		{ 
			this.RoundedButtonTextBlock.Text = value;
			this.RoundedButtonBorder.Child = RoundedButtonTextBlock;
		}
	}

	private string ImageType;
	public string ImageSrc
	{
		get { return bi.UriSource.AbsoluteUri; }
		set
		{
			if 
			(
				value.EndsWith(".jpg") || 
				value.EndsWith(".png") || 
				value.EndsWith(".ico"))
			{
				ImageType = new string(value.TakeLast(4).ToArray());

				bi = new();
				bi.BeginInit();
				bi.UriSource = new Uri(value);
				bi.EndInit();

				RoundedButtonImage = new();
				RoundedButtonImage.Source = bi;
				this.RoundedButtonBorder.Child = RoundedButtonImage;
			}
			else if(value.EndsWith(".svg"))
			{
				ImageType = ".svg";
				RoundedButtonSvgImage = new();
				RoundedButtonSvgImage.Source = new Uri(value);
				this.RoundedButtonBorder.Child = RoundedButtonSvgImage;
			}
		}
	}

	public BitmapImage BitmapIcon
	{
		get
		{
			return bi;
		}
		set
		{
			RoundedButtonImage = new();
			RoundedButtonImage.Source = bi = value;
			RoundedButtonBorder.Child = RoundedButtonImage;
		}
	}

	public int IconWidth
	{
		get { 
			if(
				ImageType == ".jpg" ||
				ImageType == ".png" ||
				ImageType == ".ico"
			)
			{
                return (int)this.RoundedButtonImage.Width; 
			}
			else if(ImageType == ".svg") {
				return (int)this.RoundedButtonSvgImage.Width;
			}
			return 0;
		}
		set
		{
			if(
				ImageType == ".jpg" ||
				ImageType == ".png" ||
				ImageType == ".ico" 
			)
			{
                this.RoundedButtonImage.Width = value;
			}
			else if(ImageType == ".svg")
			{
				this.RoundedButtonSvgImage.Width = value;
			}
		}
	}

    public int IconHeight
    {
        get
        {
            if (
                ImageType == ".jpg" ||
                ImageType == ".png" ||
                ImageType == ".ico"
            )
            {
                return (int)this.RoundedButtonImage.Height;
            }
            else if (ImageType == ".svg")
            {
                return (int)this.RoundedButtonSvgImage.Height;
            }
            return 0;
        }
        set
        {
            if (
                ImageType == ".jpg" ||
                ImageType == ".png" ||
                ImageType == ".ico"
            )
            {
                this.RoundedButtonImage.Height= value;
            }
            else if (ImageType == ".svg")
            {
                this.RoundedButtonSvgImage.Height = value;
            }
        }
    }


    public CornerRadius CornerRadius
	{
		get { return this.RoundedButtonBorder.CornerRadius; }
		set { this.RoundedButtonBorder.CornerRadius = value; }
	}

	public Brush Background
	{
		get { return this.RoundedButtonBorder.Background; }
		set { this.RoundedButtonBorder.Background = value; }
	}

	public Brush Foreground {
		get { return this.RoundedButtonTextBlock.Foreground; }
		set { this.RoundedButtonTextBlock.Foreground = value;  }
	}

	public Brush BorderBrush {
		get { return this.RoundedButtonBorder.BorderBrush; }	
		set { this.RoundedButtonBorder.BorderBrush = value; }
	}

	public Thickness BorderThickness
	{
		get { return this.RoundedButtonBorder.BorderThickness; }
		set { this.RoundedButtonBorder.BorderThickness = value; }
	}
	
	public Brush HoverColor = new SolidColorBrush(Colors.Black);

	public RoundedButton()
	{
        this.Content = RoundedButtonBorder;
		this.MouseEnter += MouseHoverHandler;
		this.MouseLeave += MouseHoverHandler;

		RoundedButtonTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
		RoundedButtonTextBlock.VerticalAlignment = VerticalAlignment.Center;

	}

	public void MouseHoverHandler(object sender, MouseEventArgs e) {
		Brush buffer = HoverColor;
		HoverColor = Background;
		Background = buffer;
	}
}

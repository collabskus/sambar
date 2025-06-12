using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace sambar;

public class RoundedButton : UserControl
{
	Border RoundedButtonBorder = new();
	TextBlock RoundedButtonTextBlock = new();
	Image RoundedButtonImage = new();
	BitmapImage bi = new();
	public string Text
	{
		get { return this.RoundedButtonTextBlock.Text; }
		set 
		{ 
			this.RoundedButtonTextBlock.Text = value;
			this.RoundedButtonBorder.Child = RoundedButtonTextBlock;
		}
	}

	public string ImageSrc
	{
		get { return bi.UriSource.AbsoluteUri; }
		set
		{
			bi = new();
			bi.BeginInit();
			bi.UriSource = new Uri(value);	
			bi.EndInit();

			RoundedButtonImage.Source = bi;
			this.RoundedButtonBorder.Child = RoundedButtonImage;
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

public class Wallpapers : Widget
{
	public RoundedButton btn = new();

	public Wallpapers(WidgetEnv ENV) : base(ENV)
	{
		btn.Height = Theme.BUTTON_HEIGHT;
		btn.Width = Theme.BUTTON_WIDTH;
		btn.Margin = Theme.BUTTON_MARGIN;
		if (!ENV.IS_IMPORTED) btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, "image.svg");
		else btn.ImageSrc = Path.Join(ENV.IMPORTS_ASSETS_FOLDER, "image.svg");
		btn.IconWidth = 13;
		btn.IconHeight = 13;
		btn.Background = Theme.BUTTON_BACKGROUND;
		btn.CornerRadius = Theme.BUTTON_CORNER_RADIUS;
		btn.MouseDown += ButtonMouseDown;
		this.Content = btn;
	}
	public void ButtonMouseDown(object? sender, MouseEventArgs e)
	{
		sambar.Menu menu = Sambar.api.CreateMenu(0, 0, 500, 300, centerOffset: true);

		string wallpapersFolder = Path.Join(ENV.HOME, "Pictures", "Wallpapers");
		string[] walls = Directory.GetFiles(wallpapersFolder).Where(path => path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".jpeg")).ToArray();

		ImageSelector imageSelector = new();
		Task.Run(() =>
			this.Thread.Invoke(() =>
				imageSelector.Load(walls)
			)
		);
		imageSelector.IMAGE_SELECTED += (imgFile) => Sambar.api.SetWallpaper(imgFile, WallpaperAnimation.NONE);
		//imageSelector.IMAGE_SELECTED += (imgFile) => Sambar.api.SetWallpaper(imgFile);

		menu.KeyDown += (s, e) => { if (e.Key == Key.Escape) menu.Close(); };
		menu.Content = imageSelector;
	}
}

public class ImageSelector : Grid
{
	Image img1 = new();
	Image img2 = new();
	Image img3 = new();

	Border border1 = new() { Padding = new(10) };
	Border border2 = new() { Padding = new(10), BorderBrush = new SolidColorBrush(Colors.Green), BorderThickness = new(2), VerticalAlignment = VerticalAlignment.Center, CornerRadius = new(5) };
	Border border3 = new() { Padding = new(10) };

	string[] imgFiles = [];
	int index = 0;
	public delegate void ImageSelectedHandler(string imgFile);
	public event ImageSelectedHandler IMAGE_SELECTED = (imgFile) => { };
	public ImageSelector()
	{
		ColumnDefinition col1 = new() { Width = new GridLength(1, GridUnitType.Star) };
		ColumnDefinition col2 = new() { Width = new GridLength(1.5, GridUnitType.Star) };
		ColumnDefinition col3 = new() { Width = new GridLength(1, GridUnitType.Star) };

		this.ColumnDefinitions.Add(col1);
		this.ColumnDefinitions.Add(col2);
		this.ColumnDefinitions.Add(col3);

		border1.Child = img1;
		border2.Child = img2;
		border3.Child = img3;

		this.Children.Add(border1);
		this.Children.Add(border2);
		this.Children.Add(border3);

		Grid.SetColumn(border1, 0);
		Grid.SetColumn(border2, 1);
		Grid.SetColumn(border3, 2);

		this.Focusable = true;
		this.Loaded += (s, e) =>
		{
			this.Focus();
		};

		this.KeyDown += (s, e) =>
		{
			switch (e.Key)
			{
				case Key.L or Key.Right:
					Forward();
					break;
				case Key.H or Key.Left:
					Backward();
					break;
				case Key.Enter:
					IMAGE_SELECTED(this.imgFiles[index]);
					break;
			}
			Sambar.api.Print($"key pressed: {e.Key.ToString()}, index: {index}");
		};
	}

	public ImageSelector(string[] imgFiles)
	{
		Load(imgFiles);
	}

	public void Load(string[] imgFiles)
	{
		this.imgFiles = imgFiles;
		SetState(index);
	}

	public void Forward()
	{
		if (index == this.imgFiles.Length - 1) return;
		index++;
		SetState(index);
	}
	public void Backward()
	{
		if (index == 0) return;
		index--;
		SetState(index);
	}

	public void SetState(int i)
	{
		if (index == 0)
		{
			img1.Source = null;
			SetImageSource(img2, imgFiles[i]);
			SetImageSource(img3, imgFiles[i + 1]);
		}
		else if (index == imgFiles.Length - 1)
		{
			SetImageSource(img2, imgFiles[i]);
			SetImageSource(img3, imgFiles[i + 1]);
			img3.Source = null;
		}
		else
		{
			SetImageSource(img1, imgFiles[i - 1]);
			SetImageSource(img2, imgFiles[i]);
			SetImageSource(img3, imgFiles[i + 1]);
		}
	}

	public void SetImageSource(Image img, string imgFile)
	{
		img.Source = Sambar.api.GetImageSource(imgFile);
	}
}


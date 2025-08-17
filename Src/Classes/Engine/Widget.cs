/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace sambar;

public class Widget : Border
{
	public int index;

	public UIElement Content
	{
		get { return this.Child; }
		set { this.Child = value; }
	}

	public Dispatcher Thread
	{
		get { return this.Dispatcher; }
	}

	Config config;
	public Widget()
	{
		this.HorizontalAlignment = HorizontalAlignment.Left;
		config = Sambar.api.config;
	}
}

# Creating a widget

Inorder to create a new widget for your new widget pack, follow along !

 1. Inside your widgetpack folder (`WidgetPacks\<MyWidgetPack>`) create a file named, `MyNewWidget.widget.cs`.
 2. Create your widget class names `MyNewWidget` which inherits from the base `Widget` class.
 
 this is what it looks like now:

```cs
class MyNewWidget: Widget 
{
    public MyNewWidget() 
    {

    }
}
```

Now all widgets upon being instantiated by the [`WidgetLoader`](https://github.com/TheAjaykrishnanR/sambar/blob/29edadad7b02062c92393803413cef43dcc99755/Src/Classes/Engine/WidgetEngine.cs#L30) recieves an environment variable `ENV` of type [`WidgetEnv`](https://github.com/TheAjaykrishnanR/sambar/blob/29edadad7b02062c92393803413cef43dcc99755/Src/Classes/Engine/Widget.cs#L38). This is done so that the widget can know and access certain information that might only be available at runtime. So pass this to the constructor of your new widget as follows:

```cs
class MyNewWidget: Widget 
{
    public MyNewWidget(WidgetEnv ENV) 
    {

    }
}
```

you know what ? lets just even initialize the base constructor with `ENV`:

```cs
class MyNewWidget: Widget 
{
    public MyNewWidget(WidgetEnv ENV): base(ENV)
    {

    }
}
```

Now thats all done, lets actuall go back to creating our widget. You can literally do anything you want in here at this point, but for illustration lets write a simple widget that displays the text "Hello, World!" on the bar. Do the following:

```cs
class MyNewWidget: Widget 
{
    public MyNewWidget(WidgetEnv ENV): base(ENV)
    {
        TextBock textBlock = new();
        textBlock.Text = "Hello, World!";
    }
}
```

Now we have created the `TextBlock` but we still need to set the text block as the content of our widget, so do:

```cs
class MyNewWidget: Widget 
{
    public MyNewWidget(WidgetEnv ENV): base(ENV)
    {
        TextBock textBlock = new();
        textBlock.Text = "Hello, World!";
        // set the widget content as our textblock
        this.Content = textBlock;
    }
}
```

Now thats it ! Provided that `MyNewWidget` has been added in the `.layout.cs` to specify where the widget should be on the bar, you should see "Hello, World!" next time you launch sambar.



# Layout

## Creating a layout

 1. In the `WidgetPack` of your choice create a `.layout.cs` file.
 2. A layout class must inherit from the base `Layout` class.

```cs
class MyNewLayout: Layout 
{
    public MyNewLayout() 
    {

    }
}
```

Now what is a layout ? Its like a blueprint right where you lay out a plan depicting where your widgets are finally going to appear on the bar. So first, like in all plans you have to create placeholders demarking positions for each of your widgets. These placeholders would then be arranged in the way you want them to be, and thats your layout. The `WidgetEngine` when processing the Layout object would fill these placeholders with the unique widgets they were designated with.

Seems pretty straightforward, right ?

Okay then, the placeholders are `Border`s in WPF. So for each widget you have to create a field in the Layout class thats of type `Border`. Lets say that I have two widgets named `MyWidget1` and `MyWidget2`, then it would look like this:

```cs
class MyNewLayout: Layout 
{
    // placeholders
    Border MyWidget1;
    Border MyWidget2;

    public MyNewLayout() 
    {

    }
}
```

Now you can arange these placeholders as if they were the actual widgets and in the end they would be filled with their respective widgets.

**PS: The field's name must match exactly as that of their widget's (the widget's class to be precise)**

Now who do you arrange these placeholders ?

First you will have to choose a master layout for the whole container, like `Grid` or `StackPanel` you know, the usual kinds we have in WPF. Once you have done that you must add your placeholder borders to these containers. For simplicity i will choose a simply horizontally oriented stackpanel as my container.

```cs
class MyNewLayout: Layout 
{
    // placeholders
    Border MyWidget1;
    Border MyWidget2;

    public MyNewLayout() 
    {
        StackPanel panel = new();
        this.Container = panel;
    }
}
```

Now just add those placeholders to the panel which is my container.

```cs
class MyNewLayout: Layout 
{
    // placeholders
    Border MyWidget1;
    Border MyWidget2;

    public MyNewLayout() 
    {
        StackPanel panel= new();
        panel.Children.Add(MyWidget1);
        panel.Children.Add(MyWidget2);
        this.Container = panel;
    }
}
```

And just like that your layout is finished.


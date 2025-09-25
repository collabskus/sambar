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

### Things to note about widgets

Since widgets are `UIElements` that are added to the sambar they are ran on the main UIThread.
This is necessary because WPF doesnt allow UI updation or even setting UIElements that arent owned/created on the main UIThread. Therefore it is totally possible to crash sambar from widgets.

On hindsight this is a good thing since you precisely know what went wrong with your configuration or the action performed that led to the crash, but it can be annoying. Also it doesnt make sense to crash the entire bar just because one `Widget` had an `ArrayOutOfBounds` exception.

Therefore to meet in the middle all methods of the `Widget` class will get automatically wrapped in a `try catch` block so that one widget crashing does not crash the other widgets. Also the widget is instantiated in a similar `try catch` block so that no funny things in its constructor can cause an abrupt crash. Any exceptions on these methods will be logged by the logger and a visual menu will be displayed that can be closed showing the exception message.

It is still possible to crash the entire thing from a widget if you do something like subscribe to an event using an inline function that is not a method of your widget. This is because only class methods are automatically wrapped with try catch blocks not the anonymous lambdas.

```cs
public MyWidget() {
    MY_EVENT += (arg) => {
        /* do stuff */
    }
}
```
instrad of the above, create a method and subscribe

```cs
public MyWidget() {
    MY_EVENT += MyEventHandler;
}
void MyEventHandler(string arg) {
    /* do stuff */
}
```

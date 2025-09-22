# Configuration

Code and configuration are the same. Everything configurable in Sambar are present in `.cs` files in the `WidgetPacks` folder. There are no `json` or `yaml` files. At its core sambar exposes an api and is a script runner. With this approach it is possible to customize sambar to its fullest extend. You are not limited to the customization thats exposed by a widget, instead you can customize the widget yourself.

## Directory Structure

```
   WidgetPacks
   ├───Base
   │   └───assets
   │   .config.cs
   │   .layout.cs
   │   .theme.cs
   │   <-->.widget.cs
   └───Plain1
       .config.cs
       .layout.cs
       .imports.cs
       .theme.cs
   .init.cs
```

A widgetpack is a the fundamental unit composed of a collection of widgets and configuration files necessary for a fully functional bar with a certain style/theme.

Each widget pack contains a set of widgets (ending with `.widget.cs` extensions) and a set of configuration files (or dotfiles): `.config.cs`, `.layout.cs`, `.theme.cs`

The root WidgetPacks folder contains an `.init.cs` file. Sambar will use the widgetpack returned from this file.

Lets have a look at these files in depth: 

### `.config.cs` : 

Configure the primary properties of the bar window that are necessary during launch, these (and their default values) are:

```
int height = 40;
int width = 0;
int marginXLeft = 10;
int marginXRight = 10;
int marginYTop = 10;
int paddingXLeft = 0;
int paddingXRight = 0;
int paddingYTop = 0;
int paddingYDown = 0;
string backgroundColor = "";
bool roundedCorners = true;
string borderColor = "";
int borderThickness = 0;
string widgetPack = "Base";
```

### `.layout.cs`: 

This file basically defines the layout of all the widgets in the sambar bar. Their positioning, sizes, everything. Refer to [constructing the widgets layout]() for a detailed look into creating or changing the layout of sambar.

### `.theme.cs`: 

This file contains a static class called `Theme` which can hold global variables for common use by your widgets. Say `TEXT_COLOR` for example and you need a common text color in all your widgetsfor creating a particular theme. Then you can make that as a variable in here. They can be called from the widgets easily as `Theme.TEXT_COLOR`.

### `.init.cs`:

contains a single return statement

```
return "<WIDGETPACKNAME>";
```

### `imports.cs`:

Notice in `Plain1` widgetpack there's another file called `.imports.cs`. Well `Plain1` is just a slight modification of the `Base` widgetpack and therefore "imports" the default widgets from BAse without any modification to the widgets themselves. This is so that you dont have to copy unmodified `.widget.cs` files if all you need is to have them as it is in your new widgetpack/theme.



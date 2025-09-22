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
       .theme.cs
   .init.cs
```

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

### `.theme.cs`: 

### `.init.cs`:



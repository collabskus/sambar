# Basics

## Downloading

Download the latest release from [releases]()

## Launching

Launch the sambar by double clicking the `sambar.exe` executable. If you want to see the live debug output, run the executable from a console such as powershell or cmd.

## Logging

The logfile `sambar.log` can be found alongside the executable which will be created after the first run.

## Exiting

You can kill `sambar.exe` from the task manager or by running `taskkill /IM sambar.exe` from cmd/pwsh. Even while running the terminal, since the bar window runs on its own UI thread the `Main()` returns immediately, so `Ctrl-C` wont work to kill sambar.

# Widgets

Widgets are organized into a `Pack`. They are a collection of widgets. You can see them in the `WidgetPacks` folder. `sambar.exe` looks for this folder in the location where the binary is located. A basic set of widgets have been already written for you in the `Base` widget pack. You can include them or copy and modify them to create your own widget packs.

Each widget pack is folder in the `WidgetPacks` folder. To create your own widget pack, first create a folder in `WidgetPacks` and give it the name you want. You will notice that in the `widgetPacks` folder there is a file called `.init.cs`. Sambar will look for this file in order to determine which widgetPack you want to use. So inorder to use the widget pack you have created, change the `return "Base"` in `init.cs` to `return "<YOUR WIDGETPACKNAME>"` where the `WIDGETPACKNAME` is the folder name you specified before.

But your newly created widgetpack is empty. So you need to create some widgets. If your merely want to modify existing widgets, modifying them in `Base` is enough. But if you want to create a new layout or a layout with different colors, its better to create a new widget pack.

To start hacking with sambar have a go at [Tutorials](https://github.com/TheAjaykrishnanR/sambar/blob/master/Docs/Tutorials/Landing.md)

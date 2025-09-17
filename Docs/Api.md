# Api Reference

The entire Sambar api exists as a single partial class thats made available to plugins as an instance of this class.
When Sambar initializes this instance is set to the variable `Sambar.api`. The Api class is spread out in all the files 
in the [Api folder](https://github.com/TheAjaykrishnanR/sambar/tree/master/Src/Classes/Api). So you can read through all
the public functions and events defined in those files and basically thats all the functionality exposed through the api.
Much of the Api is served as events that you can subscribe to from your plugins.

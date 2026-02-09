# What are Essentials Plugins?

**Note : this entry updates a deprecated method - for information related to that deprecated method, see [Plugins - Deprecated](~/docs/Plugins-Deprecated.md)**

***
* [YouTube Video - Loading Plugins in PepperDash Essentials](https://youtu.be/NA64iyNNAgE)
* [YouTube Video - Build Your Own Plugin, Part 1](https://youtu.be/m2phC8g3Kfk)
* [YouTube Video - Build Your Own Plugin, Part 2](https://youtu.be/2_PrWRk6Gy0)
***

Plugins are SIMPL# Pro libraries that reference the Essentials Framework and can be loaded into an Essentials Application at runtime to extend functionality beyond what the Essentials Framework provides on its own.

## Why Use Plugins?

Plugins are a way to extend or add new functionality to the Essentials Application without having to modify the actual Framework. In most cases, a plugin can be written to support a new device or behavior. Using plugins also limits the scope of understanding needed to work within the Essentials Framework.

## Should I use a Plugin?

Essentials is meant to be a lightweight framework and an extensible basis for development. While some devices are included in the framework, mostly for the purposes of providing examples and developing and prototyping new device types, the bulk of new development is intended to take place in Plugins. Once a plugin adds new functionality that may be of benefit if shared across multiple plugins, it may make sense to port that common logic (base classes and/or interfaces) back into the framework to make it available to others. The thrust of future Essentials development is targeted towards building a library of plugins.

## How do Plugins Work?

One or more plugins can be loaded to the /user/ProgramX/plugins as .dlls or .cplz packages. When the Essentials Application starts, it looks for any .cplz files, unzips them and then iterates any .dll assemblies in that folder and loads them. Once the plugin assemblies are loaded the Essentials Application will then attempt to load a configuration file and construct items as defined in the file. Those items can be defined in either the Essentials Framework or in any of the loaded plugin assemblies.

![Architecture drawing](~/docs/images/Plugin%20Load%20Sequence.png)

## What Must be Implemented in a Plugin for it to Work?

All plugin assemblies must contain a class which inherits from ```EssentialsPluginDeviceFactory<T>```, where ```<T>``` is a class which inherits from ```PepperDash.Essentials.Core.EssentialsDevice```

Within this class, we will define some metadata for the plugin and define which constructor to use
for instantiating each class as defined by type.

Note that multiple types can be loaded from the same plugin.

```cs
using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;
using System.Collections.Generic;

namespace MyPlugin
{
    public class SomeDeviceFactory : EssentialsPluginDeviceFactory<SomeDevice>
    {
        // This method defines metadata for the devices in the plugin
        public SomeDeviceFactory()
        {
            // This string is used to define the minimum version of the
            // Essentials Framework required for this plugin
            MinimumEssentialsFrameworkVersion = "1.5.0";

            // This string is used to define all valid type names for
            // this plugin.  This string is case insensitive
            TypeNames = new List<string> { "SomeDevice" , "ThisNewDevice" };
        }

        // This method instantiates new devices for the defined type
        // within the plugin
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            // Deserialize the json properties for the loaded type to a new object
            var props = dc.Properties.ToObject<SomeDeviceProperties>();

            // Return a newly instantiated device using the desired constructor
            // If no valid property data exists, return null with console print
            // describing the failure.
            if (props == null)
            {
                Debug.Console(0, "No valid property data for 'SomeDevice' - Verify Configuration.");
                Debug.Console(0, dc.Properties.ToString());
                return null;
            }
            return new SomeDevice(dc.Key, dc.Name, props);
        }
    }

    public class OtherDeviceFactory : EssentialsPluginDeviceFactory<OtherDevice>
    {
        // This method defines metadata for the devices in the plugin
        public OtherDeviceFactory()
        {
            // This string is used to define the minimum version of the
            // Essentials Framework required for this plugin
            MinimumEssentialsFrameworkVersion = "1.5.0";

            // This string is used to define all valid type names for
            // this plugin.  This string is case insensitive
            TypeNames = new List<string> { "OtherDevice", "ThisOtherDevice" };
        }

        // This method instantiates new devices for the defined type
        // within the plugin
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            // Deserialize the json properties for the loaded type to a new object
            var props = dc.Properties.ToObject<OtherDeviceProperties>();

            // Return a newly instantiated device using the desired constructor
            // If no valid property data exists, return null with console print
            // describing the failure.
            if (props == null)
            {
                Debug.Console(0, "No valid property data for 'OtherDevice' - Verify Configuration.");
                Debug.Console(0, dc.Properties.ToString());
                return null;
            }
            return new OtherDevice(dc.Key, dc.Name, props);
        }
    }
}
```

## SIMPL Bridging

Optionally, if your plugin device needs to be able to bridge to a SIMPL program over EISC, and there isn't already an existing bridge class in the Essentials Framework, you can write a new bridge class in your plugin. However, in order for the Essentials Application to be able to us that new bridge, the bridge must implement the ```IBridgeAdvanced``` interface with the required ```LinkToApi()``` Extension method.

If you are writing code for a bridgeable device, you should be inheriting from ```EssentialsBridgeableDevice```, which inherits from the already-required ```EssentialsDevice``` and implements ```IBridgeAdvanced```.

Often though, you may find that a bridge class already exists in the Essentials Framework that you can leverage. For example, if you were writing a plugin to support a new display model that isn't already in the Essentials Framework, you would define a class in your plugin that inherits from PepperDash.Essentials.Core.DisplayBase. If you're only implementing the standard display control functions such as power/input/volume control, then the existing bridge class `DisplayControllerBridge` can be used. If you needed to add additional functions to the bridge, then you would need to write your own bridge in the plugin.

For additional info see the [SIMPL-Bridging article](~/docs/SIMPL-Bridging.md).

## Template Essentials Plugin Repository

Fork this repository when starting a new plugin. The template repository uses the essentials-builds repository as a submodule. This allows the plugin to reference a specific build version of Essentials. You must make sure that you checkout the correct build of the Essentials-Builds repo that contains any dependencies that your plugin may rely on.

[Essentials Plugin Template Repository](https://github.com/PepperDash/EssentialsPluginTemplate)

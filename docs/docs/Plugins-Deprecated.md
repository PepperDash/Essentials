# Deprecated

**Note : this entry is out of date - please see [Plugins](~/docs/technical-docs/Plugins.md)**

## What are Essentials Plugins?

Plugins are SIMPL# Pro libraries that reference the Essentials Framework and can be loaded into an Essentials Application at runtime to extend functionality beyond what the Essentials Framework provides on its own.

## Why Use Plugins?

Plugins are a way to extend or add new functionality to the Essentials Application without having to modify the actual Framework. In most cases, a plugin can be written to support a new device or behavior. Using plugins also limits the scope of understanding needed to work within the Essentials Framework.

## Should I use a Plugin?

Essentials is meant to be a lightweight framework and an extensible basis for development. While some devices are included in the framework, mostly for the purposes of providing examples and developing and prototyping new device types, the bulk of new development is intended to take place in Plugins. Once a plugin adds new functionality that may be of benefit if shared across multiple plugins, it may make sense to port that common logic (base classes and/or interfaces) back into the framework to make it available to others. The thrust of future Essentials development is targeted towards building a library of plugins.

## How do Plugins Work?

One or more plugins can be loaded to the /user/ProgramX/plugins as .dlls or .cplz packages. When the Essentials Application starts, it looks for any .cplz files, unzips them and then iterates any .dll assemblies in that folder and loads them. Once the plugin assemblies are loaded the Essentials Application will then attempt to load a configuration file and construct items as defined in the file. Those items can be defined in either the Essentials Framework or in any of the loaded plugin assemblies.

![Architecture drawing](~/docs/images/Plugin%20Load%20Sequence.png)

## What Must be Implemented in a Plugin for it to Work?

All plugin assemblies must contain a static method called LoadPlugin():

```cs
public class SomeDevice : Device , IBridge  //IBridge only needs to be implemented if using a bridge
{
    // This string is used to define the minimum version of the
    // Essentials Framework required for this plugin
    public static string MinimumEssentialsFrameworkVersion = "1.4.23";

    // This method gets called by the Essentials Framework when the application starts.
    // It is intended to be used to load the new Device type(s) specified in the plugin
    public static void LoadPlugin()
    {
        DeviceFactory.AddFactoryForType("typeName", FactoryMethod);
        // typeName should be the unique string that identifies the type of device to build,
        // FactoryMethod represents the method that takes a DevicConfig object as and argument
        // and returns an instance of the desired device type
    }

    // This is a factory method to construct a device and return it to be
    // added to the DeviceManager
    public static Device FactoryMethod(DeviceConfig dc)
    {
        return new SomeDevice(dc.key, dc.name, dc);
    }

#region IBridge
    // This method is called by an EiscApi bridge instance and should call an extension method
    // defined in your plugin.  Required for implementing IBridge
    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey)
    {
        this.LinkToApiExt(trilist, joinStart, joinMapKey);
    }
#endregion
}
```

## SIMPL Bridging

Optionally, if your plugin device needs to be able to bridge to a SIMPL program over EISC, and there isn't already an existing bridge class in the Essentials Framework, you can write a new bridge class in your plugin. However, in order for the Essentials Application to be able to us that new bridge, the bridge must implement the IBridge interface with the required LinkToApi() Extension method.

Often though, you may find that a bridge class already exists in the Essentials Framework that you can leverage. For example, if you were writing a plugin to support a new display model that isn't already in the Essentials Framework, you would define a class in your plugin that inherits from PepperDash.Essentials.Core.DisplayBase. If you're only implementing the standard display control functions such as power/input/volume control, then the existing bridge class `DisplayControllerBridge` can be used. If you needed to add additional functions to the bridge, then you would need to write your own bridge in the plugin.

For additional info see the [SIMPL-Bridging article](~/docs/SIMPL-Bridging.md).

## Template Essentials Plugin Repository

Fork this repository when starting a new plugin. The template repository uses the essentials-builds repository as a submodule. This allows the plugin to reference a specific build version of Essentials. You must make sure that you checkout the correct build of the Essentials-Builds repo that contains any dependencies that your plugin may rely on.

[Essentials Plugin Template Repository](https://github.com/PepperDash/EssentialsPluginTemplate)

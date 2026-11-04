# Configuration topics

Configuration is central to Essentials. On this page we will cover configuration-related topics, including the important concept of configure-first and some details about the config file process.

## Classes Referenced

- `PepperDash.Essentials.Core.Config.DeviceConfig`

## Configure-first development

## Framework Libraries

See [Essentials architecture](~/docs/technical-docs/Arch-summary.md) for a table of what each library contains and a diagram of the references between them.

### Architecture

#### Device and DeviceManager

A `Device` is a logical construct. It may represent a piece of hardware, a port, a socket, a collection of other devices/ports/constructs that define an operation, or any unit of logic that should be created at startup and exist independent of other devices.

`DeviceManager` is the collection of all Devices. The collection of everything we control on a system. **ADD SOME MORE HERE**

#### Flat system design

In Essentials, most everything we do is focused in one layer: The Devices layer. This layer interacts with the physical Crestron and other hardware and logical constructs underneath, and is designed so that we rarely act directly on the often-inconsistent hardware layer. The `DeviceManager` is responsible for containing all of the devices in this layer.

Types of devices:

- Rooms
- Sources
- Codecs, DSPs, displays, routing hardware
- IR Ports, Com ports, SSh Clients, ...
- Occupancy sensors and relay-driven devices
- Logical devices that manage multiple devices and other business, like shade or lighting scene controllers
- Fusion connectors to rooms

A Device doesn't always represent a physical piece of hardware, but rather a logical construct that "does something" and is used by one or more other devices in the running program. For example, we create a room device, and its corresponding Fusion device, and that room has a Cisco codec device, with an attached SSh client device. All of these lie in a flat collection in the `DeviceManager`.

> The `DeviceManager` is a modified collection of objects, and those objects don't have to inherit from Devices or EssentialsDevices, but must at least implement the `IKeyed` interface (so items can be looked up by their key.) Items in the `DeviceManager` that are Devices are run through additional steps of activation at startup. This collection of devices is all interrelated by their string keys.

In this flat design, we spin up devices, and then introduce them to their "coworkers and bosses" - the other devices and logical units that they will interact with - and get them all operating together to form a running unit. For example: A room configuration will contain a "VideoCodecKey" property and a "DefaultDisplayKey" property. The `DeviceManager` provides the room with the codec or displays having the appropriate keys. What the room does with those is dependent on its coding.

> In the default Essentials routing scheme, the routing system gets the various devices involved in given route from `DeviceManager`, as they are discovered along the defined tie-lines. This is all done at route-time, on the fly, using only device and port keys. As soon as the routing operation is done, the whole process is released from memory. This is extremely-loose coupling between objects.

This flat structure ensures that every device in a system exists in one place and may be shared and reused with relative ease. There is no hierarchy.

#### Architecture drawing

See the [architecture drawing](~/docs/technical-docs/Arch-1.md#architecture-drawing).

#### Essentials Configurable System Lifecycle

See [Essentials Configurable System Lifecycle](~/docs/technical-docs/Arch-lifecycle.md).

### Activation phases additional topics and examples (OTHER DOCS)

Concepts (link)

Room and touchpanel activation (link)

#### Configure first development

One of the primary concepts that has been adopted and must be adhered to when writing for Essentials framework is the concept of "configure first." The simple version is: Write what you need to do in the related configuration file (and configuration tool) first, then write the code that runs from that configuration. This ensures that the running code can actually be configured in the "flat" structure of devices and rooms that Essentials uses.

Often, code is written and tested first without consideration for configurability. Then, when a developer tries to make it configurable, they discover that the code as written doesn’t support it without complicated configuration files. This creates spaghetti code in tools that are written to generate configurations and tends to create tighter coupling between objects than we desire. Later, a modified version of the original program is desired, but because the code was written in such a specific fashion, the code is hard to refactor and extend. This causes the configuration tool and configuration files to become even more convoluted. The modern versions of configuration tools that are starting to come out are modular and componentized. We want to ensure as much re-use of these modules as possible, with extensions and added features added on, rather than complete rewrites of existing code. In our running systems, we want to ensure as much flexibility in design as possible, eliminating multiple classes with similar code.

### Configuration reader process

At the heart of the Essentials framework is the configuration system. While not technically necessary for a system written with the Essentials framework, it is the preferred and, currently, the only way to build an Essentials system. The configuration file is JSON, and well-defined (but not well documented, yet). It is comprised of blocks:

- info (object) Contains metadata about the config file
- devices (array) Contains, well, the devices we intend to build and load
- rooms (array, typically only one) Contains the rooms we need
- sourceLists (object) Used by one or more rooms to represent list(s) of sources for those rooms
- tieLines (array) Used by the routing system to discover routing between sources and displays

In addition, a downloaded Portal config file may be in the older (v1) template/system form, meaning that the file contains two main objects, representing the template configuration and its system-level overrides. A v2 Portal config is already merged and has no `template`/`system` objects. Other metadata, such as Portal UUIDs or URLs may be present.

At startup, after plugins are loaded, the configuration file is read and loaded by the `ConfigReader.LoadConfig()` method. If the file is in template/system form, the template and system are merged into a single configuration object, and that object is then deserialized into configuration wrapper classes that define most of the structure of the program to be built. (Custom configuration objects were built to allow for better type handling rather than using JToken methods to parse out error-prone property names.)

For example, a `DeviceConfig` object:

```cs
namespace PepperDash.Essentials.Core.Config
{
    public class DeviceConfig
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("uid")]
        public int Uid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        [JsonConverter(typeof(DevicePropertiesConverter))]
        public JToken Properties { get; set; }
    }
}
```

_Every_ `Device` present must adhere to those five properties plus a properties object. The properties object will have its own deserialization helpers, depending on what its structure is.

Once the ConfigReader has read and deserialized the config file, then `ControlSystem.Load()` is called. This does the following in order:

1. Loads Devices
2. Loads Rooms
3. Activation sequence (`DeviceManager.ActivateAll()`)
4. Loads TieLines

This ordering ensures that all devices are at least present before building rooms and tie lines. Rooms can be built without their required devices being present. In principle, this could break from the loosely-coupled goal we have described, but it is the clearest way to build the system in code. The goal is still to build a room class that doesn't have functional dependencies on devices that may not be ready for use.

In each device/room step, a device factory process is called. Each library and plugin registers its device types with `PepperDash.Essentials.Core.DeviceFactory` through classes that implement `IDeviceFactory` (usually by deriving from `EssentialsDeviceFactory<T>` or `EssentialsPluginDeviceFactory<T>`). `DeviceFactory.GetDevice()` looks up the factory registered for the config's `type` and calls it. This allows us to break up the factory process into individual libraries, and not have a huge list of types and build procedures. Here's part of the code:

```cs
var newDev = Core.DeviceFactory.GetDevice(devConf);

if (newDev != null)
    DeviceManager.AddDevice(newDev);
```

In each respective factory, or device constructor, the configuration's properties object is either converted to a config object or read from using `JToken` methods. This builds the device which may be ready to go, or may require activation as described above.

The same process is carried out for rooms in the `ControlSystem.LoadRooms()` method; room types are registered with `DeviceFactory` just like device types.

Plugins loaded from the `plugins` folder register their factories the same way, which is where custom Essentials systems grow from. See [Plugins](~/docs/technical-docs/Plugins.md).

After those steps, the system will be running and ready to use.

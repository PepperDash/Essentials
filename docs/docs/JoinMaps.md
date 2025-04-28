# What is a Join Map?

A join map is a class that defines the list of joins accessible to an `EssentialsBridgeableDevice` across an EISC Bridge.

## Why use a Join Map?

A join map is necessary to bridge joins across an EISC bridge from Essentials to a SIMPL program. Join maps can be overriden in a configuration as necessary, but by default each device has a standard join map that publishes joins as a function of the joinOffset property of the device within a given Essentials Bridge. Join maps are reusable and extensible. Several join maps for standard device types already exist within Essentials, and those can be utilized for plugin creation without creation of a new join map. Utilizing standard join maps allows you to create a consistent API between device types that allows switching of devices via config without any new SIMPL or SIMPL#Pro code being written.

## How Join maps Work

Whenever you instantiate a device and link that device to an EISC bridge utilizing your configuration in Essentials, the method `LinkToApi()` is called. This method matches various methods, feedbacks, and properties to joins on the EISC bridge in order to create a consistent API for communication to SIMPL.

Whenever `LinkToApi()` is called, it creates a new instance of the device's joinMap class on demand. The constructor of that joinMap creates an object containing the join metadata, adds any configured join offsets to the standard join map, and adds all associated joins to a global join list that can be easily referenced from the command line.

There are several components for each join within a join map.

```cs
[JoinName("Online")]
        public JoinDataComplete Online = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Reports Online Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
```

### Attribute

```cs
[JoinName("Online")]
```

If the attribute is present, the join data is added to the publically available list `Joins`. This can be used to "prebuild" functionality within a join map that you may not yet need. If you do not add this attribute (or simply comment it out), the join data will not be displayed whenever join data is printed using the `getjoinmap` command.

### JoinData

```cs
JoinData() { JoinNumber = 1, JoinSpan = 1 };
```

`JoinData` contains the pertinent information for the bridge. JoinData contains the information that the bridge utilizes to create each associated connection from the EISC to the methods, properties, and feedbacks associated with a device.

`JoinNumber` is the 1-based index of the join you wish to tie to a given method, property, or feedback. This join, combined with the offset defined in the brdige's configuration for a device, will give you the SIMPL EISC join linked to the given data.

`JoinSpan` determines a number of associated joins. Perhaps you have a list of Camera Presets, or a list of inputs. You can create one single join map entry and define the span of all associated join types.

A `JoinData` object with a `JoinNumber` of 11 and a `JoinSpan` of 10 and a `joinOffset` of 0 is associated with joins 11-20 on the EISC.

### JoinMetadata

```cs
JoinMetadata() { Label = "Reports Online Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital }
```

`JoinMetadata` provides the data reported when the `getjoinmap` command is used.

`Label` is the description of the what this join does.

`JoinCapabilities` is represented by an enum defining the direction that the data is flowing for this join. Appropriate values are:

```cs
public enum eJoinCapabilities
    {
        None = 0,
        ToSIMPL = 1,
        FromSIMPL = 2,
        ToFromSIMPL = ToSIMPL | FromSIMPL
    }
```

`JoinType` is represented by an enum defining the data type in SIMPL. Appropriate values are:

```cs
public enum eJoinType
    {
        None = 0,
        Digital = 1,
        Analog = 2,
        Serial = 4,
        DigitalAnalog = Digital | Analog,
        DigitalSerial = Digital | Serial,
        AnalogSerial = Analog | Serial,
        DigitalAnalogSerial = Digital | Analog | Serial
    }
```

### JoinDataComplete

```chsarp
JoinDataComplete(JoinData data, JoinMetadata metadata);
```

`JoinDataComplete` represents the `JoinData` and the `JoinMetadata` in a single object. You can call an instance of `JoinDataComplete` to report any information about a specific join. In a device bridge, you would utilize the `JoinNumber` property to link a feature from the plugin to the EISC API.

### Example Join Map

This is the join map for `IBasicCommunication` Devices

```cs
namespace PepperDash.Essentials.Core.Bridges
{
    public class IBasicCommunicationJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("TextReceived")]
        public JoinDataComplete TextReceived = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Text Received From Remote Device", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("SendText")]
        public JoinDataComplete SendText = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Text Sent To Remote Device", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        [JoinName("SetPortConfig")]
        public JoinDataComplete SetPortConfig = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Set Port Config", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        [JoinName("Connect")]
        public JoinDataComplete Connect = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Connect", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Connected")]
        public JoinDataComplete Connected = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Connected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Status")]
        public JoinDataComplete Status = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });


        public IBasicCommunicationJoinMap(uint joinStart)
            : base(joinStart, typeof(IBasicCommunicationJoinMap))
        {
        }
    }
}
```

### Returning Data on Join Maps

A mechanism for printing join maps to console from a running Essentials program is built in to Essentials.

Given a single Generic Communication device with a `joinStart` of 11 and a key of "Com-1", defined on a bridge with a key of "Bridge-1" and IPID of A0, the command `getjoinmap Bridge-1 Com-1` would return:

```sh
Join Map for device 'Com-1' on EISC 'Bridge-1':
GenericComm:

Digitals:
Found 2 Digital Joins
Join Number: '11' | JoinSpan: '1' | Label : 'Connect' | Type: 'Digital' | Capabilities : 'FromSimpl'
Join Number: '11' | JoinSpan: '1' | Label : 'Connected' | Type: 'Digital' | Capabilities : 'ToSimpl'
Analogs:
Found 1 Analog Joins
Join Number: '11' | JoinSpan: '1' | Label : 'Status' | Type: 'Analog' | Capabilities : 'ToSimpl'
Serials:
Found 3 Serial Joins
Join Number: '11' | JoinSpan: '1' | Label : 'Text Received From Remote Device' | Type: 'Serial' | Capabilities : 'FromSimpl'
Join Number: '11' | JoinSpan: '1' | Label : 'Text Sent To Remote Device' | Type: 'Serial' | Capabilities : 'ToSimpl'
Join Number: '12' | JoinSpan: '1' | Label : 'Set Port Config' | Type: 'Serial' | Capabilities : 'FromSimpl'
```

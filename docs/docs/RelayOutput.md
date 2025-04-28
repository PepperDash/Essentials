# RelayOutput

Relays can be bridged directly to SIMPL from any device that is both inlcuded within essentials and has a relay.

Consider the following example.

```JSON
{
    "template": {
        "roomInfo": [
            {}
        ],
        "devices": [
            {
                "key": "processor",
                "uid": 0,
                "type": "pro3",
                "name": "pro3",
                "group": "processor",
                "supportedConfigModes": [
                    "compliance",
                    "essentials"
                ],
                "supportedSystemTypes": [
                    "hudType",
                    "presType",
                    "vtcType",
                    "custom"
                ],
                "supportsCompliance": true,
                "properties": {}
            },
            {
                "key": "Relay-1",
                "uid": 3,
                "name": "Relay 1",
                "group": "api",
                "type": "relayOutput",
                "properties": {
                    "portDeviceKey" : "processor",
                    "portNumber" : 1
                }
            },
            {
                "key": "Relay-2",
                "uid": 3,
                "name": "Relay 2",
                "group": "api",
                "type": "relayOutput",
                "properties": {
                    "portDeviceKey" : "processor",
                    "portNumber" : 2
                }
            },
            {
                "key": "deviceBridge",
                "uid": 4,
                "name": "BridgeToDevices",
                "group": "api",
                "type": "eiscapiadv",
                "properties": {
                    "control": {
                        "tcpSshProperties": {
                            "address": "127.0.0.2",
                            "port": 0
                        },
                        "ipid": "03",
                        "method": "ipidTcp"
                    },
                    "devices": [
                        {
                            "deviceKey": "Relay-1",
                            "joinStart": 1
                        },
                        {
                            "deviceKey": "Relay-2",
                            "joinStart": 2
                        }
                    ]
                }
            }
        ]
    }
}
```

## RelayOutput Configuration Explanation

This configuration is meant for a Pro3 device, and instantiates two relay ports and links them to an eisc bridge to another processor slot on ipid 3.  Let's break down the ```Relay-1``` device.

```JSON
{
    "key": "Relay-1",
    "uid": 3,
    "name": "Relay 1",
    "group": "api",
    "type": "relayOutput",
    "properties": {
        "portDeviceKey" : "processor",
        "portNumber" : 1
    }
}
```

**```Key```**

The Key is a unique identifier for essentials.  The key allows the device to be linked to other devices also defined by key.  All Keys MUST be unique, as every device is added to a globally-accessible dictionary.  If you have accidentally utilized the same key twice, Essentials will notify you during startup that there is an issue with the device.

**```Uid```**

The Uid is reserved for use with an PepperDash internal config generation tool, and is not useful to Essentials in any way.

**```Name```**

The Name a friendly name assigned to the device.  Many devices pass this data to the bridge for utilization in SIMPL.

**```Group```**

Utilized in certain Essentials devices.  In this case, the value is unimportant.

**```Type```**

The Type is the identifier for a specific type of device in Essentials.  A list of all valid types can be reported by using the consolecommand ```gettypes``` in Essentials.  In this case, the type is ```relayOutput```.  This type is valid for any instance of a Relay Output.

**```Properties```**

These are the properties essential to the instantiation of the identified type.

### Properties

There are two properties relevant to the instantiation of a relay device.

**```portDeviceKey```**

This property maps to the ```key``` of the device upon which the relay resides.

**```portNumber```**

This property maps to the number of the relay on the device you have mapped the relay device to.  Even if the device has only a single relay, ```portNumber``` must be defined.

### The JoinMap

The joinmap for a ```relayOutput``` device is comprised of a single digital join.

```cs
namespace PepperDash.Essentials.Core.Bridges
{
    public class GenericRelayControllerJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("Relay")]
        public JoinDataComplete Relay = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Relay State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });


        public GenericRelayControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(GenericRelayControllerJoinMap))
        {
        }
    }
}
```

```Relay``` is a digital join that represents both the trigger and the feedback for the associated relay device.  Its join is set to 1.

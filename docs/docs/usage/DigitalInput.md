# DigitalInput

Digital Inputs can be bridged directly to SIMPL from any device that is both inlcuded within essentials and has a relay.

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
                "key": "DigitalInput-1",
                "uid": 3,
                "name": "Digital Input 1",
                "group": "api",
                "type": "digitalInput",
                "properties": {
                    "portDeviceKey" : "processor",
                    "portNumber" : 1,
                    "disablePullUpResistor" : true
                }
            },
            {
                "key": "DigitalInput-2",
                "uid": 3,
                "name": "Digital Input 2",
                "group": "api",
                "type": "digitalInput",
                "properties": {
                    "portDeviceKey" : "processor",
                    "portNumber" : 2,
                    "disablePullUpResistor" : true
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
                            "deviceKey": "DigitalInput-1",
                            "joinStart": 1
                        },
                        {
                            "deviceKey": "DigitalInput-2",
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

This configuration is meant for a Pro3 device, and instantiates two relay ports and links them to an eisc bridge to another processor slot on ipid 3.  Let's break down the ```DigitalInput-1``` device.

```JSON
{
    "key": "DigitalInput-1",
    "uid": 3,
    "name": "Digital Input 1",
    "group": "api",
    "type": "digitalInput",
    "properties": {
        "portDeviceKey" : "processor",
        "portNumber" : 1,
        "disablePullUpResistor" : true
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

The Type is the identifier for a specific type of device in Essentials.  A list of all valid types can be reported by using the consolecommand ```gettypes``` in Essentials.  In this case, the type is ```digitalInput```.  This type is valid for any instance of a Relay Output.

**```Properties```**

These are the properties essential to the instantiation of the identified type.

### Properties

There are two properties relevant to the instantiation of a relay device.

**```portDeviceKey```**

This property maps to the ```key``` of the device upon which the relay resides.

**```portNumber```**

This property maps to the number of the relay on the device you have mapped the relay device to.  Even if the device has only a single relay, ```portNumber``` must be defined.

**```disablePullUpResistor```**

This is a boolean value, therefore it is a case-sensitive ```true``` or ```false``` utilized to determine if the pullup resistor on the digital input will be disabled or not.

### The JoinMap

The joinmap for a ```digitalInput``` device is comprised of a single digital join.

```cs
namespace PepperDash.Essentials.Core.Bridges
{
    public class IDigitalInputJoinMap : JoinMapBaseAdvanced
    {

        [JoinName("InputState")]
        public JoinDataComplete InputState = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Room Email Url", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });


        public IDigitalInputJoinMap(uint joinStart)
            : base(joinStart, typeof(IDigitalInputJoinMap))
        {
        }
    }
}
```

```InputState``` is a digital join that represents the feedback for the associated Digital Input Device.  Its join is set to 1.

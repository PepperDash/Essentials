# Use with SIMPL Windows

***
* [YouTube Video - SIMPL Windows in PepperDash Essentials](https://youtu.be/P2jNzsfpgJE)
***

Essentials allows for devices defined within the SIMPL# Pro application to be bridged to a SIMPL Windows application over Ethernet Intersystem Communication (EISC). This allows a SIMPL Windows program to take advantage of some of the features of the SIMPL# Pro environment, without requiring the entire application to be written in C#.

Some of the main advantages are:

1. The ability to instantiate devices from configuration.
2. The ability to leverage C# concepts to handle data intensive tasks (Serialization/Deserialization of JSON/XML, cyrptography, etc.).
3. The ability to reuse the same compiled SIMPL Windows program (regardless of target processor type) by offloading all the variables that may be room or hardware specific to Essentials.
4. The ability to handle multiple communciation types generically without changing the SIMPL Program (TCP/UDP/SSH/HTTP/HTTPS/CEC, etc.)
5. Much faster development cycle
6. Reduced processor overhead
7. Ability to easily share devices defined in Essentials between multiple other programs

## Implementation

Bridges are devices that are defined within the devices array in the config file. They are unique devices with a specialized purpose: to act as a bridge between Essentials Devices and applications programmed traditionally in SIMPL Windows. This is accomplished by instantiating a Three Series Intersystem Communication symbol within the bridge device, and linking its Boolean/Ushort/String inputs and outputs to actions on one or multiple Essentials device(s). The definition for which joins map to which actions is defined within the device to be bridged to in a class that derives from JoinMapBase.

Let's consider the following Essentials Configuration:

```JSON
{
    "template": {
        "roomInfo": [
            {}
        ],
        "devices": [
            {
                "key": "processor",
                "uid": 1,
                "type": "pro3",
                "name": "PRO3 w/o cards",
                "group": "processor",
                "supportedConfigModes": [
                    "essentials"
                ],
                "supportedSystemTypes": [
                    "hudType",
                    "presType",
                    "vtcType",
                    "custom"
                ],
                "supportsCompliance": true,
                "properties": {
                    "numberOfComPorts": 6,
                    "numberOfIrPorts": 8,
                    "numberOfRelays": 8,
                    "numberOfDIOPorts": 8
                }
            },
            {
                "key": "panasonicDisplay01",
                "type": "PanasonicThefDisplay",
                "name": "Main Display",
                "group": "displays",
                "uid": 2,
                "properties": {
                    "id": "01",
                    "inputNumber": 1,
                    "outputNumber": 1,
                    "control": {
                        "comParams": {
                            "hardwareHandshake": "None",
                            "parity": "None",
                            "protocol": "RS232",
                            "baudRate": 9600,
                            "dataBits": 8,
                            "softwareHandshake": "None",
                            "stopBits": 1
                        },
                        "controlPortNumber": 1,
                        "controlPortDevKey": "processor",
                        "method": "com"
                    }
                }
            },
            {
                "key": "vtcComPort",
                "uid": 3,
                "name": "VTC Coms",
                "group": "comm",
                "type": "genericComm",
                "properties": {
                    "control": {
                        "comParams": {
                            "hardwareHandshake": "None",
                            "parity": "None",
                            "protocol": "RS232",
                            "baudRate": 38400,
                            "dataBits": 8,
                            "softwareHandshake": "None",
                            "stopBits": 1
                        },
                        "controlPortNumber": 2,
                        "controlPortDevKey": "processor",
                        "method": "com"
                    }
                }
            },
            {
                "key": "deviceBridge",
                "uid": 4,
                "name": "BridgeToDevices",
                "group": "api",
                "type": "eiscApi",
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
                            "deviceKey": "panasonicDisplay01",
                            "joinStart": 1
                        },
                        {
                            "deviceKey": "vtcComPort",
                            "joinStart": 51
                        }
                    ]
                }
            }
        ]
    }
}
```

We have four Essentials Devices configured:

1. Pro3 with a Key of "processor"

1. Panasonic Display with a Key of "panasonicDisplay01"

1. Com port with a Key of "vtcComPort"

1. Bridge with a Key of "deviceBridge"

We want to have access to the com port for VTC Control from SIMPL Windows and we want to control the display from SIMPL Windows. To accomplish this, we have created a bridge device and added the devices to be bridged to the "devices" array on the bridge. As you can see we define the device key and the join start, which will determine which joins we will use on the resulting EISC to interact with the devices. In the Bridge control properties we defined ipid 03, and we will need a corresponding Ethernet System Intercommunication in the SIMPL Windows program at ipid 03.

Now that our devices have been built, we can refer to the device join maps to see which joins correspond to which actions.

See below:

```cs
namespace PepperDash.Essentials.Core.Bridges
{
    public class DisplayControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("PowerOff")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Power Off", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PowerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Power On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IsTwoWayDisplay")]
        public JoinDataComplete IsTwoWayDisplay = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Is Two Way Display", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeUp")]
        public JoinDataComplete VolumeUp = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "Volume Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeLevel")]
        public JoinDataComplete VolumeLevel = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "Volume Level", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("VolumeDown")]
        public JoinDataComplete VolumeDown = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "Volume Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeMute")]
        public JoinDataComplete VolumeMute = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata() { Label = "Volume Mute", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeMuteOn")]
        public JoinDataComplete VolumeMuteOn = new JoinDataComplete(new JoinData() { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata() { Label = "Volume Mute On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeMuteOff")]
        public JoinDataComplete VolumeMuteOff = new JoinDataComplete(new JoinData() { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata() { Label = "Volume Mute Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputSelectOffset")]
        public JoinDataComplete InputSelectOffset = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 10 },
            new JoinMetadata() { Label = "Input Select", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputNamesOffset")]
        public JoinDataComplete InputNamesOffset = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 10 },
            new JoinMetadata() { Label = "Input Names Offset", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("InputSelect")]
        public JoinDataComplete InputSelect = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "Input Select", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("ButtonVisibilityOffset")]
        public JoinDataComplete ButtonVisibilityOffset = new JoinDataComplete(new JoinData() { JoinNumber = 41, JoinSpan = 10 },
            new JoinMetadata() { Label = "Button Visibility Offset", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalSerial });

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata() { Label = "Is Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        public DisplayControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(CameraControllerJoinMap))
        {
        }
    }
}
```

We know that the Panasonic Display uses the DisplayControllerJoinMap class and can see the join numbers that will give us access to functionality in the Device.

IsOnline = 50  
PowerOff = 1  
PowerOn = 2  
IsTwoWayDisplay = 3  
VolumeUp = 5  
VolumeDown = 6  
VolumeMute = 7

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

Considering our Bridge config, we can see that the display controls will start at join 1, and the VTC Com port will start at join 51. The result is a single EISC that allows us to interact with our Essentials devices.

To control diplay power from SIMPL Windows, we would connect Digital Signals to joins 1 & 2 on the EISC to control Display Power On & Off.
To utilize the com port device, we would connect Serial Signals (VTC_TX$ and VTC_RX$) to join 51 on the EISC.

You can refer to our [SIMPL Windows Bridging Example](https://github.com/PepperDash/EssentialsSIMPLWindowsBridgeExample) for a more complex example.  
Example device config: <https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Example%20Configuration/SIMPLBridging/SIMPLBridgeExample_configurationFile.json>

## Notes

1. It is important to realize that there are no safety checks (yet) when assigning joinStarts in bridge configurations. If you were to put two devices on a bridge with overlapping joins, the most recently bridged join would overwrite previously bridged joins. For now it is on the programmer to ensure there are no conflicting join maps.

2. There is _no_ limit to the amount of times a device may be bridged to. You may have the same device on multiple bridges across multiple applications without problem. That being said, we recommend using common sense. Accessing a single com port for VTC control via multiple bridges may not be wise...

3. A bridge need not only bridge between applications on the same processor. A bridge may bridge to an application on a completely separate processor; simply define the ip address in the Bridge control properties accordingly.

4. For devices included in Essentials, you will be able to find defined join maps below. If you are building your own plugins, you will need to build the join map yourself. It would be beneficial to review the wiki entry on the [Feedback Class](~/docs/technical-docs/Feedback-Classes.md) for this.

5. When building plugins, we highly recommend reusing JoinMaps, as this will make code more easily interchangeable. For example; if you were to build a display plugin, we'd recommend you use/extend the existing `DisplayControllerJoinMap`. This way, you can swap plugins without needing any change on the SIMPL Windows side. This is extremely powerful when maintaining SIMPL Windows code bases for large deployments that may utilize differing equipment per room. If you can build a SIMPL Windows program that interacts with established join maps, you can swap out the device via config without any change needed to SIMPL Windows.

6. Related to item 5, you can use the same paradigm with respect to physical device communication. If you were to have a DSP device in some rooms communicating over RS232 and some via SSH, it would be trival to swap the device from a Com port to an SSH client in the Essentials Devicee Config and update the Bridge Config to brigde to the desired communication method. Again this would require no change on the SIMPL Windows side as long as you maintain the same join Start in the Bridge Device Configuration.

## Common Use Cases

1. There are 10 conference rooms that all operate the same, but have hardware differences that are impossible to account for in SIMPL Windows. For example, each room might have a DM-MD8X8 chassis, but the input and output cards aren't all in the same order, or they might be different models but function the same. You can use Essentials with a unique configuration file for each hardware configuration.

2. You have a floor of conference rooms that all share some centralized hardware like DSP, AV Routing and a shared CEN-GWEXER gateway with multiple GLS-OIR-CSM-EX-BATT occupancy sensors. All the shared hardware can be defined in the Essentials configuration and bridged over an EISC to each program that needs access. The same device can even be exposed to multiple programs over different EISCs.

3. You have a SIMPL program that works for many room types, but because some rooms have different models of processors than others (CP3/CP3N/AV3/PRO3/DMPS3 variants), you have to maintain several versions of the program, compiled for each processor model to maintain access to features like the System Monitor slot. You can use Essentials running in a slot on a processor to expose the System Monitor and many other features of the processor, regardless of model. Now you only need to maintain a single SIMPL program defined for your most complex processor application (ex. PRO3)

## Join Map Documentation

[Join Map Documentation](~/docs/usage/JoinMaps.md)

## Device Type Join Maps

Please note that these joinmaps _may_ be using a deprecated implementation. The implementation is valid but nonetheless frowned upon for new features and plugins.

### AirMediaController

> supports: AM-200, AM-300

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/AirMediaControllerJoinMap.cs>

### AppleTvController

> supports: IR control of Apple TV

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/AppleTvJoinMap.cs>

### CameraControlBase

> supports: any camera that derives from CameraBase

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/CameraControllerJoinMap.cs>

### DisplayController

> supports: IR controlled displays, any two way display driver that derives from PepperDash.Essentials.Core.DisplayBase

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/DisplayControllerJoinMap.cs>

### DmChasisController

> supports: All DM-MD-8x8/16x16/32x32 chassis, with or w/o DM-CPU3 Card

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/DmChassisControllerJoinMap.cs>

### DmRmcController

> supports: All DM-RMC devices

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/DmRmcControllerJoinMap.cs>

### DmTxController

> supports: All Dm-Tx devices

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/DmTxControllerJoinMap.cs>

### DmpsAudioOutputController

> supports: Program, Aux1, Aux2 outputs of all DMPS3 Control Systems

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/DmpsAudioOutputControllerJoinMap.cs>

### DmpsRoutingController

> supports: Av routing for all DMPS3 Control Systems

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/DmpsRoutingControllerJoinMap.cs>

### GenericRelayController

> supports: Any relay port on a Crestron Control System or Dm Endpoint

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/GenericRelayControllerJoinMap.cs>

### GenericLightingJoinMap

> supports: Devices derived from PepperDash.Essentials.Core.Lighting.LightingBase

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/GenericLightingJoinMap.cs>

### GlsOccupancySensorBase

> supports: Any Crestron GLS-Type Occupancy sensor - single/dual type

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/GlsOccupancySensorBaseJoinMap.cs>

### HdMdxxxCEController

> supports: HD-MD-400-C-E, HD-MD-300-C-E, HD-MD-200-C-E, HD-MD-200-C-1G-E-B/W

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/HdMdxxxCEControllerJoinMap.cs>

### IBasicCommunication

> supports: Any COM Port on a Control System or Dm Endpoint device, TCP Client, SSH Client, or UDP Server

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/IBasicCommunicationJoinMap.cs>

### IDigitalInput

> supports: Any Digital Input on a Control System, or DM Endpoint device

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/IDigitalInputJoinMap.cs>

### SystemMonitorController

> supports: Exposing the system monitor slot for any Control System

<https://github.com/PepperDash/Essentials/blob/main/essentials-framework/Essentials%20Core/PepperDashEssentialsBase/Bridges/JoinMaps/SystemMonitorJoinMap.cs>

## Example SIMPL Windows Program

We've provided an [example program](https://github.com/PepperDash/EssentialsSIMPLWindowsBridgeExample) for SIMPL Windows that works with the provided example Essentials configuration file [SIMPLBridgeExample_configurationFile.json](https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Example%20Configuration/SIMPLBridging/SIMPLBridgeExample_configurationFile.json). Load Essentials and the example SIMPL program to two slots on the same processor and you can get a better idea of how to take advantage of SIMPL Windows bridging.

Next: [Essentials architecture](~/docs/technical-docs/Arch-summary.md)

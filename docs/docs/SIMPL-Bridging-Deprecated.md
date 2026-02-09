# Deprecated

**Note : this entry is out of date - please see [SIMPL Windows Bridging](~/docs/SIMPL-Bridging.md)**

## SIMPL Windows Bridging - Deprecated

Essentials allows for devices defined within the SIMPL# Pro application to be bridged to a SIMPL Windows application over Ethernet Intersystem Communication (EISC). This allows a SIMPL Windows program to take advantage of some of the features of the SIMPL# Pro environment, without requiring the entire application to be written in C#.

Some of the main advantages are:

1. The ability to instantiate devices from configuration.
1. The ability to leverage C# concepts to handle data intensive tasks (Serialization/Deserialization of JSON/XML, cyrptography, etc.).
1. The ability to reuse the same compiled SIMPL Windows program (regardless of target processor type) by offloading all the variables that may be room or hardware specific to Essentials.
1. The ability to handle multiple communciation types generically without changing the SIMPL Program (TCP/UDP/SSH/HTTP/HTTPS/CEC, etc.)
1. Much faster development cycle
1. Reduced processor overhead
1. Ability to easily share devices defined in Essentials between multiple other programs

## Implementation

Bridges are devices that are defined within the devices array in the config file. They are unique devices with a specialized purpose; to act as a bridge between Essentials Devices and applications programmed traditionally in Simpl Windows. This is accomplished by instantiating a Three Series Intersystem Communication symbol within the bridge device, and linking its Boolean/Ushort/String inputs and outputs to actions on one or multiple Essentials device(s). The definition for which joins map to which actions is defined within the device to be bridged to in a class that derives from JoinMapBase.

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

We want to have access to the com port for VTC Control from Simpl Windows and we want to control the display from Simpl Windows. To accomplish this, we have created a bridge device and added the devices to be bridged to the "devices" array on the bridge. As you can see we define the device key and the join start, which will determine which joins we will use on the resulting EISC to interact with the devices. In the Bridge control properties we defined ipid 03, and we will need a corresponding Ethernet System Intercommunication in the Simpl Windows program at ipid 03.

Now that our devices have been built, we can refer to the device join maps to see which joins correspond to which actions.

See below:

```cs
namespace PepperDash.Essentials.Bridges
{
    public class DisplayControllerJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Turns the display off and reports power off feedback
        /// </summary>
        public uint PowerOff { get; set; }
        /// <summary>
        /// Turns the display on and repots power on feedback
        /// </summary>
        public uint PowerOn { get; set; }
        /// <summary>
        /// Indicates that the display device supports two way communication when high
        /// </summary>
        public uint IsTwoWayDisplay { get; set; }
        /// <summary>
        /// Increments the volume while high
        /// </summary>
        public uint VolumeUp { get; set; }
        /// <summary>
        /// Decrements teh volume while high
        /// </summary>
        public uint VolumeDown { get; set; }
        /// <summary>
        /// Toggles the mute state.  Feedback is high when volume is muted
        /// </summary>
        public uint VolumeMute { get; set; }
        /// <summary>
        /// Range of digital joins to select inputs and report current input as feedback
        /// </summary>
        public uint InputSelectOffset { get; set; }
        /// <summary>
        /// Range of digital joins to report visibility for input buttons
        /// </summary>
        public uint ButtonVisibilityOffset { get; set; }
        /// <summary>
        /// High if the device is online
        /// </summary>
        public uint IsOnline { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Analog join to set the input and report current input as feedback
        /// </summary>
        public uint InputSelect { get; set; }
        /// <summary>
        /// Sets the volume level and reports the current level as feedback
        /// </summary>
        public uint VolumeLevel { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Reports the name of the display as defined in config as feedback
        /// </summary>
        public uint Name { get; set; }
        /// <summary>
        /// Range of serial joins that reports the names of the inputs as feedback
        /// </summary>
        public uint InputNamesOffset { get; set; }
        #endregion

        public DisplayControllerJoinMap()
        {
            // Digital
            IsOnline = 50;
            PowerOff = 1;
            PowerOn = 2;
            IsTwoWayDisplay = 3;
            VolumeUp = 5;
            VolumeDown = 6;
            VolumeMute = 7;

            ButtonVisibilityOffset = 40;
            InputSelectOffset = 10;

            // Analog
            InputSelect = 11;
            VolumeLevel = 5;

            // Serial
            Name = 1;
            InputNamesOffset = 10;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            PowerOff = PowerOff + joinOffset;
            PowerOn = PowerOn + joinOffset;
            IsTwoWayDisplay = IsTwoWayDisplay + joinOffset;
            ButtonVisibilityOffset = ButtonVisibilityOffset + joinOffset;
            Name = Name + joinOffset;
            InputNamesOffset = InputNamesOffset + joinOffset;
            InputSelectOffset = InputSelectOffset + joinOffset;

            InputSelect = InputSelect + joinOffset;

            VolumeUp = VolumeUp + joinOffset;
            VolumeDown = VolumeDown + joinOffset;
            VolumeMute = VolumeMute + joinOffset;
            VolumeLevel = VolumeLevel + joinOffset;
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
namespace PepperDash.Essentials.Bridges
{
    public class IBasicCommunicationJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Set High to connect, Low to disconnect
        /// </summary>
        public uint Connect { get; set; }
        /// <summary>
        /// Reports Connected State (High = Connected)
        /// </summary>
        public uint Connected { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Reports the connections status value
        /// </summary>
        public uint Status { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Data back from port
        /// </summary>
        public uint TextReceived { get; set; }
        /// <summary>
        /// Sends data to the port
        /// </summary>
        public uint SendText { get; set; }
        /// <summary>
        /// Takes a JSON serialized string that sets a COM port's parameters
        /// </summary>
        public uint SetPortConfig { get; set; }
        #endregion

        public IBasicCommunicationJoinMap()
        {
            TextReceived = 1;
            SendText = 1;
            SetPortConfig = 2;
            Connect = 1;
            Connected = 1;
            Status = 1;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            TextReceived = TextReceived + joinOffset;
            SendText = SendText + joinOffset;
            SetPortConfig = SetPortConfig + joinOffset;
            Connect = Connect + joinOffset;
            Connected = Connected + joinOffset;
            Status = Status + joinOffset;
        }
    }
}
```

TextReceived = 1  
SendText = 1  
SetPortConfig = 2  
Connect = 1  
Connected = 1  
Status = 1

Considering our Bridge config, we can see that the display controls will start at join 1, and the VTC Com port will start at join 51. The result is a single EISC that allows us to interact with our Essentials devices.

To control diplay power from Simpl Windows, we would connect Digital Signals to joins 1 & 2 on the EISC to control Display Power On & Off.
To utilize the com port device, we would connect Serial Signals (VTC_TX$ and VTC_RX$) to join 51 on the EISC.

You can refer to our [Simpl Windows Bridging Example](https://github.com/PepperDash/EssentialsSIMPLWindowsBridgeExample) for a more complex example.  
Example device config: <https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Example%20Configuration/SIMPLBridging/SIMPLBridgeExample_configurationFile.json>

## Notes

1. It is important to realize that there are no safety checks (yet) when assigning joinStarts in bridge configurations. If you were to put two devices on a bridge with overlapping joins, the most recently bridged join would overwrite previously bridged joins. For now it is on the programmer to ensure there are no conflicting join maps.

1. There is _no_ limit to the amount of times a device may be bridged to. You may have the same device on multiple bridges across multiple applications without problem. That being said, we recommend using common sense. Accessing a single com port for VTC control via multiple bridges may not be wise...

1. A bridge need not only bridge between applications on the same processor. A bridge may bridge to an application on a completely separate processor; simply define the ip address in the Bridge control properties accordingly.

1. For devices included in Essentials, you will be able to find defined join maps below. If you are building your own plugins, you will need to build the join map yourself. It would be beneficial to review the wiki entry on the [Feedback Class](~/docs/technical-docs/Feedback-Classes.md) for this.

1. When building plugins, we highly recommend reusing JoinMaps, as this will make code more easily interchangeable. For example; if you were to build a display plugin, we'd recommend you use/extend the existing DisplayControllerJoinMap. This way, you can swap plugins without needing any change on the Simpl Windows side. This is extremely powerful when maintaining Simpl Windows code bases for large deployments that may utilize differing equipment per room. If you can build a Simpl Windows program that interacts with established join maps, you can swap out the device via config without any change needed to Simpl Windows.

1. Related to item 5, you can use the same paradigm with respect to physical device communication. If you were to have a DSP device in some rooms communicating over RS232 and some via SSH, it would be trival to swap the device from a Com port to an SSH client in the Essentials Devicee Config and update the Bridge Config to brigde to the desired communication method. Again this would require no change on the Simpl Windows side as long as you maintain the same join Start in the Bridge Device Configuration.

## Common Use Cases

1. There are 10 conference rooms that all operate the same, but have hardware differences that are impossible to account for in SIMPL Windows. For example, each room might have a DM-MD8X8 chassis, but the input and output cards aren't all in the same order, or they might be different models but function the same. You can use Essentials with a unique configuration file for each hardware configuration.

1. You have a floor of conference rooms that all share some centralized hardware like DSP, AV Routing and a shared CEN-GWEXER gateway with multiple GLS-OIR-CSM-EX-BATT occupancy sensors. All the shared hardware can be defined in the Essentials configuration and bridged over an EISC to each program that needs access. The same device can even be exposed to multiple programs over different EISCs.

1. You have a SIMPL program that works for many room types, but because some rooms have different models of processors than others (CP3/CP3N/AV3/PRO3/DMPS3 variants), you have to maintain several versions of the program, compiled for each processor model to maintain access to features like the System Monitor slot. You can use Essentials running in a slot on a processor to expose the System Monitor and many other features of the processor, regardless of model. Now you only need to maintain a single SIMPL program defined for your most complex processor application (ex. PRO3)

## Device Type Join Maps

### AirMediaController

> supports: AM-200, AM-300

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/AirMediaControllerJoinMap.cs>

### AppleTvController

> supports: IR control of Apple TV

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/AppleTvJoinMap.cs>

### CameraControlBase

> supports: any camera that derives from CameraBase

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/CameraControllerJoinMap.cs>

### DisplayController

> supports: IR controlled displays, any two way display driver that derives from PepperDash.Essentials.Core.DisplayBase

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/DisplayControllerJoinMap.cs>

### DmChasisController

> supports: All DM-MD-8x8/16x16/32x32 chassis, with or w/o DM-CPU3 Card

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/DmChassisControllerJoinMap.cs>

### DmRmcController

> supports: All DM-RMC devices

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/DmRmcControllerJoinMap.cs>

### DmTxController

> supports: All Dm-Tx devices

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/DmTxControllerJoinMap.cs>

### DmpsAudioOutputController

> supports: Program, Aux1, Aux2 outputs of all DMPS3 Control Systems

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/DmpsAudioOutputControllerJoinMap.cs>

### DmpsRoutingController

> supports: Av routing for all DMPS3 Control Systems

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/DmpsRoutingControllerJoinMap.cs>

### GenericRelayController

> supports: Any relay port on a Crestron Control System or Dm Endpoint

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/GenericRelayControllerJoinMap.cs>

### GenericLightingJoinMap

> supports: Devices derived from PepperDash.Essentials.Core.Lighting.LightingBase

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/GenericLightingJoinMap.cs>

### GlsOccupancySensorBase

> supports: Any Crestron GLS-Type Occupancy sensor - single/dual type

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/GlsOccupancySensorBaseJoinMap.cs>

### HdMdxxxCEController

> supports: HD-MD-400-C-E, HD-MD-300-C-E, HD-MD-200-C-E, HD-MD-200-C-1G-E-B/W

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/HdMdxxxCEControllerJoinMap.cs>

### IBasicCommunication

> supports: Any COM Port on a Control System or Dm Endpoint device, TCP Client, SSH Client, or UDP Server

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/IBasicCommunicationJoinMap.cs>

### IDigitalInput

> supports: Any Digital Input on a Control System, or DM Endpoint device

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/IDigitalInputJoinMap.cs>

### SystemMonitorController

> supports: Exposing the system monitor slot for any Control System

<https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Bridges/JoinMaps/SystemMonitorJoinMap.cs>

## Example SIMPL Windows Program

We've provided an [example program](https://github.com/PepperDash/EssentialsSIMPLWindowsBridgeExample) for SIMPL Windows that works with the provided example Essentials configuration file [SIMPLBridgeExample_configurationFile.json](https://github.com/PepperDash/Essentials/blob/main/PepperDashEssentials/Example%20Configuration/SIMPLBridging/SIMPLBridgeExample_configurationFile.json). Load Essentials and the example SIMPL program to two slots on the same processor and you can get a better idea of how to take advantage of SIMPL Windows bridging.

Next: [Essentials architecture](~/docs/technical-docs/Arch-summary.md)

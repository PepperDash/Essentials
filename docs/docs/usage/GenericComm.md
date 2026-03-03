# GenericComm

One of the most common scenarios in control system development is utilizing RS232 to connect to a device.  Essentials doesn't restrict you to connecting a native essentials device or plugin to the comport.  You can directly access the comport, and even set baudrates on the fly if you so desire.

Similarly you can instantiate one of several socket types in this manner and bridge them directly to SIMPL.

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
                "key": "Comport-1",
                "uid": 3,
                "name": "Comport 1",
                "group": "api",
                "type": "genericComm",
                "properties": {
                    "control": {
                        "method": "com",
                        "comParams": {
                            "hardwareHandshake": "None",
                            "parity": "None",
                            "protocol": "RS232",
                            "baudRate": 115200,
                            "dataBits": 8,
                            "softwareHandshake": "None",
                            "stopBits": 1
                        },
                        "controlPortNumber": 1,
                        "controlPortDevKey": "processor",
                    }
                }
            },
            {
                "key": "Comport-2",
                "uid": 3,
                "name": "Comport 2",
                "group": "api",
                "type": "genericComm",
                "properties": {
                    "control": {
                        "method": "ssh",
                        "tcpSshProperties": {
                            "address": "192.168.1.57",
                            "port": 22,
                            "username": "",
                            "password": "",
                            "autoReconnect": true,
                            "autoReconnectIntervalMs": 10000
                        }
                    }
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
                            "deviceKey": "Comport-1",
                            "joinStart": 1
                        },
                        {
                            "deviceKey": "Comport-2",
                            "joinStart": 3
                        }
                    ]
                }
            }
        ]
    }
}
```

## GenericComm Configuration Explanation

This configuration is meant for a Pro3 device, and instantiates one comport and one SSH session and links them to an eisc bridge to another processor slot on ipid 3.  Let's break down the ```Comport-1``` device.

```JSON
{
    "key": "Comport-1",
    "uid": 3,
    "name": "Comport 1",
    "group": "comm",
    "type": "genericComm",
    "properties": {
        "control": {
            "comParams": {
                "hardwareHandshake": "None",
                "parity": "None",
                "protocol": "RS232",
                "baudRate": 115200,
                "dataBits": 8,
                "softwareHandshake": "None",
                "stopBits": 1
            },
            "controlPortNumber": 1,
            "controlPortDevKey": "processor",
            "method": "com"
        }
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

The Type is the identifier for a specific type of device in Essentials.  A list of all valid types can be reported by using the consolecommand ```gettypes``` in Essentials.  In this case, the type is ```genericComm```.  This type is valid for any instance of a serial-based communications channel such as a Serial Port, SSH, UDP, or standard TCP/IP Socket.

**```Properties```**

These are the properties essential to the instantiation of the identified type.

#### Control

The properties within this property are dependant on the type of genericComm you wish to instantiate.  There is one common property for control of any type, and that is ```method```.  The ```method``` property requires a string that maps to the following enumerations in Essentials :

```cs
namespace PepperDash.Core
{
    // Summary:
    //     Crestron Control Methods for a comm object
    public enum eControlMethod
    {
        None = 0,
        Com = 1,
        IpId = 2,
        IpidTcp = 3,
        IR = 4,
        Ssh = 5,
        Tcpip = 6,
        Telnet = 7,
        Cresnet = 8,
        Cec = 9,
        Udp = 10,
    }
}
```

These enumerations are not case sensitive.  Not all methods are valid for a ```genericComm``` device.  For a comport, the only valid type would be ```Com```.  For a direct network socket, valid options are ```Ssh```, ```Tcpip```, ```Telnet```, and ```Udp```.

##### ComParams

A ```Com``` device requires a ```comParams``` object to set the properties of the comport.  The values of all properties are case-insensitive.

```JSON
{
"comParams": {
    "hardwareHandshake": "None",
    "parity": "None",
    "protocol": "RS232",
    "baudRate": 115200,
    "dataBits": 8,
    "softwareHandshake": "None",
    "stopBits": 1
}
```

**Valid ```hardwareHandshake``` values are as follows**

```sh
"None"
"Rts"
"Cts"
"RtsCts"
```

**Valid ```parity``` values are as follows**

```sh
"None"
"Even"
"Odd"
"Mark"
```

**Valid ```protocol``` values are as follows**

```sh
"RS232"
"RS422"
"RS485"
```

**Valid ```baudRate``` values are as follows**

```sh
300
600
1200
1800
2400
3600
4800
7200
9600
14400
19200
28800
38400
57600
115200
```

**Valid ```dataBits``` values are as follows**

```sh
7
8
```

**Valid ```softwareHandshake``` values are as follows**

```sh
"None"
"XON"
"XONT"
"XONR"
```

**Valid ```stopBits``` values are as follows**

```sh
1
2
```

Additionally, a ```control``` object for a physical hardware port needs to map to that physical port.  This is accomplished by utilizing the ```controlPortDevKey``` and ```port``` properties.

**```controlPortDevKey```**

This property maps to the ```key``` of the device upon which the port resides.

**```port```**

This property maps to the number of the port on the device you have mapped the relay device to.  Even if the device has only a single port, ```port``` must be defined.

##### TcpSshParams

A ```Ssh```, ```TcpIp```, or ```Udp``` device requires a ```tcpSshProperties``` object to set the propeties of the socket.

```Json
{
    "tcpSshProperties": {
        "address": "192.168.1.57",
        "port": 22,
        "username": "",
        "password": "",
        "autoReconnect": true,
        "autoReconnectIntervalMs": 10000
    }
}
```

**```address```**

This is the IP address, hostname, or FQDN of the resource you wish to open a socket to.  In the case of a UDP device, you can set either a single whitelist address with this data, or an appropriate broadcast address.

**```port```**

This is the port you wish to utilize for the socket connection.  Certain protocols require certain ports - ```Ssh``` being ```22``` and ```Telnet``` being ```23```.

**```username```**

This is the username (if required) for authentication to the device you are connecting to.  Typcally only required for ```Ssh``` connections.

**```password```**

This is the password (if required) for authentication to the device you are connecting to.  Typcally only required for ```Ssh``` connections.

**```autoreconnect```**

This is a boolean value, therefore it is a case-sensitive ```true``` or ```false``` utilized to determine if the socket will attempt to reconnect upon loss of connection.

**```autoReconnectIntervalMs```**

This is the duration of time, in Miliseconds, that the socket will wait before discrete connection attempts if ```autoreconnect``` is set to true.

##### The JoinMap

The join map for a generic comms device is fairly simple.  

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

```TextReceived``` is a stream of strings received **FROM** the connected device.

```SendText``` is for any strings you wish to send **TO** the connected device.

```Connect``` connects to a remote socket device on the rising edge of the signal.

```Connected``` represents the current connection state.  High for Connected, low for Disconnected.

```Status``` is an analog value that is representative of the connection states as reported by the SIMPL TCP/IP socket symbol.

All of the preceeding joins are set to join ```1```.  The second serial input join is reserved for ```2```.  It allows you to send a ```comparams``` json object as a string, utilizing the same format mentioned previously in this document.  Doing so will override the configured comport specifications.

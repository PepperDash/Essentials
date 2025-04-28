# Configuration Structure

---

[YouTube Video - Configuring PepperDash Essentials](https://youtu.be/EK8Ti9a1o7s)

***

The Essentials configuration structure is designed to allow minimum duplication of data across systems that share many similarities, which makes it ideally suited for applications where large numbers of duplicate room types must be deployed.

At a high level, the idea is to define a template of all of the common configuration shared by a group of systems of the same type.  Then individual differences per system instance can be defined in a system block that either add data missing in the template, or override the default values set in the template.

## Top Level Object Structure (Double Config)

```cs
{
    // This object is deserialized to type PepperDash.Essentials.Core.Config.EssentialsConfig

    "system_url":"", // For Portal use only
    "template_url":"", // For Portal use only
    "template":{
        // This object is deserialized to type PepperDash.Essentials.Core.EssentialsConfig
        // For most manually generated configuration, only define data here.  Leave system empty
    },
    "system":{
        // This object is deserialized to type PepperDash.Essentials.Core.EssentialsConfig
        // Any data here will be overlayed on top of the data in template.  In the case of duplicate values
        // the value in system will be overwrite any value in template
    }
}
```

## Object Structure for `template` and `system` (`PepperDash.Essentials.Core.EssentialsConfig`)

``` js
{
     "info": {
        // This object is deserialized to type PepperDash.Essentials.Core.Config.InfoConfig
        // Contains information about the system/configuration
     },
     "devices": [
        // This object is deserialized to type List<PepperDash.Essentials.Core.Config.DeviceConfig>
        // An array of devices
     ],
     "rooms": [
        // This object is deserialized to type List<PepperDash.Essentials.Core.Config.DeviceConfig>
        // An array of rooms.  These are not automatically deserialized
     ],
     "tielines":[
        // An array of tie lines that describe the connections between routing ports on devices
     ],
     "sourceLists":{
        // This object is deserialized to type Dictionary<string, Dictionary<string, PepperDash.Essentials.Core.SourceListItem>>
        // An object that contains a collection
     },
     "joinMaps":{
        // This object is deserialized to type Dictionary<string, string> where the value is a serialized class that inherits from JoinMapBase to be deserialized later
        // Used to define custom join maps for bridging devices to SIMPL
     }
}
```

## The Template and System Concept (Merging Configurations)

In order to understand how and why we use a double configuration concept, it's important to understand the relationship between a Template and a System in Portal.  A System represents a physical installed group of hardware(either currently or in the future), acting together usually as part of a single control system program.  A system MUST inherit from a Template.  A Template represents the common elements of one or more systems.

The idea being that configuration values that are common to all systems can be stored in the configuration for the template.  Then, any configuration values that are unique to a particular system cane be stored in the configuration of the System.  By "merging" the System configuration values over top of the Template configuration values, the resulting data contains all of the values that should be shared by each system that inherits from a common template, as well as the unique values for each individual system.

Below is an example of a double configuration containing both template and system properties.

```JSON
{
    "template": {
        "info": {
            "name": "Template Name",
            "description": "A 12 person conference room"
        },
        "devices": [

        ],
        "rooms": [

        ]
    },
    "system": {
        "info": {
            "name": "System Name",
            "myNewSystemProperty": "Some Value"
        },
        "devices": [

        ],
        "rooms": [

        ]
    }
}
```

Below is an example of the result of merging the above double configuration example into a single configuration.

```JSON
{
    "info": {
        "name": "System Name",                          // Since this property existed in both the template and system, the system value replaces the template value after the merge
        "description": "A 12 person conference room",   // This property existed only in the template and is unchanged after the merge
        "myNewSystemProperty": "Some Value"             // This property existed only in the system and is unchanged after the merge
    },
    "devices": [

    ],
    "rooms": [

    ]
}
```

---

## Device Object Structure

The devices array is meant to hold a series of device objects.  The basic device object structure is defined below.

```JSON
{
    "key": "someUniqueString",  // *required* a unique string
    "name": "A friendly Name",  // *required* a friendly name meant for display to users
    "type": "exampleType",      // *required* the type identifier for this object.  
    "group": "exampleGroup",    // *required* the group identifier for this object.  This really equates to a category for the device,
                                // such as "lighting" or "displays" and may be deprecated in future in favor of "category"
    "uid":0,                    // *required* a unique numeric identifier for each device
    "properties": {             // *required* an object where the configurable properties of the device are contained
        "control": {            // an object to contain all of the properties to connect to and control the device
            "method": "ssh",    // the control method used by this device
            "tcpSshProperties": {   // contains the necessary properties for the specified method
                "address": "1.2.3.4",
                "port": 22,
                "username": "admin",
                "password": "uncrackablepassword"
            }
        },
        "someCustomProperty": "I Love Tacos!"
    }
    // Do NOT add any custom data at the top level of the device object.  All custom data must be in the properties object.
}
```

Some additional details about specific properties that are important to note:

* "key": This value needs to be unique in the array of devices objects
* "uid": This value also needs to be unique for reasons related to configuration tools and template/system merging
* "type": Think of this as a way to identify what specific module you might associate with this device.  In Essentials, this value is used to determine what class will be instantiated for the device (ex. "necmpsx" or "samsungMdc" for two types of displays)
* "properties":  This object is used to store both specific and miscellaneous data about the device.
  * Specific data, like that shown above in the "control" object has a pre-defined structure.
  * Other data must be stored as objects or new properties inside the "properties" object such as "someCustomProperty" in the example above.
* Do NOT add any additional properties at the top level of the device object.  All custom data must be in the "properties" object.

## The Device Properties.Control Object

The control object inside properties has some reserved properties that are used by configuration tools and Essentials that require some caution.

```JSON
{
    "properties": {             // *required* an object where the configurable properties of the device are contained
        "control": {            // an object to contain all of the properties to connect to and control the device
            // Example of the reserved properties for a socket based port (ssh, tcpIp, udp)
            "method": "ssh",    // the control method used by this device
            "tcpSshProperties": {   // contains the necessary properties for the specified method
                "address": "1.2.3.4",               // IP Address or hostname
                "port": 22,
                "username": "admin",
                "password": "uncrackablepassword",
                "autoReconnect": true,              // If true, the client will attempt to re-connect if the connection is broken externally
                "AutoReconnectIntervalMs": 2000     // The time between re-connection attempts
            },

            // Example of the reserved properties for a Com port
            "method": "com",
            "controlPortNumber": 1,                 // The number of the com port on the device specified by controlPortDevKey
            "controlPortDevKey": "processor",       // The key of the device where the com port is located
            "comParams": {                          // This object contains all of the com spec properties for the com port
                "hardwareHandshake": "None",
                "parity": "None",
                "protocol": "RS232",
                "baudRate": 9600,
                "dataBits": 8,
                "softwareHandshake": "None",
                "stopBits": 1
            }
        }
    }
}
```

---

## Device Merging

The following examples illustrate how the device key and uid properties affect how devices are merged together in a double configuration scenario.  In order for a template device and a system device to merge, they must have the same key and uid values

```JSON
{
    "template": {
        "info": {
            "name": "Template Name",
            "description": "A 12 person conference room"
        },
        "devices": [
            {                                           // This is the template device
                "key": "display-1",
                "name": "Display",
                "type": "samsungMdc",
                "group": "displays",
                "uid":0,
                "properties": {
                    "control": {
                        "method": "ssh",
                        "tcpSshProperties": {
                            "address": "",              // Note that at the template level we won't know the actual IP address so this value is left empty
                            "port": 22,
                            "username": "admin",
                            "password": "uncrackablepassword"
                        }
                    }
                }
            }
        ],
        "rooms": [

        ]
    },
    "system": {
        "info": {
            "name": "System Name",
            "myNewSystemProperty": "Some Value"
        },
        "devices": [
            {                                           // This is the system device
                "key": "display-1",
                "uid":0,
                "properties": {
                    "control": {
                            "tcpSshProperties": {
                            "address": "10.10.10.10"    // Note that the actual IP address is specified at the system level
                        }
                    }
                }
            }
        ],
        "rooms": [

        ]
    }
}
```

Below is an example of the result of merging the above double configuration example into a single configuration.  

```JSON
{
    "info": {
        "name": "System Name",
        "description": "A 12 person conference room",
        "myNewSystemProperty": "Some Value"
    },
     "devices": [
        {
            "key": "display-1",
            "name": "Display",
            "type": "samsungMdc",
            "group": "displays",
            "uid":0,
            "properties": {
                "control": {
                    "method": "ssh",
                    "tcpSshProperties": {
                        "address": "10.10.10.10",   // Note that the merged device object inherits all of the template
                                                    // properties and overwrites the template address property with the system value
                        "port": 22,
                        "username": "admin",
                        "password": "uncrackablepassword"
                    }
                }
            }
        }
     ],
     "rooms": [

     ]
}
```

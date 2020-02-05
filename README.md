
# PepperDash Essentials Framework

## Overview
Essentials Framework is a collection of C# / Simpl# Pro libraries that can be utilized in several different manners. It is currently operating as a 100% configuration-driven system, and can be extended to add different workflows and behaviors, either through the addition of further device "types" or via the plug-in mechanism. The framework is a collection of "things" that are all related and interconnected, but in general do not have dependencies on each other.

## Minimum Requirements
- Essentials Framework runs on any Crestron 3-series processor or Crestron's VC-4 platform.
- To edit and compile the source, Microsoft Visual Studio 2008 Professional with SP1 is required.
- Crestron's Simpl# Plugin is also required (must be obtained from Crestron).

## Utilization
Essentials was originally conceptualized as a standalone application for running control system logic entirely in Simpl# Pro. It is primarily designed around accomplishing this goal, but during development, it became obvious that it could easily be leveraged to also serve as a partner application to one or more SIMPL Windows programs.

Utilization of Essentials Framework falls into the following categories:

1. Standalone Control System Application for controlling one or more rooms

2. Partner Application to a SIMPL Windows program. This allows for several useful advantages

- Dynamic device instantiation. Devices can be defined in configuration and instantiated at runtime and then "bridged" to a SIMPL Windows program via EISC.

- Advanced logic. Some logic operations that cannot be affectively accomplished in SIMPL Windows (ex. JSON/XML serialization/deserialization, database operations, etc.) can be done in the Simpl# Pro environment and the necessary input and output "bridged" to a SIMPL Windows program via EISC.

3. Hybrid Application that may contain elements of both standalone control and SIMPL partner application integration.
- There may be a use case where a device can only be defined in a single application, but that device may need to be interacted with from multiple applications.  The device can be defined in an Essentials application, interacted with in that application and also "bridged" to one or more SIMPL Windows applications.

 ## Documentation
 For detailed documentation, follow this [LINK](https://github.com/PepperDash/EssentialsFramework/wiki) to the Wiki.

## How-To (Getting Started)
To help understand Essentials Framework, we recommend starting with the current [Example Build]() and loading it to a Crestron 3-Series processor.

1. First, load the PepperDashEssentials.cpz to the processor in program slot 1 and start the program.
2. On first boot, the Essentials Application will build the necessary configuration folder structure in the User/Program1/ path.
3. Load the ExampleEssentialsConfigurationFile.json to the User/Program1/ folder.
4. Reset the program via console (progreset -p:1).  The program will load the example configuration file.
5. Launch the EssentialsExampleXpanel.vtz project.  You can interact with the program (which uses simulated device logic to emulate a real commercial huddle room with presentation, audio and video calling capabilities).
6. Via console, you can run the (**devlist:1**) command to get some insight into what has been loaded from the configuration file into the system .  This will print the basic device information in the form of ["key"] "Name".  The "key" value is what we can use to interact with each device uniquely.
7. Run the command (**devprops:1 display-1**).  This will print the real-time property values of the device with key "display-1".
8. Run the command (**devmethods:1 display-1**).  This will print the public methods available for the device with key "display-1".
9. Run the command (**devjson:1 {"deviceKey":"display-1","methodName":"PowerOn", "params": []}**).  This will call the method PowerOn() on the device with key "display-1".



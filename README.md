
# PepperDash Essentials Framework (c) 2020

## License
Provided under MIT license

## Overview
PepperDash Essentials is an open source Crestron framework that can be configured as a standalone program capable of running a wide variety of system designs and can also be utilized as a plug-in architecture to augment other Simpl# Pro and Simpl Windows programs.

Essentials Framework is a collection of C# / Simpl# Pro libraries that can be utilized in several different manners. It is currently operating as a 100% configuration-driven system, and can be extended to add different workflows and behaviors, either through the addition of further device "types" or via the plug-in mechanism. The framework is a collection of "things" that are all related and interconnected, but in general do not have dependencies on each other.

## Minimum Requirements
- Essentials Framework runs on any Crestron 3-series processor, **4-series** processor or Crestron's VC-4 platform.
- To edit and compile the source, Microsoft Visual Studio 2008 Professional with SP1 is required.
- Crestron's Simpl# Plugin is also required (must be obtained from Crestron).

## Dependencies

The [PepperDash.Core](https://github.com/PepperDash/PepperDashCore) SIMPL# library is required.  It is referenced as a submodule and will be automatically checked out when cloning this repo if set to recurse submodules.  This allows different builds of the PepperDash.Core library to be referenced by checking out the desired submodule commit.

## Utilization
Essentials was originally conceptualized as a standalone application for running control system logic entirely in Simpl# Pro. It is primarily designed around accomplishing this goal, but during development, it became obvious that it could easily be leveraged to also serve as a partner application to one or more SIMPL Windows programs.

Utilization of Essentials Framework falls into the following categories:

1. Standalone Control System Application for controlling one or more rooms. See [Standalone Use](https://github.com/PepperDash/Essentials/wiki/Standalone-Use#standalone-application)

2. Partner Application to a SIMPL Windows program. This allows for several useful advantages. See [SIMPL Windows Bridging](https://github.com/PepperDash/Essentials/wiki/SIMPL-Bridging#simpl-windows-bridging)

- Dynamic device instantiation. Devices can be defined in configuration and instantiated at runtime and then bridged to a SIMPL Windows program via EISC.

- Advanced logic. Some logic operations that cannot be affectively accomplished in SIMPL Windows (ex. JSON/XML serialization/deserialization, database operations, etc.) can be done in the Simpl# Pro environment and the necessary input and output bridged to a SIMPL Windows program via EISC.

3. Hybrid Application that may contain elements of both standalone control and SIMPL partner application integration.
- There may be a use case where a device can only be defined in a single application, but that device may need to be interacted with from multiple applications.  The device can be defined in an Essentials application, interacted with in that application and also bridged to one or more SIMPL Windows applications.

 ## Documentation
 For detailed documentation, see the [Wiki](https://github.com/PepperDash/EssentialsFramework/wiki).

## How-To (Getting Started)

See [Getting Started](https://github.com/PepperDash/Essentials/wiki/Get-started#how-to-get-started)



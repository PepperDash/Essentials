# PepperDash Essentials Framework

## [Releases](https://github.com/PepperDash/Essentials/releases)

2.x releases are tagged `v2.x.y`. The latest release overall is 3.x.

## License

Provided under the [MIT license](LICENSE.md).

## Overview

PepperDash Essentials is an open source Crestron framework that can be configured as a standalone program capable of running a wide variety of system designs and can also be utilized as a plug-in architecture to augment other Simpl# Pro and Simpl Windows programs.

Essentials Framework is a collection of C# / Simpl# Pro libraries that can be utilized in several different manners. It is currently operating as a 100% configuration-driven system, and can be extended to add different workflows and behaviors, either through the addition of further device "types" or via the plug-in mechanism. The framework is a collection of "things" that are all related and interconnected, but in general do not have dependencies on each other.

## Versions

This branch, `essentials-2x`, is Essentials 2.x. Each major version has its own branch, and new development happens on `essentials-3x`:

| Branch | Essentials version | Target framework | Runs on |
|---|---|---|---|
| `essentials-3x` (the default branch) | 3.x | .NET 8 | 4-series processors and VC-4 |
| `essentials-2x` | 2.x | .NET Framework 4.7.2 | 4-series processors and VC-4 |
| `essentials-1x` | 1.x | .NET Compact Framework 3.5 | 3-series processors |

Releases are published on [GitHub](https://github.com/PepperDash/Essentials/releases) and as the [`PepperDashEssentials`](https://www.nuget.org/packages/PepperDashEssentials) NuGet package.

## Building

You'll need the [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download). Essentials 2.x targets .NET Framework 4.7.2, and the .NET 9 SDK builds it with the right references.

```
git clone -b essentials-2x https://github.com/PepperDash/Essentials.git
cd Essentials
dotnet build
```

Everything Essentials needs is in this repository or comes from NuGet, including PepperDash Core (`src/PepperDash.Core`) and the Crestron SDK. The program's `.cpz` is written to `src/PepperDash.Essentials/bin/Debug/net472/`. See [Get started](https://pepperdash.github.io/Essentials/v2/docs/Get-started.html) for loading it onto a processor.

## Utilization

Essentials was originally conceptualized as a standalone application for running control system logic entirely in Simpl# Pro. It is primarily designed around accomplishing this goal, but during development, it became obvious that it could easily be leveraged to also serve as a partner application to one or more SIMPL Windows programs.

Utilization of Essentials Framework falls into the following categories:

1. Standalone Control System Application for controlling one or more rooms. See [Standalone Use](https://pepperdash.github.io/Essentials/v2/docs/usage/Standalone-Use.html).

2. Partner Application to a SIMPL Windows program. This allows for several useful advantages. See [SIMPL Windows Bridging](https://pepperdash.github.io/Essentials/v2/docs/usage/SIMPL-Bridging-Updated.html).

- Dynamic device instantiation. Devices can be defined in configuration and instantiated at runtime and then bridged to a SIMPL Windows program via EISC.

- Advanced logic. Some logic operations that cannot be effectively accomplished in SIMPL Windows (ex. JSON/XML serialization/deserialization, database operations, etc.) can be done in the Simpl# Pro environment and the necessary input and output bridged to a SIMPL Windows program via EISC.

3. Hybrid Application that may contain elements of both standalone control and SIMPL partner application integration.

- There may be a use case where a device can only be defined in a single application, but that device may need to be interacted with from multiple applications. The device can be defined in an Essentials application, interacted with in that application and also bridged to one or more SIMPL Windows applications.

To add device types without changing Essentials itself, write a [plugin](https://pepperdash.github.io/Essentials/v2/docs/technical-docs/Plugins.html).

## Documentation

For detailed documentation, see the [documentation site](https://pepperdash.github.io/Essentials/). It has the [v3](https://pepperdash.github.io/Essentials/v3/) and [v2](https://pepperdash.github.io/Essentials/v2/) docs.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for how to report issues and submit changes, which branch to target, and how commit messages drive releases.

## Room Combiner Operation Lifecycle

Essentials room combiner now exposes an operation lifecycle that clients can use to show progress and terminal status during scenario changes.

Lifecycle states:
- Idle
- InProgress
- Completed
- Failed
- TimedOut

The lifecycle is exposed on room combiners that implement the operation-status extension and is included in app server combiner messages under the combinationOperation payload.

Configuration:
- combinationOperationTimeoutSeconds (optional): timeout in seconds for an in-progress operation.
- Default when omitted: 300 seconds.
- Values less than or equal to 0 are treated as invalid and fall back to the default.

Example room combiner properties:

```json
{
	"defaultToManualMode": false,
	"defaultScenarioKey": "divided",
	"combinationOperationTimeoutSeconds": 30,
	"roomKeys": ["roomA", "roomB"]
}
```

Operational note:
- When an operation reaches a terminal state (Completed, Failed, TimedOut), the operation timeout timer is stopped.

## Support

* Check out our [Discord Server](https://discord.gg/6Vh3ssDdPs)

## How-To (Getting Started)

See [Get started](https://pepperdash.github.io/Essentials/v2/docs/Get-started.html).

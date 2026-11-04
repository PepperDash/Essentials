# Get started

---
[YouTube Video - Getting Started with PepperDash Essentials](https://youtu.be/FxEZtbpCwiQ)
***

## Get a CPZ

### Prerequisites

* [VS Code](https://code.visualstudio.com/)
* [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
* [Git](https://git-scm.com/)

> Note: Essentials 3.x targets .NET 8 (`net8`) and runs on 4-series processors and VC-4. The .NET 9 SDK is needed because the test projects target `net9.0`

### Build From Source

1. Clone the repo: `git clone https://github.com/PepperDash/Essentials.git`. The default branch, `essentials-3x`, is Essentials v3
2. Open the folder in VS Code
3. Build using the dotnet CLI: `dotnet build`
4. The CPZ (`PepperDashEssentials.<version>.net8.cpz`) is written to `src/PepperDash.Essentials/bin/Debug/net8/` and copied to the `output` folder at the repo root

### Download the latest release

Essentials v3 builds are published as pre-releases (tags `v3.0.0-rc.*`) on [Github](https://github.com/PepperDash/Essentials/releases). Download the `PepperDashEssentials.<version>.net8.cpz` asset. The release marked "Latest" is the 2.x line.

## How to Get Started

1. Using an SFTP client or Crestron Toolbox, load the downloaded (or built) cpz to the processor in program slot 1. On VC-4, add the cpz as a program and create a room for it in the VC-4 web interface.
   1. If using SFTP, connect via SSH and start the program by sending console command `progload -p:1`
2. On first boot, the Essentials Application will build the necessary configuration folder structure (`ir`, `sgd`, `plugins` and `joinmaps` folders) in the `/user/program1/` path. On VC-4 the path is the room's `User/` folder.
3. Copy a configuration file to the `/user/program1/` folder. The file name must contain `configurationFile` and end in `.json`. Essentials v3 has no built-in room types, so a room in the configuration needs a room plugin in the `plugins` folder. For a complete working example that needs no hardware, see the [Essentials v3 demo](https://pepperdash.github.io/EssentialsDemoConfig/).
4. Reset the program via console `progreset -p:1`. The program will load the configuration file.

Once Essentials is running, open [Essentials Developer Tools](~/docs/usage/Developer-Tools.md) in a browser at `https://[processor-ip]/cws/debug/` to see its devices, config, routing and live log. Dev Tools ships with Essentials, so there's nothing else to load.

The following console commands also show what's going on:

* ```devlist:1```
  * Print the list of devices in [{key}] {name} format
  * The key of a device can be used with the rest of the commands to get more information
* `devprops:1 {deviceKey}`
  * Print the real-time property values of the device with key `{deviceKey}`.
* `devmethods:1 display-1`
  * Print the public methods available for the device with key "display-1".
* `devjson:1 {"deviceKey":"display-1","methodName":"PowerOn", "params": []}`
  * Call the method `PowerOn()` on the device with key "display-1".

Next: [Standalone use](~/docs/usage/Standalone-Use.md)

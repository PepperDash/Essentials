# Get started

---
[YouTube Video - Getting Started with PepperDash Essentials](https://youtu.be/FxEZtbpCwiQ)
***

## Get a CPZ

### Prerequisites

* [VS Code](https://code.visualstudio.com/)
* [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
* [Git](https://git-scm.com/)

> Note: Essentials 2.x.x uses .NET Framework 4.7.2 currently. The .NET 9 SDK will build the project with the appropriate references

### Build From Source

1. Clone the repo: `git clone https://github.com/PepperDash/Essentials.git`
2. Open the folder in VS Code
3. Build using the dotnet CLI: `dotnet build`

### Download the latest release

The latest release can be found on [Github](https://github.com/PepperDash/Essentials/releases/latest)

## How to Get Started

2. Using an SFTP client or Crestron Toolbox, load the downloaded (or built) cpz to the processor in program slot 1
   1. If using SFTP, connect via SSH and start the program by sending console command `progload -p:1`
3. On first boot, the Essentials Application will build the necessary configuration folder structure in the user/program1/ path.
4. The application has some example configuration files included. Copy `/Program01/Example Configuration/EssentialsSpaceHuddleRoom/configurationFile-HuddleSpace-2-Source.json` to the `/User/Program1/` folder.
6. Reset the program via console `progreset -p:1`. The program will load the example configuration file.

Once Essentials is running with a valid configuration, the following console commands can be used to see what's going on:

* ```devlist:1```
  * Print the list of devices in [{key}] {name} format
  * The key of a device can be used with the rest of the commands to get more information
* `devprops:1 {deviceKey}`
  * Print the real-time property values of the device with key "display-1".
* `devmethods:1 display-1`
  * Print the public methods available for the device with key "display-1".
* `devjson:1 {"deviceKey":"display-1","methodName":"PowerOn", "params": []}`
  * Call the method `PowerOn()` on the device with key "display-1".

Next: [Standalone use](~/docs/usage/Standalone-Use.md)

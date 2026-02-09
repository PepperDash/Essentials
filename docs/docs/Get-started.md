# Get started

---
[YouTube Video - Getting Started with PepperDash Essentials](https://youtu.be/FxEZtbpCwiQ)
***

## Download or clone

You may clone Essentials at <https://github.com/PepperDash/Essentials.git>

## How to Get Started

This section assumes knowledge of loading programs to and working with the file system on a Crestron processor.

1. Using an SFTP client, load `PepperDashEssentials1.4.32.cpz` to the processor in program slot 1 and start the program by sending console command `progload -p:1`
1. On first boot, the Essentials Application will build the necessary configuration folder structure in the User/Program1/ path.
1. The application has some example configuration files included. Copy `/Program01/Example Configuration/EssentialsSpaceHuddleRoom/configurationFile-HuddleSpace-2-Source.json` to the `/User/Program1/` folder.
1. Copy the SGD files from `/Program01/SGD` to `/User/Program1/sgd`
1. Reset the program via console `progreset -p:1`. The program will load the example configuration file.
1. Via console, you can run the `devlist:1` command to get some insight into what has been loaded from the configuration file into the system . This will print the basic device information in the form of ["key"] "Name". The "key" value is what we can use to interact with each device uniquely.
1. Run the command `devprops:1 display-1`. This will print the real-time property values of the device with key "display-1".
1. Run the command `devmethods:1 display-1`. This will print the public methods available for the device with key "display-1".
1. Run the command `devjson:1 {"deviceKey":"display-1","methodName":"PowerOn", "params": []}`. This will call the method PowerOn() on the device with key "display-1".
1. Run the provided example XPanel SmartGraphics project and connect to your processor at the appropriate IPID.

Next: [Standalone use](~/docs/usage/Standalone-Use.md)

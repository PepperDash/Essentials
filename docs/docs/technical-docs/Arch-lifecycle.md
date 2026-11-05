# Essentials Configurable System Lifecycle

The diagram below describes how Essentials gets a program up and running.

```mermaid
flowchart TD
    Start(["Program starts:<br/>ControlSystem.InitializeSystem()"]) --> Platform["Determine the platform (4-series or VC-4)<br/>and the user folder path"]
    Platform --> WebApi["Add the Essentials web API device<br/>(used by Essentials Dev Tools)"]
    WebApi --> SkipLoad{"donotloadonnextboot<br/>set?"}
    SkipLoad -- Yes --> Wait(["Wait for the go console command"])
    Wait --> Assemblies
    SkipLoad -- No --> Assemblies["Register the program's own assemblies<br/>and built-in device factories"]
    Assemblies --> Folders{"User folder<br/>already exists?"}
    Folders -- No --> Create(["Create the folder structure<br/>(ir, sgd, plugins, joinmaps) and stop:<br/>load config and plugins, then restart"])
    Folders -- Yes --> Plugins

    subgraph PL["Plugin loading (see Plugins)"]
        Plugins["Load plugins from the plugins folder<br/>and register their device types"]
    end

    Plugins --> Config["Read the config file<br/>(*configurationFile*.json)"]
    Config --> VersionCheck["Compare loaded plugin versions with<br/>the config's versions block"]
    VersionCheck --> Devices

    subgraph Load["ControlSystem.Load()"]
        Devices["Build devices: processor, routing feedback manager,<br/>system monitor (4-series only), then each config device<br/>through DeviceFactory. A device's factory may create<br/>comm devices through CommFactory"]
        Devices --> Rooms["Build rooms through DeviceFactory"]
        Rooms --> Activate

        subgraph AP["DeviceManager.ActivateAll()"]
            Activate["Pre-activation: each device's<br/>pre-activation actions"] --> Act2["Activation: each device's<br/>CustomActivate()"]
            Act2 --> Act3["Post-activation: each device's<br/>post-activation actions"]
        end

        Act3 --> TieLines["Build tie lines from config"]
    end

    Act3 -. "AllDevicesActivated event" .-> Init["Each EssentialsDevice runs<br/>Initialize() on a background task"]
    TieLines --> Done(["Program initialization complete"])
```

The activation phases are covered in more detail on the [next page](~/docs/technical-docs/Arch-activate.md), and plugin loading on the [Plugins](~/docs/technical-docs/Plugins.md) page.

Next: [Activation phases](~/docs/technical-docs/Arch-activate.md)

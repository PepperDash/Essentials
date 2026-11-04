# Methods of Debugging

Use the Debug class features built into the PepperDash.Core library.

- Pros:
  - Can be easily enabled from console
  - Allows for setting the level of verbosity
  - Works when troubleshooting remotely and doesn't require a connection to the CS interface of the processor.
  - Allows for logging to the Crestron error log, a rolling log file on the processor, and a websocket debug console
  - Works regardless of build type setting (debug/release)
  - Can easily identify which class instance is generating console messages, and filter console messages by device key (`appdebugfilter`)
  - Can use console commands to view the state of public properties on devices
  - Can use console commands to call methods on devices
  - Doesn't stop the program
- Cons:
  - No detailed object inspection in real time
  - Only prints console statements already in code
  - When enabled at the highest level of verbosity, it can produce a significant amount of data in console. Can be hard to find messages easily.
  - On VC-4 there is no program console to watch; use the log files or the live log in [Essentials Developer Tools](~/docs/usage/Developer-Tools.md) instead

## How to use the PepperDash.Core Debug Class

The majority of interaction is done via console, preferably via an SSH session through Crestron Toolbox, PuTTy or any other suitable application.

In code, logging is done with the Serilog-based methods on the `Debug` class in the `PepperDash.Core` namespace: `Debug.LogVerbose()`, `Debug.LogDebug()`, `Debug.LogInformation()`, `Debug.LogWarning()`, `Debug.LogError()` and `Debug.LogFatal()`, or `Debug.LogMessage()` with a Serilog `LogEventLevel`. Each takes a message template and optional arguments, and there are overloads that take an `IKeyed` (such as a device) and/or an `Exception`. Devices can also use the extension methods of the same names, for example `this.LogInformation(...)`. A message is written to each output (console, error log, log file, websocket) only if its level is at or above the minimum level set for that output.

All statements printed to console are prefixed by a timestamp which can be greatly helpful in debugging order of operations.

```cs
// The most basic use, sets the level and the message to print.
Debug.LogInformation("Hello World");
// prints: [timestamp][INFO][App 1] Hello World

// The message is a Serilog message template that takes params object[] items
string world = "World";
Debug.LogInformation("Hello {world}", world);
// prints: [timestamp][INFO][App 1] Hello World

// The overloads (and extension methods) that take an IKeyed print the Key of the device
// in console to help identify the class instance the message originated from
this.LogInformation("Hello World");
// prints: [timestamp][INFO][App 1][deviceKey] Hello World

// Overloads that take an Exception print the exception after the message
this.LogError(ex, "Hello World");
```

Log files are written in compact JSON to `user/debug/app{N}/` under the application root on a processor, or `user/debug/room{roomId}/` on VC-4. A new file (`global-log{yyyyMMdd}.log`) is started each day, and 30 days (60 on VC-4) are kept. The `debugappurl` console command prints the URL of [Essentials Developer Tools](~/docs/usage/Developer-Tools.md) (`/cws/debug`), whose Debug Console shows the live log and can filter it by level and device.

## Console Commands

### General Console Commands

Below are is a non-exhaustive list of some of the Essentials specific console commands that allow interaction with the application at runtime.

### `help user`

Will print the available console commands for each program slot.  Console commands can be added and removed dynamically by Essentials and may vary by the version of Essentials that is running.  This is the best place to start to determine the available commands registered for each instance of Essentials running on a processor.

### `reportversions:[slot]`

Will print the running versions of all .dll libraries.  Useful for determining the exact build version of the Essentials application and all plugins

### `gettypes:[slot] [searchString(optional)]`

The `searchString` value is an optional parameter to filter the results.

Will print all of the valid `type` values registered in the `DeviceFactory` for the running Essentials application.  This helps when generating config structure and defining devices.  Device types added by plugins will also be shown.

### `showconfig:[slot]`

Will print out the merged config object

### `donotloadonnextboot:[slot] [true/false]`

When the value is set to true, Essentials will pause when starting up, to allow for a developer to attach to the running process from an IDE for purposes of step debugging.  Once attached, issuing the command `go:[slot]` will cause the configuration file to be read and the program to initialize.  This value gets set to false when the `go` command is issues.

### DeviceManager Console Commands

The following console commands all perform actions on devices that have been registered with the `PepperDash.Essentials.Core.DeviceManager` static class

### `Appdebug:[slot] [sink (optional)] [level] [timeoutMinutes (optional)]`

Gets or sets the minimum log level. With no arguments, prints the current level for each sink.

- `sink` is one of `console` (the default), `errorlog`, `websocket`, `file` or `all`.
- `level` is a level name, `Verbose`, `Debug`, `Information`, `Warning`, `Error` or `Fatal` (not case-sensitive), or a number: `0` = Information, `1` = Debug, `2` = Verbose, `3` = Warning, `4` = Error, `5` = Fatal. The numbers 0-2 mean the same as in Essentials 2.x.
- `timeoutMinutes` applies to the console (and `all`). After it (default 120 minutes), a console level of `Debug` or `Verbose` resets to `Information`, so verbose logging isn't left on by accident.

`appdebug:[slot] ?` prints the usage.

Example:

```sh
RMC4>appdebug:1 // Gets current levels
Console log level = Information
File log level = Information
Error log level = Information
Websocket log level = Information

RMC4>appdebug:1 debug // Sets console level to Debug (Debug and higher will print)
[Application 1], Debug level set to Debug

RMC4>appdebug:1 2 30 // Sets console level to Verbose for 30 minutes
[Application 1], Debug level set to Verbose
```

### `Appdebugfilter:[slot] [+/-keys]`

Filters the console output by device key; the log file, error log and websocket outputs are not filtered. By default every key is shown, and `-key` hides a device's messages while `+key` shows them again. Start with `-all` to hide every key except the ones you then add with `+key`; `+all` clears the filter. Messages without a device key always print. `appdebugfilter:[slot] ?` prints the usage.

```sh
RMC4>appdebugfilter:1 -display-1        // Hide display-1's messages
RMC4>appdebugfilter:1 -all +codec-1     // Show only codec-1's messages
RMC4>appdebugfilter:1 +all              // Show every key again
```

### `getjoinmap:[slot] [bridgeKey] [deviceKey (optional)]`

For use with SIMPL Bridging.  Prints the join map for the specified bridge.  If a device key is specified, only the joins for that device will be printed.

### `Devlist:[slot]`

Gets the current list of devices from `DeviceManager`

Prints in the form [deviceKey] deviceName

Example:

```sh
// Get the list of devices for program 1
RMC3>devlist:1

RMC3>[16:34:05.819]App 1:28 Devices registered with Device Mangager:
[16:34:05.834]App 1:  [cec-1] Tx 5 cec 1
[16:34:05.835]App 1:  [cec-1-cec]
[16:34:05.835]App 1:  [cec-5] Rmc 1 cec 1
[16:34:05.836]App 1:  [cec-5-cec]
[16:34:05.836]App 1:  [cec-6] Dm Chassis In 1 cec 1
[16:34:05.837]App 1:  [cec-6-cec]
[16:34:05.837]App 1:  [cec-7] Dm Chassis Out 1 cec 1
[16:34:05.838]App 1:  [cec-7-cec]
[16:34:05.838]App 1:  [comm-1] Generic comm 1
[16:34:05.838]App 1:  [comm-1-com]
[16:34:05.839]App 1:  [comm-2] Rmc comm 1
[16:34:05.839]App 1:  [comm-2-com]
[16:34:05.840]App 1:  [comm-3] Rmc comm 2
[16:34:05.840]App 1:  [comm-3-com]
[16:34:05.841]App 1:  [dmMd8x8-1] DM-MD8x8 Chassis 1
[16:34:05.842]App 1:  [dmRmc100C-1] DM-RMC-100-C Out 3
[16:34:05.843]App 1:  [dmRmc200C-1] DM-RMC-200-C Out 2
[16:34:05.843]App 1:  [dmRmc4kScalerC-1] DM-RMC-4K-SCALER-C Out 1
[16:34:05.844]App 1:  [dmTx201C-1] DM-TX-201C 1
[16:34:05.845]App 1:  [eisc-1A]
[16:34:05.845]App 1:  [gls-odt-1] GLS-ODT-CN 1
[16:34:05.846]App 1:  [gls-oir-1] GLS-OIR-CN 1
[16:34:05.846]App 1:  [processor]
[16:34:05.847]App 1:  [ssh-1] Generic SSH 1
[16:34:05.847]App 1:  [ssh-1-ssh]
[16:34:05.848]App 1:  [systemMonitor]
[16:34:05.848]App 1:  [tcp-1] Generic TCP 1
[16:34:05.849]App 1:  [tcp-1-tcp]
```
### `Setdevicestreamdebug:[slot] [devicekey] [both/rx/tx/off] [timeoutMinutes (optional)]`

Enables debug for communication on a single device. Stream debugging turns off after `timeoutMinutes` (default 30).

Example:

```sh
PRO3>setdevicestreamdebug:1 lights-1-com both

[13:13:57.000]App 1:[lights-1-com] Sending 4 characters of text: 'test'

PRO3>setdevicestreamdebug:1 lights-1-com off
```

### `Devprops:[slot][devicekey]`

Gets the list of public properties on the device with the corresponding `deviceKey`

Example:

```sh
// Get the properties on the device with Key 'cec-1-cec'
// This device happens to be a CEC port on a DM-TX-201-C's HDMI input
RMC3>devprops:1 cec-1-cec
[
  {
    "Name": "IsConnected",
    "Type": "Boolean",
    "Value": "True",
    "CanRead": true,
    "CanWrite": false
  },
  {
    "Name": "Key",
    "Type": "String",
    "Value": "cec-1-cec",
    "CanRead": true,
    "CanWrite": true
  },
  {
    "Name": "Name",
    "Type": "String",
    "Value": "",
    "CanRead": true,
    "CanWrite": true
  },
  {
    "Name": "Enabled",
    "Type": "Boolean",
    "Value": "False",
    "CanRead": true,
    "CanWrite": true
  }
]

RMC3>

```

### `Devmethods:[slot][devicekey]`

Gets the list of public methods available on the device

Example:

```sh
// Get the methods on the device with Key 'cec-1-cec'
RMC3>devmethods:1 cec-1-cec
[
  {
    "Name": "SendText",
    "Params": [
      {
        "Name": "text",
        "Type": "String"
      }
    ]
  },
  {
    "Name": "SendBytes",
    "Params": [
      {
        "Name": "bytes",
        "Type": "Byte[]"
      }
    ]
  },
  {
    "Name": "SimulateReceive",
    "Params": [
      {
        "Name": "s",
        "Type": "String"
      }
    ]
  },
  //... Response abbreviated for clarity ...
]

RMC3>
```

### `Devjson:[slot][json formatted object {"devicekey", "methodname", "params"}]`

Used in conjunction with devmethods, this command allows any of the public methods to be called from console and the appropriate arguments can be passed in to the method via a JSON object.

This command is most useful for testing without access to hardware as it allows both simulated input and output for a device.

Example:

```sh
// This command will call the SendText(string text) method on the
// device with the Key 'cec-1-cec' and pass in "hello world" as the
// argument parameter.  On this particular device, it would cause
// the string to be sent via the CEC Transmit
RMC3>devjson:1 {"deviceKey":"cec-1-cec", "methodName":"SendText", "params": ["hello world\r"]}

// This command will call SimulateReceive(string text) on the device with Key 'cec-1-cec'
// This would simulate receiving data on the CEC port of the DM-TX-201-C's HDMI input
RMC3>devjson:1 {"deviceKey":"cec-1-cec", "methodName":"SimulateReceive", "params": ["hello citizen of Earth\r"]}
```

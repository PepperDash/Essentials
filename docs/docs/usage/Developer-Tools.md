# Essentials Developer Tools

Essentials Developer Tools (Dev Tools) is a web app for inspecting and troubleshooting a running Essentials program from a browser. It ships with Essentials: the Essentials v3 `.cpz` includes it, and the program serves it from the processor or VC-4 room it runs on. There's nothing to install.

Dev Tools is open source, at [PepperDash/essentials-devtools](https://github.com/PepperDash/essentials-devtools). Its documentation, including tutorials, how-to guides and a reference for every page, is in that repository's [docs folder](https://github.com/PepperDash/essentials-devtools/tree/main/docs), and the same pages are built into the app under **Help**.

## What it does

Dev Tools has a page for each of these, for the program slot you choose in its top bar:

- **Versions**: the loaded assemblies and their versions, including plugins
- **API Paths**: the Essentials web API routes the program serves
- **Initialization Exceptions** and **Plugin Load Issues**: errors from startup and plugin loading (the same information as the `pluginissues` console command)
- **Config File**: the merged configuration the program is running
- **Devices**: every device in the `DeviceManager`, with its properties, feedbacks and methods; you can call a method from here
- **Types**: the device types the program can build, including types from loaded plugins (the same list as `gettypes`)
- **Routing**: a live diagram of the routing devices and tie lines, color-coded by signal type, where you can trace a signal path and make or clear routes
- **Mobile Control**: the Direct Server settings and UI clients, with each client's token and URL
- **Secrets**: the credentials stored on the processor for use in config
- **Debug Console**: the program's live log, with filtering by level and device

## Open Dev Tools

Essentials logs the address at startup (**Developer Tools Web App available at**), and the `debugappurl` console command prints it.

# [4-series processor](#tab/4series)

```text
https://[processor-ip]/cws/debug/
```

Sign in with the processor's username and password. If several Essentials programs are running, choose one from the application selector in the top bar (`app01` to `app10`).

# [VC-4](#tab/vc4)

```text
https://[vc4-server]/VirtualControl/Rooms/[RoomId]/cws/debug/
```

`[RoomId]` is the room's ID from the **Program Instances** page of the VC-4 web interface. See [Use Dev Tools on VC-4](https://github.com/PepperDash/essentials-devtools/blob/main/docs/how-to/use-dev-tools-on-vc4.md) for signing in and how the Debug Console and routing connect there.

---

The processor or server uses a self-signed certificate, so the browser shows a security warning the first time. Accept it to continue.

## How it's loaded

The Essentials `.cpz` contains an `essentials-devtools*.zip`. When the program starts and finds that zip in its application folder (where the `.cpz` was loaded), Essentials unzips it into the `html/debug` folder, then deletes the zip. On a 4-series processor that folder is `/html/debug`; on VC-4 it's the room's own `html/debug` folder. Essentials serves the app from there at `/cws/debug`. To load a different Dev Tools version, put its `essentials-devtools-[version].zip` in the application folder and restart the program; only one `essentials-devtools*.zip` may be present.

Dev Tools gets everything it shows from the Essentials web API, served by the `essentialsWebApi` device that Essentials adds at startup. See the Dev Tools [API Endpoints reference](https://github.com/PepperDash/essentials-devtools/blob/main/docs/reference/api-endpoints.md) for the routes it uses.

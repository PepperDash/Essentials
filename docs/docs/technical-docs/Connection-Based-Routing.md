# Essentials connection-based routing

## TL;DR

Routing is defined by a connection graph or a wiring diagram. Routeable devices are sources, midpoints, or destinations. Devices are connected by tie lines. Tie lines represent the cables connecting devices, and are of type audio, video or both. Routes are made by telling a destination to get an audio/video/combined route from a source.

## Summary

Essentials routing is described by defining a graph of connections between devices in a system, typically in configuration. The audio, video and combination connections are like a wiring diagram. This graph is a collection of devices and tie lines, each tie line connecting a source device, source output port, destination device and destination input port. Tie lines are logically represented as a collection.

When routes are to be executed, Essentials will use this connection graph to decide on routes from source to destination. A method call is made on a destination, which says “destination, find a way for source xyz to get to you.” An algorithm analyzes the tie lines, instantly walking backwards from the destination, down every connection until it finds a complete path from the source. If a connected path is found, the algorithm then walks forward through all midpoints to the destination, executing switches as required until the full route is complete. The developer or configurer only needs to say “destination, get source xyz” and Essentials figures out how, regardless of what devices lie in between.

### Classes Referenced

* `PepperDash.Essentials.Core.IRoutingSource`
* `PepperDash.Essentials.Core.IRoutingOutputs`
* `PepperDash.Essentials.Core.IRoutingInputs`
* `PepperDash.Essentials.Core.IRoutingMidpoint`
* `PepperDash.Essentials.Core.IRoutingMidpointWithFeedback`
* `PepperDash.Essentials.Core.IRoutingSinkWithFeedback`

> **Note:** As of the v3 routing refactor, the older `IRoutingInputsOutputs`, `IRouting`,
> `IRoutingWithFeedback`, `IRoutingWithClear`, `IRoutingSink`, `IRoutingSinkWithInputPort`,
> `IRoutingSinkNoSwitching`, `IRoutingSinkWithSwitching`, and `IRoutingSinkWithSwitchingWithInputPort`
> interfaces have been removed and consolidated - see [Routing interfaces (v3)](#routing-interfaces-v3)
> below for the current mapping.

## Example system, a simple presentation system

The diagram below shows the connections in a simple presentation system, with a few variations in connection paths. Example routes will be described following the diagram.

Each visible line between ports on devices represents a tie line. A tie line connects an output port on one device to an input port on another device, for example: an HDMI port on a document camera to an HDMI input on a matrix switcher. A tie line may be audio, video or both. It is essentially a logical representation of a physical cable in a system. This diagram has 12 tie lines, and those tie lines are defined in the tieLines array in configuration.

![Routing system diagram](~/docs/images/routing-system-diagram.png)

Let’s go through some examples of routing, using pseudo-code:

1. Method call: “Projector 1, show Doc cam.” Routing will walk backwards through DM-RMC-3 and DM-8x8 iterating through all “wired up” ports until it finds a path back to the Doc cam. Routing will then step back through all devices in the discovered chain, switching routes on those that are switchable: Doc cam: no switching; DM 8x8: route input 3 to output 3; DM-RMC-3: no switching; Projector 1: Select input HDMI In. Route is complete.
2. Method call: “Projector 2, show Laptop, video-only.” Routing will walk backwards through DM-RMC-4, DM 8x8, DM-TX-1, iterating through all connected ports until it finds a connection to the laptop. Routing then steps back through all devices, switching video where it can: Laptop: No switching; DM-TX-1: Select HDMI in; DM 8x8: Route input 5 to output 4; DM-RMC-4: No switching; Projector 2: Select HDMI input. Route is complete.
3. Method call: “Amplifier, connect Laptop audio.” Again walking backwards to Laptop, as in #2 above. Switching will take place on DM-TX-1, DM 8x8, audio-only.
4. Very simple call: “Lobby display, show signage controller.” Routing will walk back on HDMI input 1 and immediately find the signage controller. It then does a switch to HDMI 1 on the display.

All four of the above could be logically combined in a series of calls to define a possible “scene” in a room: Put Document camera on Projector 1, put Laptop on Projector 2 and the audio, put Signage on the Lobby display. They key takeaway is that the developer doesn’t need to define what is involved in making a certain route. The person configuring the system defines how it’s wired up, and the code only needs to tell a given destination to get a source, likely through configuration as well.

All of the above routes can be defined in source list routing tables, covered elsewhere (**make link)**.

---

### Definitions

#### Ports

Ports are logical representations of the input and output ports on a device.

#### Source

A source is a device at the beginning of a signal chain. For example, a set-top box, or a camera. Source devices typically have only output ports.

Source devices in Essentials must implement `IRoutingOutputs` or `IRoutingSource`

#### Midpoint

A midpoint is a device in the middle of the signal chain. Typically a switcher, matrix or otherwise. Examples: DM chassis; DM-TX; DM-RMC; A video codec. These devices will have input and output ports.

Passthrough midpoints (no active switching) must implement `IRoutingMidpoint`. Midpoints that actively
switch and report their current routes must implement `IRoutingMidpointWithFeedback`.

#### Sink

A sink is a device at the end of a full signal path. For example, a display, amplifier, encoder, etc. Sinks typically contain only input ports. They may or may not have switching, like a display with several inputs. Classes defining sink devices must implement `IRoutingSinkWithFeedback`.

#### Tie-line

A tie-line is a logical representation of a physical cable connection between two devices. It has five properties that define how the tie-line connects two devices. A configuration snippet for a single tie line connecting HDMI output 1 on a Cisco RoomKit to HDMI input 1 on a display, carrying both audio and video, is shown below.

```json
{
  "sourceKey": "ciscoSparkPlusCodec-1",
  "sourcePort": "HdmiOut1",
  "destinationKey": "display-1",
  "destinationPort": "HdmiIn1",
  "type": "audioVideo"
}
```

### Interfaces

#### Routing interfaces (v3)

The v3 routing refactor consolidated several older, overlapping interfaces into two feedback-aware
interfaces. If you're updating a device implemented against the pre-v3 API, use this table to find
its replacement:

| Old interface (pre-v3, removed) | Current interface |
| --- | --- |
| `IRoutingInputsOutputs` | `IRoutingMidpoint` |
| `IRouting`, `IRoutingWithFeedback`, `IRoutingWithClear` | `IRoutingMidpointWithFeedback` |
| `IRoutingSink`, `IRoutingSinkWithInputPort`, `IRoutingSinkWithSwitching`, `IRoutingSinkWithSwitchingWithInputPort` | `IRoutingSinkWithFeedback` |

**Base building-block interfaces** (unchanged):

* `IRoutingSource` - marker interface for a device that originates a signal path; extends `IRoutingOutputs`.
* `IRoutingOutputs` - exposes `RoutingPortCollection<RoutingOutputPort> OutputPorts`.
* `IRoutingInputs` - exposes `RoutingPortCollection<RoutingInputPort> InputPorts`.

**Midpoint interfaces:**

* `IRoutingMidpoint` - a passthrough device with both input and output ports but no active switching. Extends `IRoutingInputs` and `IRoutingOutputs`.
* `IRoutingMidpointWithFeedback` - a midpoint that actively switches and reports feedback. Extends `IRoutingMidpoint` and adds:
  * `void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)`
  * `void ClearRoute(object outputSelector, eRoutingSignalType signalType)`
  * `List<RouteSwitchDescriptor> CurrentRoutes { get; }`
  * `event RouteChangedEventHandler RouteChanged`

**Sink interface:**

* `IRoutingSinkWithFeedback` - a routing endpoint device that can switch inputs and provides feedback. Extends `IRoutingInputs`, `IKeyName`, and `ICurrentSources` (see below), and adds:
  * `void ExecuteSwitch(object inputSelector)`
  * `RoutingInputPort CurrentInputPort { get; }`
  * `event InputChangedEventHandler InputChanged`

#### Current source tracking (`ICurrentSources`)

`ICurrentSources` is a new interface (implemented by `IRoutingSinkWithFeedback`) that reports which
source is actually feeding a sink right now, per signal type:

* `Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; }`
* `Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; }`
* `event EventHandler<CurrentSourcesChangedEventArgs> CurrentSourcesChanged`

A new `RoutingFeedbackManager` subscribes to `RouteChanged` on every `IRoutingMidpointWithFeedback`
and `InputChanged` on every `IRoutingSinkWithFeedback` in the system, traces each change back through
the tie-line graph to the originating source (debounced ~500ms), and keeps each sink's
`ICurrentSources` state up to date automatically - developers no longer need to manually wire up
source tracking per device.

#### Multiview layout support

For decoders that can display several independently-routable tiles at once (e.g. a WyreStorm
NetworkHD-style multiview decoder), two additional interfaces are available:

* `IRoutingSinkWithLayouts` - extends `IRoutingSource` and exposes `Dictionary<int, IRoutingSinkWithFeedback> WindowTileSinks`, one per-tile "virtual sink" that can be routed independently like any other `IRoutingSinkWithFeedback`.
* `IRoutingSinkWithLayoutState` - extends `IRoutingSinkWithLayouts` and additionally reports the current canvas/tile geometry via `MultiviewLayoutState CurrentLayout { get; }` and `event EventHandler<MultiviewLayoutStateEventArgs> LayoutChanged`.

`MultiviewLayoutState` is a JSON-serializable, product-agnostic model of the canvas: `CanvasWidth`,
`CanvasHeight`, and a list of `MultiviewTileState` entries (`TileNumber`, `TileSinkKey`, `X`, `Y`,
`Width`, `Height`, `ZOrder`, `SourceDeviceKey`) - suitable for driving a routing diagram UI.

#### Real-time routing feedback over WebSocket

Essentials Core includes a built-in WebSocket service, `RoutingFeedbackWebsocket`
(`PepperDash.Essentials.Core.Web`), that broadcasts routing state changes to any connected client in
real time - this is what powers the live routing diagram in the
[Essentials Web Config App](https://github.com/PepperDash/essentials-web-config-app) and
[essentials-devtools](https://github.com/PepperDash/essentials-devtools).

A client starts (or reuses) a session by sending an HTTP `GET` to the `routingFeedbackSession` API
route (e.g. `https://[processor-ip]/cws/[appId]/api/routingFeedbackSession`), which starts the
WebSocket server on a random high port (if not already running), forwards that port on the CS LAN
adapter if present, and returns a JSON body with the connection URL(s):

```json
{ "url": "wss://[processor-ip]:[port]/routing/join/", "fallbackUrl": "wss://..." }
```

Once connected, the client receives a `snapshot` message describing every midpoint route, sink
input, and multiview layout currently active, followed by incremental update messages (each
debounced ~200ms) as things change:

| Message `type` | Sent when | Payload |
| --- | --- | --- |
| `snapshot` | On connect | `midpointRoutes`, `sinkRoutes`, `layouts` for the whole system |
| `midpointRouteChanged` | An `IRoutingMidpointWithFeedback`'s `RouteChanged` fires | `deviceKey`, `routes: [{ inputPortKey, outputPortKey, signalType }]` |
| `sinkInputChanged` | An `IRoutingSinkWithFeedback`'s `InputChanged` fires | `deviceKey`, `inputPortKey`, `sourceDeviceKey`, `signalType` |
| `layoutChanged` | An `IRoutingSinkWithLayoutState`'s `LayoutChanged` fires | `deviceKey`, `layout` (a `MultiviewLayoutState`) |

Per-tile sinks belonging to an `IRoutingSinkWithLayouts` device are reported to clients as qualified
input ports on their parent device's node, rather than as separate graph nodes, so a routing diagram
can render a multiview decoder as a single device with per-tile inputs.

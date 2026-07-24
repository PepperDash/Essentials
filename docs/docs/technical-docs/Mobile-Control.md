# Mobile Control: architecture and client communication

## TL;DR

Mobile Control is a WebSocket-based messaging layer built into Essentials. Devices expose small
"messenger" classes that register JSON message paths (e.g. `/device/{key}/startMeeting`) with a
central `MobileControlSystemController`. Clients (typically a React app built on
[`@pepperdash/mobile-control-react-app-core`](https://github.com/PepperDash/mobile-control-react-app-core))
connect over WebSocket with a per-room token, send the same JSON envelope to invoke device methods,
and receive unsolicited state-update messages that a Redux store uses to keep device/room state in
sync in real time.

## Server-side architecture

### Projects involved

* `PepperDash.Essentials.MobileControl` - the `MobileControlSystemController` device, its
  WebSocket server (`WebSocketServer/MobileControlWebsocketServer.cs`), room bridges, and
  touchpanel controller.
* `PepperDash.Essentials.MobileControl.Messengers` (namespace `PepperDash.Essentials.AppServer.Messengers`) -
  the `MessengerBase` class and the library of built-in messengers for common device interfaces
  (`IHasStartMeetingMessenger`, `IHasMeetingInfoMessenger`, `ICommunicationMonitorMessenger`,
  `IRoutingMidpointWithFeedbackMessenger`, etc.), plus the message envelope types.
* `PepperDash.Essentials.Core.DeviceTypeInterfaces` - the public contracts:
  `IMobileControl`, `IMobileControlMessenger`, `IMobileControlMessengerWithSubscriptions`,
  `IMobileControlRoomMessenger`, `IMobileControlAction`, `IMobileControlMessage`, and the
  touchpanel-controller interfaces.

### Direct Server vs. Edge Server

Mobile Control supports two (non-exclusive) transport modes, configured on the `MobileControlConfig`
device:

* **Direct Server** - `MobileControlWebsocketServer` runs an HTTPS WebSocket server directly on the
  processor hardware. Clients on the same network connect straight to the processor - lowest
  latency, no external dependency. This is what `mobile-control-react-app-core`'s local dev flow
  and the deployed `mcUserApp` React app use.
* **Edge Server** (API server / cloud gateway) - the processor instead makes an *outbound* WebSocket
  connection to an external Mobile Control server, which relays messages between it and remote
  clients. Useful when clients can't reach the processor directly (e.g. no local network access).

Both modes can be enabled simultaneously; outgoing messages are queued and sent down whichever
transport(s) are active.

### The WebSocket endpoint (Direct Server)

`MobileControlWebsocketServer` (`WebSocketServer/MobileControlWebsocketServer.cs`) hosts the
Direct Server:

* **Path**: `/mc/api/ui/join/` (`_wsPath`)
* **Port**: `50000 + <program slot number>` by default (e.g. program slot 2 → port `50002`), or a
  custom port from config
* **User app**: static files are served from `/user/programX/mcUserApp` (`_appPath`) at the base
  href `/mc/app` (`_userAppBaseHref`) - this is exactly the `mcUserApp` deployment directory used
  when [deploying](../Get-started.md) a built React app.

### Connecting and authentication (tokens)

A client doesn't connect straight to the WebSocket - it first needs a **token** identifying which
room/UI-client slot it's joining:

1. **Get a token** - on the processor console, run:
   ```
   mobileinfo:[programSlot]
   ```
   This prints the Direct Server port, any existing UI-client tokens, and full connect URLs (e.g.
   `http://[ip]:[port]/mc/app?token=[token]`). New tokens can also be added dynamically
   (`mobileadduiclient:[programSlot] [roomKey]`) - see the console command's help text for details.
2. **Join a room** - the client calls `GET {apiPath}/ui/joinroom?token={token}` over HTTPS, which
   validates the token and returns room data including a generated `clientId`.
3. **Open the WebSocket** - the client connects to
   `wss://[processor-ip]:[port]/mc/api/ui/join/{token}?clientId={clientId}`.

Each connected client is tracked as a `UiClient`; disconnecting removes its subscriptions from
every messenger it was subscribed to.

### The message envelope

Every message sent in either direction is the same simple JSON envelope
(`MobileControlMessage`, namespace `PepperDash.Essentials.AppServer.Messengers`):

```json
{
  "type": "/device/zoomRoom1/startMeeting",
  "clientId": "abc-123",
  "content": { "value": 30 }
}
```

* `type` - a path identifying the target device/room and action (client → server), or the
  device/room whose state is being reported (server → client).
* `clientId` - which client sent the message, or (server → client) which single client a targeted
  reply is meant for; omitted/ignored for broadcasts to all subscribers.
* `content` - a `JToken` payload. For simple values, `MobileControlSimpleContent<T>` wraps a single
  `value` property (as in the example above); for state updates, `content` is the device's full
  serialized state object.

### The messenger pattern

Each device that wants to be controllable/observable over Mobile Control gets a small **messenger**
class deriving from `MessengerBase` (`PepperDash.Essentials.AppServer.Messengers`):

```csharp
public class IHasStartMeetingMessenger : MessengerBase
{
    // ...
    protected override void RegisterActions()
    {
        AddAction("/fullStatus", (id, content) => SendFullStatus(id));

        AddAction("/startMeeting", (id, content) =>
        {
            var msg = content.ToObject<MobileControlSimpleContent<uint>>();
            _startMeeting.StartMeeting(msg?.Value ?? _startMeeting.DefaultMeetingDurationMin);
        });

        AddAction("/leaveMeeting", (id, content) => _startMeeting.LeaveMeeting());
    }
}
```

* The messenger is constructed with a **base `MessagePath`** (e.g. `/device/zoomRoom1`) and calls
  `AddAction(subPath, handler)` for each supported action, relative to that base path.
* `RegisterWithAppServer(IMobileControl appServerController)` registers the messenger's
  `HandleMessage` callback with the system controller via `IMobileControl.AddAction<T>(...)`.
* When an incoming message's `type` starts with the messenger's `MessagePath`, `HandleMessage`
  strips that prefix (leaving e.g. `/startMeeting`) and dispatches to the matching registered
  action. The sending client is automatically added to that messenger's subscriber list, so it
  receives future unsolicited feedback from it.
* To push state out, messengers call `PostStatusMessage(DeviceStateMessageBase message, clientId)` -
  omitting `clientId` broadcasts to every subscribed client; passing one targets a single client
  (e.g. replying to a `/fullStatus` request).

### How devices opt in

`EssentialsDevice.CustomActivate()` calls a virtual `CreateMobileControlMessengers()` hook once all
devices have activated. A device (or a room) overrides this method to look up the `IMobileControl`
device and construct/register whichever messengers it needs:

```csharp
protected override void CreateMobileControlMessengers()
{
    var mc = DeviceManager.AllDevices.OfType<IMobileControl>().FirstOrDefault();
    if (mc == null) return;

    var messenger = new IHasStartMeetingMessenger("zoomRoom1-startMeeting", "/device/zoomRoom1", this);
    messenger.RegisterWithAppServer(mc);
}
```

Rooms typically implement `IMobileControlRoomMessenger` and register a room-level messenger at a
`/room/{roomKey}` path (via a `MobileControlBridgeBase`/`MobileControlEssentialsRoomBridge`), in
addition to any per-device messengers.

## Client-side architecture (React)

`@pepperdash/mobile-control-react-app-core` provides the client building blocks used by apps like
[beincourt-pv2-react-app](https://github.com/pepperdash-beincourt/beincourt-pv2-react-app):

### Connection lifecycle

The real work happens in a Redux middleware (`src/lib/store/middleware/websocketMiddleware.ts`);
`WebsocketProvider`/`WebsocketContext` are a thin, backward-compatible React Context wrapper around
it that simply dispatches Redux actions (`wsConnect`, `wsSendMessage`, `wsReconnect`, ...).

1. On mount, `wsConnect()` is dispatched.
2. The middleware reads `apiPath` from the app's local config (`_config.local.json`/
   `_config.default.json`) and the connection `token` (from the URL's `?token=` query param).
3. It calls `GET {apiPath}/ui/joinroom?token={token}` to validate the token and get room data
   (including a `clientId`).
4. It opens a WebSocket at `{apiPath with ws(s) scheme}/ui/join/{token}?clientId={clientId}`.
5. On specific close codes (e.g. `4000` user code changed, `4002` room combination changed) it stops
   auto-reconnecting and surfaces an error; otherwise it automatically retries.

### Sending messages

```ts
const { sendMessage } = useWebsocketContext();
sendMessage('/device/zoomRoom1/startMeeting', { value: 30 });
```

`sendMessage` serializes `{ type, clientId, content }` (the same envelope described above) and
sends it over the open WebSocket.

### Receiving messages / state hydration

Incoming messages are routed by their `type` prefix:

| Prefix | Handling |
| --- | --- |
| `/system/*` | Internal system messages (user code, touchpanel key, room-combination/device-interface changes, initial sync complete, ...) |
| `/event/*` | Dispatched to any handlers registered via `addEventHandler(eventType, key, callback)` |
| `/room/*` | `dispatch(roomsActions.setRoomState(message))` |
| `/device/*` | `dispatch(devicesActions.setDeviceState(message))` |

Device/room state lives in Redux slices keyed by device/room key, so any component can read the
latest known state for a given key.

### The hook pattern

Each supported device interface has a small hook that combines reading state with calling actions,
e.g. `useIHasStartMeeting`:

```ts
export function useIHasStartMeeting(key: string): IHasStartMeetingReturn | undefined {
  const { sendMessage } = useWebsocketContext();
  const state = useGetDevice<IHasStartMeetingState>(key); // reads devices[key] from Redux

  return useMemo(() => {
    if (!state) return undefined;
    const path = `/device/${key}`;
    return {
      state,
      startMeeting: (durationMin?: number) => sendMessage(`${path}/startMeeting`, { value: durationMin }),
      leaveMeeting: () => sendMessage(`${path}/leaveMeeting`, null),
    };
  }, [key, sendMessage, state]);
}
```

Components use these hooks to read live device state and call device actions without needing to
know anything about the WebSocket transport underneath.

## Full round-trip example: starting a meeting

1. **UI**: user clicks "Start Meeting"; component calls `startMeeting(30)` from `useIHasStartMeeting('zoomRoom1')`.
2. **Client → server**: `sendMessage('/device/zoomRoom1/startMeeting', { value: 30 })` sends
   `{"type":"/device/zoomRoom1/startMeeting","clientId":"abc-123","content":{"value":30}}` over the WebSocket.
3. **Server routing**: the system controller matches `/device/zoomRoom1` against the registered
   `IHasStartMeetingMessenger`'s `MessagePath`, strips the prefix to get `/startMeeting`, and invokes
   its registered action, which calls `StartMeeting(30)` on the underlying device.
4. **Device fires feedback**: the device's meeting-started feedback fires.
5. **Server → clients**: the messenger calls `PostStatusMessage(...)`, broadcasting
   `{"type":"/device/zoomRoom1","clientId":null,"content":{ ...state }}` to every client subscribed
   to that messenger.
6. **Client receives**: the message's `type` starts with `/device/`, so
   `dispatch(devicesActions.setDeviceState(message))` merges the new state into Redux under
   `devices['zoomRoom1']`.
7. **UI updates**: `useIHasStartMeeting('zoomRoom1')`'s `state` reflects the new meeting info on the
   next render.

# Unifying communication methods

In networked A/V systems, devices can use many different methods of communication: COM ports, TCP/IP sockets, Telnet, SSH. Generally, the data protocol and commands that are sent and received using any of these methods are the same, and it is not necessary for a device to know the details of the communication method it is using. A Samsung MDC protocol display in room 1 using RS232 speaks the same language as another Samsung MDC does in the next room using TCP/IP. For these, and most cases where the device doesn't need to know its communication method, we introduce the `IBasicCommunication` interface.
## Classes Referenced

* `PepperDash.Core.IBasicCommunication`
* `PepperDash.Core.ISocketStatus`
* `PepperDash.Core.GenericTcpIpClient`
* `PepperDash.Core.GenericSshClient`
* `PepperDash.Core.GenericSecureTcpIpClient`
* `PepperDash.Essentials.Core.ComPortController`
* `PepperDash.Essentials.Core.StatusMonitorBase`
## IBasicCommunication and ISocketStatus

All common communication controllers will implement the `IBasicCommunication` interface, which is an extension of `ICommunicationReceiver`. This defines receive events, connection state properties, and send methods. Devices that need to use COM port, TCP, SSh or other similar communication will require an `IBasicCommunication` type object to be injected at construction time.

```cs
/// <summary>
/// An incoming communication stream
/// </summary>
public interface ICommunicationReceiver : IKeyed
{
    event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
    event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

    bool IsConnected { get; }
    void Connect();
    void Disconnect();
}

/// <summary>
/// Represents a device that uses basic connection
/// </summary>
public interface IBasicCommunication : ICommunicationReceiver
{
    void SendText(string text);
    void SendBytes(byte[] bytes);
}

/// <summary>
/// For IBasicCommunication classes that have SocketStatus. GenericSshClient,
/// GenericTcpIpClient
/// </summary>
public interface ISocketStatus : IBasicCommunication
{
    event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;
    SocketStatus ClientStatus { get; }
}
```

### Developing devices with communication

Essentials uses dependency injection concepts in its start up phase. Simply, most devices use the same methods of communication, and are often communication-agnostic. During the build-from-configuration phase, the communication method device is instantiated, and then injected into the device that will use it. Since the communication device is of `IBasicCommunication`, the device controller receiving it knows that it can do things like listen for events, send text, or be notified when sockets change.

### Device Factory, Codec example

![Communication Device factory](~/docs/images/comm-device-factory.png)

The DeviceManager will contain two new devices after this: The Cisco codec, and the codec's `GenericSshClient`. This enables easier debugging of the client using console methods. Some devices like this codec will also have a `StatusMonitorBase` device, for Fusion and other reporting.

> `ComPortController` is `IBasicCommunication` as well, but methods like `Connect()` and `Disconnect()` do nothing on these types.

#### ISocketStatus

`PepperDash.Core.GenericTcpIpClient`, `GenericSshClient` and some other socket controllers implement `ISocketStatus`, which is an extension of `IBasicCommunication`. This interface reveals connection status properties and events.

```cs
public interface ISocketStatus : IBasicCommunication
{
    event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;
    SocketStatus ClientStatus { get; }
}
```

Classes that are using socket-based comms will need to check if the communication is `ISocketStatus` and link up to the `ConnectionChange` event for connection handling.

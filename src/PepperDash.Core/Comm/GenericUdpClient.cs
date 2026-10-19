using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using PepperDash.Core.Logging;
using ThreadingTimeout = System.Threading.Timeout;
using NetSocketException = System.Net.Sockets.SocketException;

namespace PepperDash.Core;

/// <summary>
/// A class to handle basic UDP communications to a remote endpoint. Unlike <see cref="GenericUdpServer"/>,
/// which binds a local listener, this opens an outbound socket associated with one remote address and port.
/// </summary>
public class GenericUdpClient : Device, ISocketStatusWithStreamDebugging, IAutoReconnect
{
    private const string SplusKey = "Uninitialized UdpClient";

    private readonly object stateLock = new object();
    private readonly Timer reconnectTimer;

    private UdpClient client;
    private CancellationTokenSource receiveCancellationTokenSource;
    private bool connectEnabled;
    private bool connectionRefusedLogged;
    private SocketStatus clientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;

    /// <summary>
    /// Object to enable stream debugging
    /// </summary>
    public CommunicationStreamDebugging StreamDebugging { get; private set; }

    /// <summary>
    /// Fires when data is received from the remote endpoint and returns it as a byte array
    /// </summary>
    public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

    /// <summary>
    /// Fires when data is received from the remote endpoint and returns it as text
    /// </summary>
    public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

    /// <summary>
    /// Fires when the socket status changes
    /// </summary>
    public event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;

    /// <summary>
    /// Address of remote endpoint
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    /// Port on remote endpoint
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Another S+ helper because large port numbers can be treated as signed ints
    /// </summary>
    public ushort UPort
    {
        get { return Convert.ToUInt16(Port); }
        set { Port = Convert.ToInt32(value); }
    }

    /// <summary>
    /// Defaults to 2000
    /// </summary>
    public int BufferSize { get; set; }

    /// <summary>
    /// True when the local socket is created and associated with the configured remote endpoint
    /// </summary>
    public bool IsConnected
    {
        get { return ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED; }
    }

    /// <summary>
    /// S+ helper for IsConnected
    /// </summary>
    public ushort UIsConnected
    {
        get { return (ushort)(IsConnected ? 1 : 0); }
    }

    /// <summary>
    /// The current socket status of the client
    /// </summary>
    public SocketStatus ClientStatus
    {
        get
        {
            lock (stateLock)
            {
                return clientStatus;
            }
        }
        private set
        {
            var shouldFireEvent = false;

            lock (stateLock)
            {
                if (clientStatus != value)
                {
                    clientStatus = value;
                    shouldFireEvent = true;
                }
            }

            if (shouldFireEvent)
                ConnectionChange?.Invoke(this, new GenericSocketStatusChageEventArgs(this));
        }
    }

    /// <summary>
    /// Ushort representation of client status
    /// </summary>
    public ushort UStatus
    {
        get { return (ushort)ClientStatus; }
    }

    /// <summary>
    /// Gets or sets the AutoReconnect
    /// </summary>
    public bool AutoReconnect { get; set; }

    /// <summary>
    /// S+ helper for AutoReconnect
    /// </summary>
    public ushort UAutoReconnect
    {
        get { return (ushort)(AutoReconnect ? 1 : 0); }
        set { AutoReconnect = value == 1; }
    }

    /// <summary>
    /// Milliseconds to wait before attempting to reconnect. Defaults to 5000
    /// </summary>
    public int AutoReconnectIntervalMs { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public GenericUdpClient(string key, string address, int port, int bufferSize)
        : base(key)
    {
        StreamDebugging = new CommunicationStreamDebugging(key);
        CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironment_ProgramStatusEventHandler;
        AutoReconnectIntervalMs = 5000;
        Hostname = address;
        Port = port;
        BufferSize = bufferSize;

        // ConnectCore checks connectEnabled under the lock, so a callback already running when
        // Disconnect() fires cannot re-enable retries or reopen the socket.
        reconnectTimer = new Timer(o => ConnectCore(), null, ThreadingTimeout.Infinite, ThreadingTimeout.Infinite);
    }

    /// <summary>
    /// Constructor for S+
    /// </summary>
    public GenericUdpClient()
        : base(SplusKey)
    {
        StreamDebugging = new CommunicationStreamDebugging(SplusKey);
        CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironment_ProgramStatusEventHandler;
        AutoReconnectIntervalMs = 5000;
        BufferSize = 2000;

        // ConnectCore checks connectEnabled under the lock, so a callback already running when
        // Disconnect() fires cannot re-enable retries or reopen the socket.
        reconnectTimer = new Timer(o => ConnectCore(), null, ThreadingTimeout.Infinite, ThreadingTimeout.Infinite);
    }

    /// <summary>
    /// Initialize method
    /// </summary>
    public void Initialize(string key)
    {
        Key = key;
    }

    private void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
    {
        if (programEventType == eProgramStatusEventType.Stopping)
        {
            this.LogDebug("Program stopping. Closing connection");
            Deactivate();
        }
    }

    /// <summary>
    /// Deactivate method
    /// </summary>
    public override bool Deactivate()
    {
        Disconnect();
        return true;
    }

    /// <summary>
    /// Connect method
    /// </summary>
    public void Connect()
    {
        lock (stateLock)
        {
            connectEnabled = true;
        }

        ConnectCore();
    }

    /// <summary>
    /// Opens the socket if connecting is enabled and no client exists. Shared by <see cref="Connect"/>
    /// and the reconnect timer; only the former turns connecting on, so a reconnect that lands after
    /// <see cref="Disconnect"/> is a no-op instead of reopening the socket.
    /// </summary>
    private void ConnectCore()
    {
        if (string.IsNullOrEmpty(Hostname))
        {
            this.LogWarning("No address set");
            ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
            return;
        }

        if (Port < 1 || Port > 65535)
        {
            this.LogWarning("Invalid port {port}", Port);
            ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
            return;
        }

        var hostname = Hostname;
        var port = Port;
        var bufferSize = BufferSize;
        UdpClient newClient = null;
        CancellationTokenSource newReceiveCancellationTokenSource = null;
        CancellationToken startReceiveToken = default(CancellationToken);
        var adopted = false;

        lock (stateLock)
        {
            if (!connectEnabled || client != null)
                return;
        }

        try
        {
            newReceiveCancellationTokenSource = new CancellationTokenSource();
            newClient = new UdpClient();
            newClient.Client.ReceiveBufferSize = bufferSize;
            newClient.Client.SendBufferSize = bufferSize;
            newClient.Connect(hostname, port);

            lock (stateLock)
            {
                if (!connectEnabled || client != null)
                {
                    newClient.Close();
                    newReceiveCancellationTokenSource.Cancel();
                    newReceiveCancellationTokenSource.Dispose();
                    return;
                }

                receiveCancellationTokenSource = newReceiveCancellationTokenSource;
                client = newClient;
                adopted = true;

                // Capture the token and stop the timer before publishing the status: a ConnectionChange
                // handler may call Disconnect(), which cancels and disposes the token source.
                startReceiveToken = newReceiveCancellationTokenSource.Token;
                reconnectTimer.Change(ThreadingTimeout.Infinite, ThreadingTimeout.Infinite);

                ClientStatus = SocketStatus.SOCKET_STATUS_CONNECTED;
            }

            // If a handler disconnected us, the token is already cancelled and there is nothing to receive on.
            if (!startReceiveToken.IsCancellationRequested)
                StartReceive(startReceiveToken);
        }
        catch (Exception ex)
        {
            this.LogException(ex, "Error connecting UDP client to {host}:{port}", hostname, port);

            // Once adopted, the client and token source belong to the instance and are cleaned up
            // by Disconnect/HandleDisconnected, not here.
            if (!adopted)
            {
                if (newClient != null)
                    newClient.Close();

                if (newReceiveCancellationTokenSource != null)
                {
                    newReceiveCancellationTokenSource.Cancel();
                    newReceiveCancellationTokenSource.Dispose();
                }
            }

            lock (stateLock)
            {
                if (connectEnabled && client == null)
                {
                    ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
                    StartReconnectTimer();
                }
            }
        }
    }

    /// <summary>
    /// Disconnect method
    /// </summary>
    public void Disconnect()
    {
        lock (stateLock)
        {
            connectEnabled = false;
            reconnectTimer.Change(ThreadingTimeout.Infinite, ThreadingTimeout.Infinite);
            CleanupClient();
            ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
        }
    }

    /// <summary>
    /// SendText method
    /// </summary>
    public void SendText(string text)
    {
        this.PrintSentText(text);

        var bytes = Encoding.GetEncoding(28591).GetBytes(text);
        SendBytes(bytes);
    }

    /// <summary>
    /// SendBytes method
    /// </summary>
    public void SendBytes(byte[] bytes)
    {
        if (bytes == null)
            return;

        UdpClient udpClient = null;

        try
        {
            this.PrintSentBytes(bytes);

            if (!IsConnected || client == null)
                Connect();

            udpClient = client;
            if (!IsConnected || udpClient == null)
            {
                this.LogWarning("Cannot send bytes because the client is not connected");
                return;
            }

            udpClient.Send(bytes, bytes.Length);
        }
        catch (Exception ex)
        {
            this.LogException(ex, "Error sending UDP bytes");
            HandleDisconnected(udpClient);
        }
    }

    private void StartReceive(CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                var udpClient = client;
                if (udpClient == null)
                    return;

                try
                {

                    var result = await udpClient.ReceiveAsync().ConfigureAwait(false);
                    var bytes = result.Buffer;
                    if (bytes == null || bytes.Length == 0)
                        continue;

                    connectionRefusedLogged = false;

                    var text = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);

                    this.PrintReceivedBytes(bytes);
                    this.PrintReceivedText(text);

                    BytesReceived?.Invoke(this, new GenericCommMethodReceiveBytesArgs(bytes));
                    TextReceived?.Invoke(this, new GenericCommMethodReceiveTextArgs(text));
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (InvalidOperationException)
                {
                    return;
                }
                catch (NetSocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        // ICMP port-unreachable from the remote surfaces here. Logged once per outage so a
                        // device that is off does not fill the log every receive cycle.
                        if (!connectionRefusedLogged)
                        {
                            this.LogWarning("Remote endpoint refused UDP traffic or is no longer listening");
                            connectionRefusedLogged = true;
                        }

                        HandleDisconnected(udpClient);
                        return;
                    }

                    this.LogException(ex, "UDP receive error");

                    if (AutoReconnect)
                    {
                        HandleDisconnected(udpClient);
                        return;
                    }

                    continue;
                }
                catch (Exception ex)
                {
                    this.LogException(ex, "Unexpected UDP receive error");

                    if (AutoReconnect)
                    {
                        HandleDisconnected(udpClient);
                        return;
                    }

                    continue;
                }
            }
        }, token);
    }

    /// <summary>
    /// Tears down the connection after an I/O failure on <paramref name="failedClient"/>.
    /// A failure reported for a socket that has already been replaced is ignored, so a stale
    /// receive loop or send cannot close the healthy replacement.
    /// </summary>
    private void HandleDisconnected(UdpClient failedClient)
    {
        lock (stateLock)
        {
            if (failedClient == null || !ReferenceEquals(client, failedClient))
                return;

            CleanupClient();
            ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
            StartReconnectTimer();
        }
    }

    private void StartReconnectTimer()
    {
        if (AutoReconnect && connectEnabled)
            reconnectTimer.Change(AutoReconnectIntervalMs, ThreadingTimeout.Infinite);
    }

    private void CleanupClient()
    {
        if (receiveCancellationTokenSource != null)
        {
            receiveCancellationTokenSource.Cancel();
            receiveCancellationTokenSource.Dispose();
            receiveCancellationTokenSource = null;
        }

        if (client != null)
        {
            client.Close();
            client = null;
        }
    }
}

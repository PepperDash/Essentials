using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using ThreadingTimeout = System.Threading.Timeout;
using NetSocketException = System.Net.Sockets.SocketException;

namespace PepperDash.Core
{
    /// <summary>
    /// A class to handle basic UDP communications to a remote endpoint
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

            reconnectTimer = new Timer(o =>
            {
                if (connectEnabled)
                    Connect();
            }, null, ThreadingTimeout.Infinite, ThreadingTimeout.Infinite);
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

            reconnectTimer = new Timer(o =>
            {
                if (connectEnabled)
                    Connect();
            }, null, ThreadingTimeout.Infinite, ThreadingTimeout.Infinite);
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
                Debug.Console(1, this, "Program stopping. Closing connection");
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
            if (string.IsNullOrEmpty(Hostname))
            {
                Debug.Console(1, Debug.ErrorLogLevel.Warning, "GenericUdpClient '{0}': No address set", Key);
                ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
                return;
            }

            if (Port < 1 || Port > 65535)
            {
                Debug.Console(1, Debug.ErrorLogLevel.Warning, "GenericUdpClient '{0}': Invalid port", Key);
                ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
                return;
            }

            lock (stateLock)
            {
                connectEnabled = true;

                if (client != null)
                    return;

                try
                {
                    receiveCancellationTokenSource = new CancellationTokenSource();
                    client = new UdpClient();
                    client.Client.ReceiveBufferSize = BufferSize;
                    client.Client.SendBufferSize = BufferSize;
                    client.Connect(Hostname, Port);
                    ClientStatus = SocketStatus.SOCKET_STATUS_CONNECTED;
                    reconnectTimer.Change(ThreadingTimeout.Infinite, ThreadingTimeout.Infinite);
                    StartReceive(receiveCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Error connecting UDP client {0}", this, Key);
                    CleanupClient();
                    ClientStatus = SocketStatus.SOCKET_STATUS_NO_CONNECT;
                    StartReconnectTimer();
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

            try
            {
                this.PrintSentBytes(bytes);

                if (!IsConnected || client == null)
                    Connect();

                var udpClient = client;
                if (!IsConnected || udpClient == null)
                    return;

                udpClient.Send(bytes, bytes.Length);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error sending UDP bytes for {0}", this, Key);
                HandleDisconnected();
            }
        }

        private void StartReceive(CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var udpClient = client;
                        if (udpClient == null)
                            return;

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
                            if (!connectionRefusedLogged)
                            {
                                Debug.Console(1, Debug.ErrorLogLevel.Warning,
                                    "GenericUdpClient '{0}': Remote endpoint refused UDP traffic or is no longer listening",
                                    Key);
                                connectionRefusedLogged = true;
                            }

                            HandleDisconnected();
                            return;
                        }

                        Debug.LogMessage(ex, "UDP receive error for {0}", this, Key);
                        HandleDisconnected();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogMessage(ex, "Unexpected UDP receive error for {0}", this, Key);
                        HandleDisconnected();
                        return;
                    }
                }
            }, token);
        }

        private void HandleDisconnected()
        {
            lock (stateLock)
            {
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
}
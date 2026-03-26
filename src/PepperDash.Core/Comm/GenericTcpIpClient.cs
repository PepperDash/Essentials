extern alias NewtonsoftJson;

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Timer = System.Timers.Timer;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using JsonProperty = NewtonsoftJson::Newtonsoft.Json.JsonPropertyAttribute;
using Required = NewtonsoftJson::Newtonsoft.Json.Required;
using PepperDash.Core.Logging;
using System.Threading.Tasks;

namespace PepperDash.Core;

/// <summary>
/// A class to handle basic TCP/IP communications with a server
/// </summary>
public class GenericTcpIpClient : Device, ISocketStatusWithStreamDebugging, IAutoReconnect
{
    private const string SplusKey = "Uninitialized TcpIpClient";
    /// <summary>
    /// Object to enable stream debugging
    /// </summary>
    public CommunicationStreamDebugging StreamDebugging { get; private set; }

    /// <summary>
    /// Fires when data is received from the server and returns it as a Byte array
    /// </summary>
    public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

    /// <summary>
    /// Fires when data is received from the server and returns it as text
    /// </summary>
    public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

    /// <summary>
    /// 
    /// </summary>
    //public event GenericSocketStatusChangeEventDelegate SocketStatusChange;
    public event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;


    private string _hostname;

    /// <summary>
    /// Address of server
    /// </summary>
    public string Hostname
    {
        get
        {
            return _hostname;
        }

        set
        {
            _hostname = value;
            if (_client != null)
            {
                _client.AddressClientConnectedTo = _hostname;
            }
        }
    }

    /// <summary>
    /// Port on server
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Another damn S+ helper because S+ seems to treat large port nums as signed ints
    /// which screws up things
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
    /// The actual client class
    /// </summary>
    private TCPClient _client;

    /// <summary>
    /// Bool showing if socket is connected
    /// </summary>
    public bool IsConnected
    {
        get { return _client != null && _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED; }
    }

    /// <summary>
    /// S+ helper for IsConnected
    /// </summary>
    public ushort UIsConnected
    {
        get { return (ushort)(IsConnected ? 1 : 0); }
    }

    /// <summary>
    /// _client socket status Read only
    /// </summary>
    public SocketStatus ClientStatus
    {
        get
        {
            return _client == null ? SocketStatus.SOCKET_STATUS_NO_CONNECT : _client.ClientStatus;
        }
    }

    /// <summary>
    /// Contains the familiar Simpl analog status values. This drives the ConnectionChange event
    /// and IsConnected would be true when this == 2.
    /// </summary>
    public ushort UStatus
    {
        get { return (ushort)ClientStatus; }
    }

    /// <summary>
    /// Status text shows the message associated with socket status
    /// </summary>
    public string ClientStatusText { get { return ClientStatus.ToString(); } }

    /// <summary>
    /// Connection failure reason
    /// </summary>
    public string ConnectionFailure { get { return ClientStatus.ToString(); } }

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
    /// Set only when the disconnect method is called
    /// </summary>
    bool DisconnectCalledByUser;

    /// <summary>
    /// 
    /// </summary>
    public bool Connected
    {
        get { return _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED; }
    }

    //Lock object to prevent simulatneous connect/disconnect operations
    private readonly object _connectLock = new();

    // private Timer for auto reconnect
    private Timer RetryTimer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key">unique string to differentiate between instances</param>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="bufferSize"></param>
    public GenericTcpIpClient(string key, string address, int port, int bufferSize)
        : base(key)
    {
        StreamDebugging = new CommunicationStreamDebugging(key);
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        AutoReconnectIntervalMs = 5000;
        Hostname = address;
        Port = port;
        BufferSize = bufferSize;

        SetupRetryTimer();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    public GenericTcpIpClient(string key)
        : base(key)
    {
        StreamDebugging = new CommunicationStreamDebugging(key);
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        AutoReconnectIntervalMs = 5000;
        BufferSize = 2000;

        SetupRetryTimer();
    }

    /// <summary>
    /// Default constructor for S+
    /// </summary>
    public GenericTcpIpClient()
            : base(SplusKey)
    {
        StreamDebugging = new CommunicationStreamDebugging(SplusKey);
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        AutoReconnectIntervalMs = 5000;
        BufferSize = 2000;

        SetupRetryTimer();
    }

    private void SetupRetryTimer()
    {
        RetryTimer = new Timer { AutoReset = false, Enabled = false };
        RetryTimer.Elapsed += (s, e) => Reconnect();
    }



    /// <summary>
    /// Just to help S+ set the key
    /// </summary>
    public void Initialize(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Handles closing this up when the program shuts down
    /// </summary>
    void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
    {
        if (programEventType == eProgramStatusEventType.Stopping)
        {
            this.LogInformation("Program stopping. Closing connection");
            Deactivate();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool Deactivate()
    {
        RetryTimer.Stop();
        RetryTimer.Dispose();
        if (_client != null)
        {
            _client.SocketStatusChange -= this.Client_SocketStatusChange;
            DisconnectClient();
        }
        return true;
    }

    /// <summary>
    /// Attempts to connect to the server
    /// </summary>
    public void Connect()
    {
        if (string.IsNullOrEmpty(Hostname))
        {
            this.LogWarning("GenericTcpIpClient '{0}': No address set", Key);
            return;
        }
        if (Port < 1 || Port > 65535)
        {
            {
                this.LogWarning("GenericTcpIpClient '{0}': Invalid port", Key);
                return;
            }
        }

        lock (_connectLock)
        {
            if (IsConnected)
            {
                this.LogInformation("Connection already connected. Exiting Connect()");
            }
            else
            {
                //Stop retry timer if running
                RetryTimer.Stop();
                _client = new TCPClient(Hostname, Port, BufferSize);
                _client.SocketStatusChange -= Client_SocketStatusChange;
                _client.SocketStatusChange += Client_SocketStatusChange;
                DisconnectCalledByUser = false;
                _client.ConnectToServerAsync(ConnectToServerCallback);
            }
        }
    }

    private void Reconnect()
    {
        if (_client == null)
        {
            return;
        }
        lock (_connectLock)
        {
            if (IsConnected || DisconnectCalledByUser == true)
            {
                this.LogInformation("Reconnect no longer needed. Exiting Reconnect()");
            }
            else
            {
                this.LogInformation("Attempting reconnect now");
                _client.ConnectToServerAsync(ConnectToServerCallback);
            }
        }
    }

    /// <summary>
    /// Attempts to disconnect the client
    /// </summary>
    public void Disconnect()
    {
        lock (_connectLock)
        {
            DisconnectCalledByUser = true;

            // Stop trying reconnects, if we are
            RetryTimer.Stop();
            DisconnectClient();
        }
    }

    /// <summary>
    /// Does the actual disconnect business
    /// </summary>
    public void DisconnectClient()
    {
        if (_client != null)
        {
            this.LogInformation("Disconnecting client");
            if (IsConnected)
                _client.DisconnectFromServer();
        }
    }

    /// <summary>
    /// Callback method for connection attempt
    /// </summary>
    /// <param name="c"></param>
    void ConnectToServerCallback(TCPClient c)
    {
        if (c.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
        {
            this.LogInformation("Server connection result: {0}", c.ClientStatus);
            WaitAndTryReconnect();
        }
        else
        {
            this.LogInformation("Server connection result: {0}", c.ClientStatus);
        }
    }

    /// <summary>
    /// Disconnects, waits and attemtps to connect again
    /// </summary>
    void WaitAndTryReconnect()
    {
        Task.Run(() =>
        {
            lock (_connectLock)
            {
                if (!IsConnected && AutoReconnect && !DisconnectCalledByUser && _client != null)
                {
                    DisconnectClient();
                    this.LogInformation("Attempting reconnect, status={0}", _client.ClientStatus);
                    RetryTimer.Stop();
                    RetryTimer.Interval = AutoReconnectIntervalMs;
                    RetryTimer.Start();
                }
            }
        });
    }

    /// <summary>
    /// Recieves incoming data
    /// </summary>
    /// <param name="client"></param>
    /// <param name="numBytes"></param>
    void Receive(TCPClient client, int numBytes)
    {
        if (client != null)
        {
            if (numBytes > 0)
            {
                var bytes = client.IncomingDataBuffer.Take(numBytes).ToArray();
                var bytesHandler = BytesReceived;
                if (bytesHandler != null)
                {
                    if (StreamDebugging.RxStreamDebuggingIsEnabled)
                    {
                        this.LogInformation("Received {1} bytes: '{0}'", ComTextHelper.GetEscapedText(bytes), bytes.Length);
                    }
                    bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
                }
                var textHandler = TextReceived;
                if (textHandler != null)
                {
                    var str = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);

                    if (StreamDebugging.RxStreamDebuggingIsEnabled)
                    {
                        this.LogInformation("Received {1} characters of text: '{0}'", ComTextHelper.GetDebugText(str), str.Length);
                    }

                    textHandler(this, new GenericCommMethodReceiveTextArgs(str));
                }
            }
            client.ReceiveDataAsync(Receive);
        }
    }

    /// <summary>
    /// General send method
    /// </summary>
    public void SendText(string text)
    {
        var bytes = Encoding.GetEncoding(28591).GetBytes(text);
        // Check debug level before processing byte array
        if (StreamDebugging.TxStreamDebuggingIsEnabled)
            this.LogInformation("Sending {0} characters of text: '{1}'", text.Length, ComTextHelper.GetDebugText(text));
        if (_client != null)
            _client.SendData(bytes, bytes.Length);
    }

    /// <summary>
    /// SendEscapedText method
    /// </summary>
    public void SendEscapedText(string text)
    {
        var unescapedText = Regex.Replace(text, @"\\x([0-9a-fA-F][0-9a-fA-F])", s =>
            {
                var hex = s.Groups[1].Value;
                return ((char)Convert.ToByte(hex, 16)).ToString();
            });
        SendText(unescapedText);
    }

    /// <summary>
    /// Sends Bytes to the server
    /// </summary>
    /// <param name="bytes"></param>
    public void SendBytes(byte[] bytes)
    {
        if (StreamDebugging.TxStreamDebuggingIsEnabled)
            this.LogInformation("Sending {0} bytes: '{1}'", bytes.Length, ComTextHelper.GetEscapedText(bytes));
        if (_client != null)
            _client.SendData(bytes, bytes.Length);
    }

    /// <summary>
    /// Socket Status Change Handler
    /// </summary>
    /// <param name="client"></param>
    /// <param name="clientSocketStatus"></param>
    void Client_SocketStatusChange(TCPClient client, SocketStatus clientSocketStatus)
    {
        if (clientSocketStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
        {
            this.LogDebug("Socket status change {0} ({1})", clientSocketStatus, ClientStatusText);
            WaitAndTryReconnect();
        }
        else
        {
            this.LogDebug("Socket status change {0} ({1})", clientSocketStatus, ClientStatusText);
            _client.ReceiveDataAsync(Receive);
        }

        var handler = ConnectionChange;
        if (handler != null)
            ConnectionChange(this, new GenericSocketStatusChageEventArgs(this));
    }
}

/// <summary>
/// Configuration properties for TCP/SSH Connections
/// </summary>
public class TcpSshPropertiesConfig
{
    /// <summary>
    /// Address to connect to
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string Address { get; set; }

    /// <summary>
    /// Port to connect to
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public int Port { get; set; }

    /// <summary>
    /// Username credential
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// Passord credential
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Defaults to 32768
    /// </summary>
    public int BufferSize { get; set; }

    /// <summary>
    /// Gets or sets the AutoReconnect
    /// </summary>
    public bool AutoReconnect { get; set; }

    /// <summary>
    /// Gets or sets the AutoReconnectIntervalMs
    /// </summary>
    public int AutoReconnectIntervalMs { get; set; }

    /// <summary>
    /// When true, turns off echo for the SSH session
    /// </summary>
    [JsonProperty("disableSshEcho")]
    public bool DisableSshEcho { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public TcpSshPropertiesConfig()
    {
        BufferSize = 32768;
        AutoReconnect = true;
        AutoReconnectIntervalMs = 5000;
        Username = "";
        Password = "";
    }

}

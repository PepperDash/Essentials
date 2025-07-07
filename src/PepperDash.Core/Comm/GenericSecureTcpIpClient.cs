using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using PepperDash.Core.Logging;

namespace PepperDash.Core;

/// <summary>
/// A class to handle secure TCP/IP communications with a server
/// </summary>
public class GenericSecureTcpIpClient : Device, ISocketStatusWithStreamDebugging, IAutoReconnect
{
    private const string SplusKey = "Uninitialized Secure Tcp _client";
    /// <summary>
    /// Stream debugging 
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

    #region GenericSecureTcpIpClient Events & Delegates

    /// <summary>
    /// 
    /// </summary>
    //public event GenericSocketStatusChangeEventDelegate SocketStatusChange;		
    public event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;

    /// <summary>
    /// Auto reconnect evant handler
    /// </summary>
    public event EventHandler AutoReconnectTriggered;

    /// <summary>
    /// Event for Receiving text. Once subscribed to this event the receive callback will start a thread that dequeues the messages and invokes the event on a new thread. 
    /// It is not recommended to use both the TextReceived event and the TextReceivedQueueInvoke event. 
    /// </summary>
    public event EventHandler<GenericTcpServerCommMethodReceiveTextArgs> TextReceivedQueueInvoke;
    
    /// <summary>
    /// For a client with a pre shared key, this will fire after the communication is established and the key exchange is complete. If you require
    /// a key and subscribe to the socket change event and try to send data on a connection the data sent will interfere with the key exchange and disconnect.
    /// </summary>
    public event EventHandler<GenericTcpServerClientReadyForcommunicationsEventArgs> ClientReadyForCommunications;

    #endregion


    #region GenricTcpIpClient properties

    private string _hostname;

    /// <summary>
    /// Address of server
    /// </summary>
    public string Hostname
    {
        get { return _hostname; }
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
    /// S+ helper
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
    /// Internal secure client
    /// </summary>
    private SecureTCPClient _client;

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
    /// bool to track if auto reconnect should be set on the socket
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
    /// Flag Set only when the disconnect method is called.
    /// </summary>
    bool DisconnectCalledByUser;

    /// <summary>
    /// 
    /// </summary>
    public bool Connected
    {
        get { return _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED; }
    }

    // private Timer for auto reconnect
    private CTimer RetryTimer;

    #endregion

    #region GenericSecureTcpIpClient properties

    /// <summary>
    /// Bool to show whether the server requires a preshared key. This is used in the DynamicTCPServer class
    /// </summary>
    public bool SharedKeyRequired { get; set; }

    /// <summary>
    /// S+ helper for requires shared key bool
    /// </summary>
    public ushort USharedKeyRequired
    {
        set
        {
            if (value == 1)
                SharedKeyRequired = true;
            else
                SharedKeyRequired = false;
        }
    }

    /// <summary>
    /// SharedKey is sent for varification to the server. Shared key can be any text (255 char limit in SIMPL+ Module), but must match the Shared Key on the Server module
    /// </summary>
    public string SharedKey { get; set; }

    /// <summary>
    /// flag to show the client is waiting for the server to send the shared key
    /// </summary>
    private bool WaitingForSharedKeyResponse { get; set; }

    /// <summary>
    /// Semaphore on connect method
    /// </summary>
    bool IsTryingToConnect;

    /// <summary>
    /// Bool showing if socket is ready for communication after shared key exchange
    /// </summary>
    public bool IsReadyForCommunication { get; set; }

    /// <summary>
    /// S+ helper for IsReadyForCommunication
    /// </summary>
    public ushort UIsReadyForCommunication
    {
        get { return (ushort)(IsReadyForCommunication ? 1 : 0); }
    }

    /// <summary>
    /// Bool Heartbeat Enabled flag
    /// </summary>
    public bool HeartbeatEnabled { get; set; }

    /// <summary>
    /// S+ helper for Heartbeat Enabled
    /// </summary>
    public ushort UHeartbeatEnabled
    {
        get { return (ushort)(HeartbeatEnabled ? 1 : 0); }
        set { HeartbeatEnabled = value == 1; }
    }

    /// <summary>
    /// Heartbeat String
    /// </summary>
    public string HeartbeatString { get; set; }
    //public int HeartbeatInterval = 50000;

    /// <summary>
    /// Milliseconds before server expects another heartbeat. Set by property HeartbeatRequiredIntervalInSeconds which is driven from S+
    /// </summary>
    public int HeartbeatInterval { get; set; }

    /// <summary>
    /// Simpl+ Heartbeat Analog value in seconds
    /// </summary>
    public ushort HeartbeatRequiredIntervalInSeconds { set { HeartbeatInterval = (value * 1000); } }

    CTimer HeartbeatSendTimer;
    CTimer HeartbeatAckTimer;

    // Used to force disconnection on a dead connect attempt
    CTimer ConnectFailTimer;
    CTimer WaitForSharedKey;
    private int ConnectionCount;

    bool ProgramIsStopping;

    /// <summary>
    /// Queue lock
    /// </summary>
    CCriticalSection DequeueLock = new CCriticalSection();

    /// <summary>
    /// Receive Queue size. Defaults to 20. Will set to 20 if QueueSize property is less than 20. Use constructor or set queue size property before
    /// calling initialize. 
    /// </summary>
    public int ReceiveQueueSize { get; set; }

    /// <summary>
    /// Queue to temporarily store received messages with the source IP and Port info. Defaults to size 20. Use constructor or set queue size property before
    /// calling initialize. 
    /// </summary>
    private CrestronQueue<GenericTcpServerCommMethodReceiveTextArgs> MessageQueue;

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="bufferSize"></param>
    public GenericSecureTcpIpClient(string key, string address, int port, int bufferSize)
        : base(key)
    {
        StreamDebugging = new CommunicationStreamDebugging(key);
        Hostname = address;
        Port = port;
        BufferSize = bufferSize;
        AutoReconnectIntervalMs = 5000;

        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
    }

    /// <summary>
    /// Contstructor that sets all properties by calling the initialize method with a config object. 
    /// </summary>        
    /// <param name="key"></param>
    /// <param name="clientConfigObject"></param>
    public GenericSecureTcpIpClient(string key, TcpClientConfigObject clientConfigObject)
        : base(key)
    {
        StreamDebugging = new CommunicationStreamDebugging(key);
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        AutoReconnectIntervalMs = 5000;
        BufferSize = 2000;

        Initialize(clientConfigObject);
    }

    /// <summary>
    /// Default constructor for S+
    /// </summary>
    public GenericSecureTcpIpClient()
        : base(SplusKey)
    {
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        AutoReconnectIntervalMs = 5000;
        BufferSize = 2000;
    }

    /// <summary>
    /// Just to help S+ set the key
    /// </summary>
    public void Initialize(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Initialize called by the constructor that accepts a client config object. Can be called later to reset properties of client. 
    /// </summary>
    /// <param name="config"></param>
    public void Initialize(TcpClientConfigObject config)
    {
        if (config == null)
        {
            Debug.Console(0, this, "Could not initialize client with key: {0}", Key);
            return;
        }
        try
        {
            Hostname = config.Control.TcpSshProperties.Address;
            Port = config.Control.TcpSshProperties.Port > 0 && config.Control.TcpSshProperties.Port <= 65535
                ? config.Control.TcpSshProperties.Port
                : 80;

            AutoReconnect = config.Control.TcpSshProperties.AutoReconnect;
            AutoReconnectIntervalMs = config.Control.TcpSshProperties.AutoReconnectIntervalMs > 1000
                ? config.Control.TcpSshProperties.AutoReconnectIntervalMs
                : 5000;

            SharedKey = config.SharedKey;
            SharedKeyRequired = config.SharedKeyRequired;

            HeartbeatEnabled = config.HeartbeatRequired;
            HeartbeatRequiredIntervalInSeconds = config.HeartbeatRequiredIntervalInSeconds > 0
                ? config.HeartbeatRequiredIntervalInSeconds
                : (ushort)15;


            HeartbeatString = string.IsNullOrEmpty(config.HeartbeatStringToMatch)
                ? "heartbeat"
                : config.HeartbeatStringToMatch;

            BufferSize = config.Control.TcpSshProperties.BufferSize > 2000
                ? config.Control.TcpSshProperties.BufferSize
                : 2000;

            ReceiveQueueSize = config.ReceiveQueueSize > 20
                ? config.ReceiveQueueSize
                : 20;

            MessageQueue = new CrestronQueue<GenericTcpServerCommMethodReceiveTextArgs>(ReceiveQueueSize);
        }
        catch (Exception ex)
        {
            Debug.Console(0, this, "Exception initializing client with key: {0}\rException: {1}", Key, ex);
        }
    }

    #endregion

    /// <summary>
    /// Handles closing this up when the program shuts down
    /// </summary>
    void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
    {
        if (programEventType == eProgramStatusEventType.Stopping || programEventType == eProgramStatusEventType.Paused)
        {
            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Program stopping. Closing _client connection");
            ProgramIsStopping = true;
            Disconnect();
        }

    }

    /// <summary>
    /// Deactivate the client
    /// </summary>
    /// <returns></returns>
    public override bool Deactivate()
    {
        if (_client != null)
        {
            _client.SocketStatusChange -= this.Client_SocketStatusChange;
            DisconnectClient();
        }
        return true;
    }

    /// <summary>
    /// Connect Method. Will return if already connected. Will write errors if missing address, port, or unique key/name.
    /// </summary>
    public void Connect()
    {
        ConnectionCount++;
        Debug.Console(2, this, "Attempting connect Count:{0}", ConnectionCount);


        if (IsConnected)
        {
            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Already connected. Ignoring.");
            return;
        }
        if (IsTryingToConnect)
        {
            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Already trying to connect. Ignoring.");
            return;
        }
        try
        {
            IsTryingToConnect = true;
            if (RetryTimer != null)
            {
                RetryTimer.Stop();
                RetryTimer = null;
            }
            if (string.IsNullOrEmpty(Hostname))
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning, "DynamicTcpClient: No address set");
                return;
            }
            if (Port < 1 || Port > 65535)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning, "DynamicTcpClient: Invalid port");
                return;
            }
            if (string.IsNullOrEmpty(SharedKey) && SharedKeyRequired)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Warning, "DynamicTcpClient: No Shared Key set");
                return;
            }

            // clean up previous client
            if (_client != null)
            {
                Disconnect();
            }
            DisconnectCalledByUser = false;

            _client = new SecureTCPClient(Hostname, Port, BufferSize);
            _client.SocketStatusChange += Client_SocketStatusChange;
            if (HeartbeatEnabled)
                _client.SocketSendOrReceiveTimeOutInMs = (HeartbeatInterval * 5);
            _client.AddressClientConnectedTo = Hostname;
            _client.PortNumber = Port;
            // SecureClient = c;

            //var timeOfConnect = DateTime.Now.ToString("HH:mm:ss.fff");

            ConnectFailTimer = new CTimer(o =>
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Error, "Connect attempt has not finished after 30sec Count:{0}", ConnectionCount);
                if (IsTryingToConnect)
                {
                    IsTryingToConnect = false;

                    //if (ConnectionHasHungCallback != null)
                    //{
                    //    ConnectionHasHungCallback();
                    //}		
                    //SecureClient.DisconnectFromServer();
                    //CheckClosedAndTryReconnect();
                }
            }, 30000);

            Debug.Console(2, this, "Making Connection Count:{0}", ConnectionCount);
            _client.ConnectToServerAsync(o =>
            {
                Debug.Console(2, this, "ConnectToServerAsync Count:{0} Ran!", ConnectionCount);

                if (ConnectFailTimer != null)
                {
                    ConnectFailTimer.Stop();
                }
                IsTryingToConnect = false;

                if (o.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    Debug.Console(2, this, "_client connected to {0} on port {1}", o.AddressClientConnectedTo, o.LocalPortNumberOfClient);
                    o.ReceiveDataAsync(Receive);

                    if (SharedKeyRequired)
                    {
                        WaitingForSharedKeyResponse = true;
                        WaitForSharedKey = new CTimer(timer =>
                        {

                            Debug.Console(1, this, Debug.ErrorLogLevel.Warning, "Shared key exchange timer expired. IsReadyForCommunication={0}", IsReadyForCommunication);
                            // Debug.Console(1, this, "Connect attempt failed {0}", c.ClientStatus);
                            // This is the only case where we should call DisconectFromServer...Event handeler will trigger the cleanup 
                            o.DisconnectFromServer();
                            //CheckClosedAndTryReconnect();
                            //OnClientReadyForcommunications(false); // Should send false event
                        }, 15000);
                    }
                    else
                    {
                        //CLient connected and shared key is not required so just raise the ready for communication event. if Shared key 
                        //required this is called by the shared key being negotiated
                        if (IsReadyForCommunication == false)
                        {
                            OnClientReadyForcommunications(true); // Key not required
                        }
                    }
                }
                else
                {
                    Debug.Console(1, this, "Connect attempt failed {0}", o.ClientStatus);
                    CheckClosedAndTryReconnect();
                }
            });
        }
        catch (Exception ex)
        {
            Debug.Console(0, this, Debug.ErrorLogLevel.Error, "_client connection exception: {0}", ex.Message);
            IsTryingToConnect = false;
            CheckClosedAndTryReconnect();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disconnect()
    {
        this.LogVerbose("Disconnect Called");

        DisconnectCalledByUser = true;

        // stop trying reconnects, if we are
        if (RetryTimer != null)
        {
            RetryTimer.Stop();
            RetryTimer = null;
        }

        if (_client != null)
        {
            DisconnectClient();
            this.LogDebug("Disconnected");
        }
    }

    /// <summary>
    /// Does the actual disconnect business
    /// </summary>
    public void DisconnectClient()
    {
        if (_client == null) return;

        Debug.Console(1, this, "Disconnecting client");
        if (IsConnected)
            _client.DisconnectFromServer();

        // close up client. ALWAYS use this when disconnecting.
        IsTryingToConnect = false;

        Debug.Console(2, this, "Disconnecting _client {0}", DisconnectCalledByUser ? ", Called by user" : "");
        _client.SocketStatusChange -= Client_SocketStatusChange;
        _client.Dispose();
        _client = null;

        if (ConnectFailTimer == null) return;
        ConnectFailTimer.Stop();
        ConnectFailTimer.Dispose();
        ConnectFailTimer = null;
    }
   
    #region Methods

    /// <summary>
    /// Called from Connect failure or Socket Status change if 
    /// auto reconnect and socket disconnected (Not disconnected by user)
    /// </summary>
    void CheckClosedAndTryReconnect()
    {
        if (_client != null)
        {
            Debug.Console(2, this, "Cleaning up remotely closed/failed connection.");
            Disconnect();
        }
        if (!DisconnectCalledByUser && AutoReconnect)
        {
            var halfInterval = AutoReconnectIntervalMs / 2;
            var rndTime = new Random().Next(-halfInterval, halfInterval) + AutoReconnectIntervalMs;
            Debug.Console(2, this, "Attempting reconnect in {0} ms, randomized", rndTime);
            if (RetryTimer != null)
            {
                RetryTimer.Stop();
                RetryTimer = null;
            }
            if (AutoReconnectTriggered != null)
                AutoReconnectTriggered(this, new EventArgs());
            RetryTimer = new CTimer(o => Connect(), rndTime);
        }
    }

    /// <summary>
    /// Receive callback
    /// </summary>
    /// <param name="client"></param>
    /// <param name="numBytes"></param>
    void Receive(SecureTCPClient client, int numBytes)
    {
        if (numBytes > 0)
        {
            string str = string.Empty;
            var handler = TextReceivedQueueInvoke;
            try
            {
                var bytes = client.IncomingDataBuffer.Take(numBytes).ToArray();
                str = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
                Debug.Console(2, this, "_client Received:\r--------\r{0}\r--------", str);
                if (!string.IsNullOrEmpty(checkHeartbeat(str)))
                {

                    if (SharedKeyRequired && str == "SharedKey:")
                    {
                        Debug.Console(2, this, "Server asking for shared key, sending");
                        SendText(SharedKey + "\n");
                    }
                    else if (SharedKeyRequired && str == "Shared Key Match")
                    {
                        StopWaitForSharedKeyTimer();


                        Debug.Console(2, this, "Shared key confirmed. Ready for communication");
                        OnClientReadyForcommunications(true); // Successful key exchange
                    }
                    else
                    {
                        //var bytesHandler = BytesReceived;
                        //if (bytesHandler != null)
                        //    bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
                        var textHandler = TextReceived;
                        if (textHandler != null)
                            textHandler(this, new GenericCommMethodReceiveTextArgs(str));
                        if (handler != null)
                        {
                            MessageQueue.TryToEnqueue(new GenericTcpServerCommMethodReceiveTextArgs(str));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Console(1, this, "Error receiving data: {1}. Error: {0}", ex.Message, str);
            }
            if (client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                client.ReceiveDataAsync(Receive);

            //Check to see if there is a subscription to the TextReceivedQueueInvoke event. If there is start the dequeue thread. 
            if (handler != null)
            {
                var gotLock = DequeueLock.TryEnter();
                if (gotLock)
                    CrestronInvoke.BeginInvoke((o) => DequeueEvent());
            }
        }
        else //JAG added this as I believe the error return is 0 bytes like the server. See help when hover on ReceiveAsync
        {
            client.DisconnectFromServer();
        }
    }

    /// <summary>
    /// This method gets spooled up in its own thread an protected by a CCriticalSection to prevent multiple threads from running concurrently.
    /// It will dequeue items as they are enqueued automatically.
    /// </summary>
    void DequeueEvent()
    {
        try
        {
            while (true)
            {
                // Pull from Queue and fire an event. Block indefinitely until an item can be removed, similar to a Gather.
                var message = MessageQueue.Dequeue();
                var handler = TextReceivedQueueInvoke;
                if (handler != null)
                {
                    handler(this, message);
                }
            }
        }
        catch (Exception e)
        {
            this.LogException(e, "DequeueEvent error");
        }
        // Make sure to leave the CCritical section in case an exception above stops this thread, or we won't be able to restart it.
        if (DequeueLock != null)
        {
            DequeueLock.Leave();
        }
    }

    void HeartbeatStart()
    {
        if (HeartbeatEnabled)
        {
            this.LogVerbose("Starting Heartbeat");
            if (HeartbeatSendTimer == null)
            {

                HeartbeatSendTimer = new CTimer(this.SendHeartbeat, null, HeartbeatInterval, HeartbeatInterval);
            }
            if (HeartbeatAckTimer == null)
            {
                HeartbeatAckTimer = new CTimer(HeartbeatAckTimerFail, null, (HeartbeatInterval * 2), (HeartbeatInterval * 2));
            }
        }

    }
    void HeartbeatStop()
    {

        if (HeartbeatSendTimer != null)
        {
            Debug.Console(2, this, "Stoping Heartbeat Send");
            HeartbeatSendTimer.Stop();
            HeartbeatSendTimer = null;
        }
        if (HeartbeatAckTimer != null)
        {
            Debug.Console(2, this, "Stoping Heartbeat Ack");
            HeartbeatAckTimer.Stop();
            HeartbeatAckTimer = null;
        }

    }
    void SendHeartbeat(object notused)
    {
        this.SendText(HeartbeatString);
        Debug.Console(2, this, "Sending Heartbeat");

    }

    //private method to check heartbeat requirements and start or reset timer
    string checkHeartbeat(string received)
    {
        try
        {
            if (HeartbeatEnabled)
            {
                if (!string.IsNullOrEmpty(HeartbeatString))
                {
                    var remainingText = received.Replace(HeartbeatString, "");
                    var noDelimiter = received.Trim(new char[] { '\r', '\n' });
                    if (noDelimiter.Contains(HeartbeatString))
                    {
                        if (HeartbeatAckTimer != null)
                        {
                            HeartbeatAckTimer.Reset(HeartbeatInterval * 2);
                        }
                        else
                        {
                            HeartbeatAckTimer = new CTimer(HeartbeatAckTimerFail, null, (HeartbeatInterval * 2), (HeartbeatInterval * 2));
                        }
                        Debug.Console(2, this, "Heartbeat Received: {0}, from Server", HeartbeatString);
                        return remainingText;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Console(1, this, "Error checking heartbeat: {0}", ex.Message);
        }
        return received;
    }



    void HeartbeatAckTimerFail(object o)
    {
        try
        {

            if (IsConnected)
            {
                Debug.Console(1, Debug.ErrorLogLevel.Warning, "Heartbeat not received from Server...DISCONNECTING BECAUSE HEARTBEAT REQUIRED IS TRUE");
                SendText("Heartbeat not received by server, closing connection");
                CheckClosedAndTryReconnect();
            }

        }
        catch (Exception ex)
        {
            ErrorLog.Error("Heartbeat timeout Error on _client: {0}, {1}", Key, ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void StopWaitForSharedKeyTimer()
    {
        if (WaitForSharedKey != null)
        {
            WaitForSharedKey.Stop();
            WaitForSharedKey = null;
        }
    }

    /// <summary>
    /// General send method
    /// </summary>
    public void SendText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            try
            {
                var bytes = Encoding.GetEncoding(28591).GetBytes(text);
                if (_client != null && _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    _client.SendDataAsync(bytes, bytes.Length, (c, n) =>
                    {
                        // HOW IN THE HELL DO WE CATCH AN EXCEPTION IN SENDING?????
                        if (n <= 0)
                        {
                            Debug.Console(1, Debug.ErrorLogLevel.Warning, "[{0}] Sent zero bytes. Was there an error?", this.Key);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Error sending text: {1}. Error: {0}", ex.Message, text);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SendBytes(byte[] bytes)
    {
        if (bytes.Length > 0)
        {
            try
            {
                if (_client != null && _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                    _client.SendData(bytes, bytes.Length);
            }
            catch (Exception ex)
            {
                Debug.Console(0, this, "Error sending bytes. Error: {0}", ex.Message);
            }
        }
    }

    /// <summary>
    /// SocketStatusChange Callback 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="clientSocketStatus"></param>
    void Client_SocketStatusChange(SecureTCPClient client, SocketStatus clientSocketStatus)
    {
        if (ProgramIsStopping)
        {
            ProgramIsStopping = false;
            return;
        }
        try
        {
            Debug.Console(2, this, "Socket status change: {0} ({1})", client.ClientStatus, (ushort)(client.ClientStatus));

            OnConnectionChange();
            // The client could be null or disposed by this time...
            if (_client == null || _client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                HeartbeatStop();
                OnClientReadyForcommunications(false); // socket has gone low
                CheckClosedAndTryReconnect();
            }
        }
        catch (Exception ex)
        {
            Debug.Console(1, this, Debug.ErrorLogLevel.Error, "Error in socket status change callback. Error: {0}\r\r{1}", ex, ex.InnerException);
        }
    }

    /// <summary>
    /// Helper for ConnectionChange event
    /// </summary>
    void OnConnectionChange()
    {
        var handler = ConnectionChange;
        if (handler == null) return;

        handler(this, new GenericSocketStatusChageEventArgs(this));
    }

    /// <summary>
    /// Helper to fire ClientReadyForCommunications event
    /// </summary>
    void OnClientReadyForcommunications(bool isReady)
    {
        IsReadyForCommunication = isReady;
        if (IsReadyForCommunication) 
            HeartbeatStart();

        var handler = ClientReadyForCommunications;
        if (handler == null) return;
        
        handler(this, new GenericTcpServerClientReadyForcommunicationsEventArgs(IsReadyForCommunication));
    }
    #endregion
}
/*PepperDash Technology Corp.
JAG
Copyright:		2017
------------------------------------
***Notice of Ownership and Copyright***
The material in which this notice appears is the property of PepperDash Technology Corporation,
which claims copyright under the laws of the United States of America in the entire body of material
and in all parts thereof, regardless of the use to which it is being put.  Any use, in whole or in part,
of this material by another party without the express written permission of PepperDash Technology Corporation is prohibited.
PepperDash Technology Corporation reserves all rights under applicable laws.
------------------------------------ */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using PepperDash.Core.Logging;

namespace PepperDash.Core;

/// <summary>
/// Generic secure TCP/IP server
/// </summary>
public class GenericSecureTcpIpServer : Device
{
    #region Events
    /// <summary>
    /// Event for Receiving text
    /// </summary>
    public event EventHandler<GenericTcpServerCommMethodReceiveTextArgs> TextReceived;

    /// <summary>
    /// Event for Receiving text. Once subscribed to this event the receive callback will start a thread that dequeues the messages and invokes the event on a new thread. 
    /// It is not recommended to use both the TextReceived event and the TextReceivedQueueInvoke event. 
    /// </summary>
    public event EventHandler<GenericTcpServerCommMethodReceiveTextArgs> TextReceivedQueueInvoke;

    /// <summary>
    /// Event for client connection socket status change
    /// </summary>
    public event EventHandler<GenericTcpServerSocketStatusChangeEventArgs> ClientConnectionChange;

    /// <summary>
    /// Event for Server State Change
    /// </summary>
    public event EventHandler<GenericTcpServerStateChangedEventArgs> ServerStateChange;

    /// <summary>
    /// For a server with a pre shared key, this will fire after the communication is established and the key exchange is complete. If no shared key, this will fire
    /// after connection is successful. Use this event to know when the client is ready for communication to avoid stepping on shared key. 
    /// </summary>
    public event EventHandler<GenericTcpServerSocketStatusChangeEventArgs> ServerClientReadyForCommunications;

    /// <summary>
    /// A band aid event to notify user that the server has choked.
    /// </summary>
    public ServerHasChokedCallbackDelegate ServerHasChoked { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public delegate void ServerHasChokedCallbackDelegate();

    #endregion

    #region Properties/Variables

    /// <summary>
    /// Server listen lock
    /// </summary>
    CCriticalSection ServerCCSection = new CCriticalSection();

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

    /// <summary>
    /// A bandaid client that monitors whether the server is reachable
    /// </summary>
    GenericSecureTcpIpClient_ForServer MonitorClient;

    /// <summary>
    /// Timer to operate the bandaid monitor client in a loop.
    /// </summary>
    CTimer MonitorClientTimer;

    /// <summary>
    /// 
    /// </summary>
    int MonitorClientFailureCount;

    /// <summary>
    /// 3 by default
    /// </summary>
    public int MonitorClientMaxFailureCount { get; set; }

    /// <summary>
    /// Text representation of the Socket Status enum values for the server
    /// </summary>
    public string Status
    {
        get
        {
            if (SecureServer != null)
                return SecureServer.State.ToString();
            return ServerState.SERVER_NOT_LISTENING.ToString();

        }

    }

    /// <summary>
    /// Bool showing if socket is connected
    /// </summary>
    public bool IsConnected
    {
        get
        {
            if (SecureServer != null)
                return (SecureServer.State & ServerState.SERVER_CONNECTED) == ServerState.SERVER_CONNECTED;
            return false;

            //return (Secure ? SecureServer != null : UnsecureServer != null) && 
            //(Secure ? (SecureServer.State & ServerState.SERVER_CONNECTED) == ServerState.SERVER_CONNECTED :
            //          (UnsecureServer.State & ServerState.SERVER_CONNECTED) == ServerState.SERVER_CONNECTED); 
        }
    }

    /// <summary>
    /// S+ helper for IsConnected
    /// </summary>
    public ushort UIsConnected
    {
        get { return (ushort)(IsConnected ? 1 : 0); }
    }

    /// <summary>
    /// Bool showing if socket is connected
    /// </summary>
    public bool IsListening
    {
        get
        {
            if (SecureServer != null)
                return (SecureServer.State & ServerState.SERVER_LISTENING) == ServerState.SERVER_LISTENING;
            else
                return false;
            //return (Secure ? SecureServer != null : UnsecureServer != null) &&
            //(Secure ? (SecureServer.State & ServerState.SERVER_LISTENING) == ServerState.SERVER_LISTENING :
            //          (UnsecureServer.State & ServerState.SERVER_LISTENING) == ServerState.SERVER_LISTENING);
        }
    }

    /// <summary>
    /// S+ helper for IsConnected
    /// </summary>
    public ushort UIsListening
    {
        get { return (ushort)(IsListening ? 1 : 0); }
    }
    /// <summary>
    /// Max number of clients this server will allow for connection. Crestron max is 64. This number should be less than 65 
    /// </summary>
    public ushort MaxClients { get; set; } // should be set by parameter in SIMPL+ in the MAIN method, Should not ever need to be configurable
    /// <summary>
    /// Number of clients currently connected.
    /// </summary>
    public ushort NumberOfClientsConnected
    {
        get
        {
            if (SecureServer != null)
                return (ushort)SecureServer.NumberOfClientsConnected;
            return 0;
        }
    }

    /// <summary>
    /// Port Server should listen on
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// S+ helper for Port
    /// </summary>
    public ushort UPort
    {
        get { return Convert.ToUInt16(Port); }
        set { Port = Convert.ToInt32(value); }
    }

    /// <summary>
    /// Bool to show whether the server requires a preshared key. Must be set the same in the client, and if true shared keys must be identical on server/client
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
    /// SharedKey is sent for varification to the server. Shared key can be any text (255 char limit in SIMPL+ Module), but must match the Shared Key on the Server module. 
    /// If SharedKey changes while server is listening or clients are connected, disconnect and stop listening will be called
    /// </summary>
    public string SharedKey { get; set; }

    /// <summary>
    /// Heartbeat Required bool sets whether server disconnects client if heartbeat is not received
    /// </summary>
    public bool HeartbeatRequired { get; set; }

    /// <summary>
    /// S+ Helper for Heartbeat Required
    /// </summary>
    public ushort UHeartbeatRequired
    {
        set
        {
            if (value == 1)
                HeartbeatRequired = true;
            else
                HeartbeatRequired = false;
        }
    }

    /// <summary>
    /// Milliseconds before server expects another heartbeat. Set by property HeartbeatRequiredIntervalInSeconds which is driven from S+
    /// </summary>
    public int HeartbeatRequiredIntervalMs { get; set; }

    /// <summary>
    /// Simpl+ Heartbeat Analog value in seconds
    /// </summary>
    public ushort HeartbeatRequiredIntervalInSeconds { set { HeartbeatRequiredIntervalMs = (value * 1000); } }

    /// <summary>
    /// String to Match for heartbeat. If null or empty any string will reset heartbeat timer
    /// </summary>
    public string HeartbeatStringToMatch { get; set; }

    //private timers for Heartbeats per client
    Dictionary<uint, CTimer> HeartbeatTimerDictionary = new Dictionary<uint, CTimer>();

    //flags to show the secure server is waiting for client at index to send the shared key
    List<uint> WaitingForSharedKey = new List<uint>();

    List<uint> ClientReadyAfterKeyExchange = new List<uint>();

    /// <summary>
    /// The connected client indexes
    /// </summary>
    public List<uint> ConnectedClientsIndexes = new List<uint>();

    /// <summary>
    /// Defaults to 2000
    /// </summary>
    public int BufferSize { get; set; }

    /// <summary>
    /// Private flag to note that the server has stopped intentionally
    /// </summary>
    private bool ServerStopped { get; set; }

    //Servers
    SecureTCPServer SecureServer;

    /// <summary>
    /// 
    /// </summary>
    bool ProgramIsStopping;

    #endregion

    #region Constructors
    /// <summary>
    /// constructor S+ Does not accept a key. Use initialze with key to set the debug key on this device. If using with + make sure to set all properties manually.
    /// </summary>
    public GenericSecureTcpIpServer()
        : base("Uninitialized Secure TCP Server")
    {
        HeartbeatRequiredIntervalInSeconds = 15;
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        BufferSize = 2000;
        MonitorClientMaxFailureCount = 3;
    }

    /// <summary>
    /// constructor with debug key set at instantiation. Make sure to set all properties before listening. 
    /// </summary>
    /// <param name="key"></param>
    public GenericSecureTcpIpServer(string key)
        : base("Uninitialized Secure TCP Server")
    {
        HeartbeatRequiredIntervalInSeconds = 15;
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        BufferSize = 2000;
        MonitorClientMaxFailureCount = 3;
        Key = key;
    }

    /// <summary>
    /// Contstructor that sets all properties by calling the initialize method with a config object. This does set Queue size. 
    /// </summary>
    /// <param name="serverConfigObject"></param>
    public GenericSecureTcpIpServer(TcpServerConfigObject serverConfigObject)
        : base("Uninitialized Secure TCP Server")
    {
        HeartbeatRequiredIntervalInSeconds = 15;
        CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        BufferSize = 2000;
        MonitorClientMaxFailureCount = 3;
        Initialize(serverConfigObject);
    }
    #endregion

    #region Methods - Server Actions
    /// <summary>
    /// Disconnects all clients and stops the server
    /// </summary>
    public void KillServer()
    {
        ServerStopped = true;
        if (MonitorClient != null)
        {
            MonitorClient.Disconnect();
        }
        DisconnectAllClientsForShutdown();
        StopListening();
    }

    /// <summary>
    /// Initialize Key for device using client name from SIMPL+. Called on Listen from SIMPL+
    /// </summary>
    /// <param name="key"></param>
    public void Initialize(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Initialze the server
    /// </summary>
    /// <param name="serverConfigObject"></param>
    public void Initialize(TcpServerConfigObject serverConfigObject)
    {
        try
        {
            if (serverConfigObject != null || string.IsNullOrEmpty(serverConfigObject.Key))
            {
                Key = serverConfigObject.Key;
                MaxClients = serverConfigObject.MaxClients;
                Port = serverConfigObject.Port;
                SharedKeyRequired = serverConfigObject.SharedKeyRequired;
                SharedKey = serverConfigObject.SharedKey;
                HeartbeatRequired = serverConfigObject.HeartbeatRequired;
                HeartbeatRequiredIntervalInSeconds = serverConfigObject.HeartbeatRequiredIntervalInSeconds;
                HeartbeatStringToMatch = serverConfigObject.HeartbeatStringToMatch;
                BufferSize = serverConfigObject.BufferSize;
                ReceiveQueueSize = serverConfigObject.ReceiveQueueSize > 20 ? serverConfigObject.ReceiveQueueSize : 20;
                MessageQueue = new CrestronQueue<GenericTcpServerCommMethodReceiveTextArgs>(ReceiveQueueSize);
            }
            else
            {
                ErrorLog.Error("Could not initialize server with key: {0}", serverConfigObject.Key);
            }
        }
        catch
        {
            ErrorLog.Error("Could not initialize server with key: {0}", serverConfigObject.Key);
        }
    }

    /// <summary>
    /// Start listening on the specified port
    /// </summary>
    public void Listen()
    {
        ServerCCSection.Enter();
        try
        {
            if (Port < 1 || Port > 65535)
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Error, "Server '{0}': Invalid port", Key);
                ErrorLog.Warn(string.Format("Server '{0}': Invalid port", Key));
                return;
            }
            if (string.IsNullOrEmpty(SharedKey) && SharedKeyRequired)
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Error, "Server '{0}': No Shared Key set", Key);
                ErrorLog.Warn(string.Format("Server '{0}': No Shared Key set", Key));
                return;
            }


            if (SecureServer == null)
            {
                SecureServer = new SecureTCPServer(Port, MaxClients);
                if (HeartbeatRequired)
                    SecureServer.SocketSendOrReceiveTimeOutInMs = (this.HeartbeatRequiredIntervalMs * 5);
                SecureServer.HandshakeTimeout = 30;
                SecureServer.SocketStatusChange += new SecureTCPServerSocketStatusChangeEventHandler(SecureServer_SocketStatusChange);
            }
            else
            {
                SecureServer.PortNumber = Port;
            }
            ServerStopped = false;

            // Start the listner
            SocketErrorCodes status = SecureServer.WaitForConnectionAsync("0.0.0.0", SecureConnectCallback);
            if (status != SocketErrorCodes.SOCKET_OPERATION_PENDING)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Error starting WaitForConnectionAsync {0}", status);
            }
            else
            {
                ServerStopped = false;
            }
            OnServerStateChange(SecureServer.State);
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Secure Server Status: {0}, Socket Status: {1}", SecureServer.State, SecureServer.ServerSocketStatus);
            ServerCCSection.Leave();

        }
        catch (Exception ex)
        {
            ServerCCSection.Leave();
            ErrorLog.Error("{1} Error with Dynamic Server: {0}", ex.ToString(), Key);
        }
    }

    /// <summary>
    /// Stop Listeneing
    /// </summary>
    public void StopListening()
    {
        try
        {
            Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Stopping Listener");
            if (SecureServer != null)
            {
                SecureServer.Stop();
                Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Server State: {0}", SecureServer.State);
                OnServerStateChange(SecureServer.State);
            }
            ServerStopped = true;
        }
        catch (Exception ex)
        {
            Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error stopping server. Error: {0}", ex);
        }
    }

    /// <summary>
    /// Disconnects Client
    /// </summary>
    /// <param name="client"></param>
    public void DisconnectClient(uint client)
    {
        try
        {
            SecureServer.Disconnect(client);
            Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Disconnected client index: {0}", client);
        }
        catch (Exception ex)
        {
            Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error Disconnecting client index: {0}. Error: {1}", client, ex);
        }
    }
    /// <summary>
    /// Disconnect All Clients
    /// </summary>
    public void DisconnectAllClientsForShutdown()
    {
        Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Disconnecting All Clients");
        if (SecureServer != null)
        {
            SecureServer.SocketStatusChange -= SecureServer_SocketStatusChange;
            foreach (var index in ConnectedClientsIndexes.ToList()) // copy it here so that it iterates properly
            {
                var i = index;
                if (!SecureServer.ClientConnected(index))
                    continue;
                try
                {
                    SecureServer.Disconnect(i);
                    Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Disconnected client index: {0}", i);
                }
                catch (Exception ex)
                {
                    Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error Disconnecting client index: {0}. Error: {1}", i, ex);
                }
            }
            Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Server Status: {0}", SecureServer.ServerSocketStatus);
        }

        Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Disconnected All Clients");
        ConnectedClientsIndexes.Clear();

        if (!ProgramIsStopping)
        {
            OnConnectionChange();
            OnServerStateChange(SecureServer.State); //State shows both listening and connected
        }

        // var o = new { };
    }

    /// <summary>
    /// Broadcast text from server to all connected clients
    /// </summary>
    /// <param name="text"></param>
    public void BroadcastText(string text)
    {
        CCriticalSection CCBroadcast = new CCriticalSection();
        CCBroadcast.Enter();
        try
        {
            if (ConnectedClientsIndexes.Count > 0)
            {
                byte[] b = Encoding.GetEncoding(28591).GetBytes(text);
                foreach (uint i in ConnectedClientsIndexes)
                {
                    if (!SharedKeyRequired || (SharedKeyRequired && ClientReadyAfterKeyExchange.Contains(i)))
                    {
                        SocketErrorCodes error = SecureServer.SendDataAsync(i, b, b.Length, (x, y, z) => { });
                        if (error != SocketErrorCodes.SOCKET_OK && error != SocketErrorCodes.SOCKET_OPERATION_PENDING)
                            this.LogVerbose("{error}", error);
                    }
                }
            }
            CCBroadcast.Leave();
        }
        catch (Exception ex)
        {
            CCBroadcast.Leave();
            Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error Broadcasting messages from server. Error: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Not sure this is useful in library, maybe Pro??
    /// </summary>
    /// <param name="text"></param>
    /// <param name="clientIndex"></param>
    public void SendTextToClient(string text, uint clientIndex)
    {
        try
        {
            byte[] b = Encoding.GetEncoding(28591).GetBytes(text);
            if (SecureServer != null && SecureServer.GetServerSocketStatusForSpecificClient(clientIndex) == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                if (!SharedKeyRequired || (SharedKeyRequired && ClientReadyAfterKeyExchange.Contains(clientIndex)))
                    SecureServer.SendDataAsync(clientIndex, b, b.Length, (x, y, z) => { });
            }
        }
        catch (Exception ex)
        {
            Debug.Console(2, this, "Error sending text to client. Text: {1}. Error: {0}", ex.Message, text);
        }
    }

    //private method to check heartbeat requirements and start or reset timer
    string checkHeartbeat(uint clientIndex, string received)
    {
        try
        {
            if (HeartbeatRequired)
            {
                if (!string.IsNullOrEmpty(HeartbeatStringToMatch))
                {
                    var remainingText = received.Replace(HeartbeatStringToMatch, "");
                    var noDelimiter = received.Trim(new char[] { '\r', '\n' });
                    if (noDelimiter.Contains(HeartbeatStringToMatch))
                    {
                        if (HeartbeatTimerDictionary.ContainsKey(clientIndex))
                            HeartbeatTimerDictionary[clientIndex].Reset(HeartbeatRequiredIntervalMs);
                        else
                        {
                            CTimer HeartbeatTimer = new CTimer(HeartbeatTimer_CallbackFunction, clientIndex, HeartbeatRequiredIntervalMs);
                            HeartbeatTimerDictionary.Add(clientIndex, HeartbeatTimer);
                        }
                        Debug.Console(1, this, "Heartbeat Received: {0}, from client index: {1}", HeartbeatStringToMatch, clientIndex);
                        // Return Heartbeat
                        SendTextToClient(HeartbeatStringToMatch, clientIndex);
                        return remainingText;
                    }
                }
                else
                {
                    if (HeartbeatTimerDictionary.ContainsKey(clientIndex))
                        HeartbeatTimerDictionary[clientIndex].Reset(HeartbeatRequiredIntervalMs);
                    else
                    {
                        CTimer HeartbeatTimer = new CTimer(HeartbeatTimer_CallbackFunction, clientIndex, HeartbeatRequiredIntervalMs);
                        HeartbeatTimerDictionary.Add(clientIndex, HeartbeatTimer);
                    }
                    Debug.Console(1, this, "Heartbeat Received: {0}, from client index: {1}", received, clientIndex);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Console(1, this, "Error checking heartbeat: {0}", ex.Message);
        }
        return received;
    }

    /// <summary>
    /// Get the IP Address for the client at the specifed index
    /// </summary>
    /// <param name="clientIndex"></param>
    /// <returns></returns>
    public string GetClientIPAddress(uint clientIndex)
    {
        Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "GetClientIPAddress Index: {0}", clientIndex);
        if (!SharedKeyRequired || (SharedKeyRequired && ClientReadyAfterKeyExchange.Contains(clientIndex)))
        {
            var ipa = this.SecureServer.GetAddressServerAcceptedConnectionFromForSpecificClient(clientIndex);
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "GetClientIPAddress IPAddreess: {0}", ipa);
            return ipa;

        }
        else
        {
            return "";
        }
    }

    #endregion

    #region Methods - HeartbeatTimer Callback

    void HeartbeatTimer_CallbackFunction(object o)
    {
        uint clientIndex = 99999;
        string address = string.Empty;
        try
        {
            clientIndex = (uint)o;
            address = SecureServer.GetAddressServerAcceptedConnectionFromForSpecificClient(clientIndex);

            Debug.Console(1, this, Debug.ErrorLogLevel.Warning, "Heartbeat not received for Client index {2} IP: {0}, DISCONNECTING BECAUSE HEARTBEAT REQUIRED IS TRUE {1}",
                address, string.IsNullOrEmpty(HeartbeatStringToMatch) ? "" : ("HeartbeatStringToMatch: " + HeartbeatStringToMatch), clientIndex);

            if (SecureServer.GetServerSocketStatusForSpecificClient(clientIndex) == SocketStatus.SOCKET_STATUS_CONNECTED)
                SendTextToClient("Heartbeat not received by server, closing connection", clientIndex);

            var discoResult = SecureServer.Disconnect(clientIndex);
            //Debug.Console(1, this, "{0}", discoResult);  

            if (HeartbeatTimerDictionary.ContainsKey(clientIndex))
            {
                HeartbeatTimerDictionary[clientIndex].Stop();
                HeartbeatTimerDictionary[clientIndex].Dispose();
                HeartbeatTimerDictionary.Remove(clientIndex);
            }
        }
        catch (Exception ex)
        {
            ErrorLog.Error("{3}: Heartbeat timeout Error on Client Index: {0}, at address: {1}, error: {2}", clientIndex, address, ex.Message, Key);
        }
    }

    #endregion

    #region Methods - Socket Status Changed Callbacks
    /// <summary>
    /// Secure Server Socket Status Changed Callback
    /// </summary>
    /// <param name="server"></param>
    /// <param name="clientIndex">Index of the client whose socket status has changed</param>
    /// <param name="status">New socket status</param>
    void SecureServer_SocketStatusChange(TCPServer server, uint clientIndex, SocketStatus status)
    {
        try
        {
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "SecureServerSocketStatusChange ConnectedClients: {0} ServerState: {1} Port: {2} ClientIndex: {3} Status: {4}",
                server.NumberOfClientsConnected, server.State, server.PortNumber, clientIndex, status);

            // Handle connection limit and listening state
            if (server.MaxNumberOfClientSupported > server.NumberOfClientsConnected)
            {
                Listen();
            }
        }
        catch (Exception ex)
        {
            Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error in Socket Status Change Callback. Error: {0}", ex);
        }
        //Use a thread for this event so that the server state updates to listening while this event is processed. Listening must be added to the server state
        //after every client connection so that the server can check and see if it is at max clients. Due to this the event fires and server listening enum bit flag
        //is not set. Putting in a thread allows the state to update before this event processes so that the subscribers to this event get accurate isListening in the event. 
        CrestronInvoke.BeginInvoke(o => onConnectionChange(clientIndex, server.GetServerSocketStatusForSpecificClient(clientIndex)), null);
    }

    #endregion

    #region Methods Connected Callbacks
    /// <summary>
    /// Secure TCP Client Connected to Secure Server Callback
    /// </summary>
    /// <param name="server"></param>
    /// <param name="clientIndex"></param>
    void SecureConnectCallback(SecureTCPServer server, uint clientIndex)
    {
        try
        {
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "ConnectCallback: IPAddress: {0}. Index: {1}. Status: {2}",
                server.GetAddressServerAcceptedConnectionFromForSpecificClient(clientIndex),
                clientIndex, server.GetServerSocketStatusForSpecificClient(clientIndex));
            if (clientIndex != 0)
            {
                if (server.ClientConnected(clientIndex))
                {

                    if (!ConnectedClientsIndexes.Contains(clientIndex))
                    {
                        ConnectedClientsIndexes.Add(clientIndex);
                    }
                    if (SharedKeyRequired)
                    {
                        if (!WaitingForSharedKey.Contains(clientIndex))
                        {
                            WaitingForSharedKey.Add(clientIndex);
                        }
                        byte[] b = Encoding.GetEncoding(28591).GetBytes("SharedKey:");
                        server.SendDataAsync(clientIndex, b, b.Length, (x, y, z) => { });
                        Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Sent Shared Key Request to client at {0}", server.GetAddressServerAcceptedConnectionFromForSpecificClient(clientIndex));
                    }
                    else
                    {
                        OnServerClientReadyForCommunications(clientIndex);
                    }
                    if (HeartbeatRequired)
                    {
                        if (!HeartbeatTimerDictionary.ContainsKey(clientIndex))
                        {
                            HeartbeatTimerDictionary.Add(clientIndex, new CTimer(HeartbeatTimer_CallbackFunction, clientIndex, HeartbeatRequiredIntervalMs));
                        }
                    }

                    server.ReceiveDataAsync(clientIndex, SecureReceivedDataAsyncCallback);
                }
            }
            else
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Error, "Client attempt faulty.");
            }
        }
        catch (Exception ex)
        {
            Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error in Socket Status Connect Callback. Error: {0}", ex);
        }

        // Rearm the listner 
        SocketErrorCodes status = server.WaitForConnectionAsync("0.0.0.0", SecureConnectCallback);
        if (status != SocketErrorCodes.SOCKET_OPERATION_PENDING)
        {
            Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Socket status connect callback status {0}", status);
            if (status == SocketErrorCodes.SOCKET_CONNECTION_IN_PROGRESS)
            {
                // There is an issue where on a failed negotiation we need to stop and start the server. This should still leave connected clients intact. 
                server.Stop();
                Listen();
            }
        }
    }

    #endregion

    #region Methods - Send/Receive Callbacks
    /// <summary>
    /// Secure Received Data Async Callback
    /// </summary>
    /// <param name="mySecureTCPServer"></param>
    /// <param name="clientIndex"></param>
    /// <param name="numberOfBytesReceived"></param>
    void SecureReceivedDataAsyncCallback(SecureTCPServer mySecureTCPServer, uint clientIndex, int numberOfBytesReceived)
    {
        if (numberOfBytesReceived > 0)
        {

            string received = "Nothing";
            var handler = TextReceivedQueueInvoke;
            try
            {
                byte[] bytes = mySecureTCPServer.GetIncomingDataBufferForSpecificClient(clientIndex);
                received = System.Text.Encoding.GetEncoding(28591).GetString(bytes, 0, numberOfBytesReceived);
                if (WaitingForSharedKey.Contains(clientIndex))
                {
                    received = received.Replace("\r", "");
                    received = received.Replace("\n", "");
                    if (received != SharedKey)
                    {
                        byte[] b = Encoding.GetEncoding(28591).GetBytes("Shared key did not match server. Disconnecting");
                        Debug.Console(1, this, Debug.ErrorLogLevel.Warning, "Client at index {0} Shared key did not match the server, disconnecting client. Key: {1}", clientIndex, received);
                        mySecureTCPServer.SendData(clientIndex, b, b.Length);
                        mySecureTCPServer.Disconnect(clientIndex);

                        return;
                    }

                    WaitingForSharedKey.Remove(clientIndex);
                    byte[] success = Encoding.GetEncoding(28591).GetBytes("Shared Key Match");
                    mySecureTCPServer.SendDataAsync(clientIndex, success, success.Length, null);
                    OnServerClientReadyForCommunications(clientIndex);
                    Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Client with index {0} provided the shared key and successfully connected to the server", clientIndex);
                }
                else if (!string.IsNullOrEmpty(checkHeartbeat(clientIndex, received)))
                {
                    onTextReceived(received, clientIndex);
                    if (handler != null)
                    {
                        MessageQueue.TryToEnqueue(new GenericTcpServerCommMethodReceiveTextArgs(received, clientIndex));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error Receiving data: {0}. Error: {1}", received, ex);
            }
            if (mySecureTCPServer.GetServerSocketStatusForSpecificClient(clientIndex) == SocketStatus.SOCKET_STATUS_CONNECTED)
                mySecureTCPServer.ReceiveDataAsync(clientIndex, SecureReceivedDataAsyncCallback);

            //Check to see if there is a subscription to the TextReceivedQueueInvoke event. If there is start the dequeue thread. 
            if (handler != null)
            {
                var gotLock = DequeueLock.TryEnter();
                if (gotLock)
                    CrestronInvoke.BeginInvoke((o) => DequeueEvent());
            }
        }
        else
        {
            mySecureTCPServer.Disconnect(clientIndex);
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

    #endregion

    #region Methods - EventHelpers/Callbacks

    //Private Helper method to call the Connection Change Event
    void onConnectionChange(uint clientIndex, SocketStatus clientStatus)
    {
        if (clientIndex != 0) //0 is error not valid client change
        {
            var handler = ClientConnectionChange;
            if (handler != null)
            {
                handler(this, new GenericTcpServerSocketStatusChangeEventArgs(SecureServer, clientIndex, clientStatus));
            }
        }
    }

    //Private Helper method to call the Connection Change Event
    void OnConnectionChange()
    {
        if (ProgramIsStopping)
        {
            return;
        }
        var handler = ClientConnectionChange;
        if (handler != null)
        {
            handler(this, new GenericTcpServerSocketStatusChangeEventArgs());
        }
    }

    //Private Helper Method to call the Text Received Event
    void onTextReceived(string text, uint clientIndex)
    {
        var handler = TextReceived;
        if (handler != null)
            handler(this, new GenericTcpServerCommMethodReceiveTextArgs(text, clientIndex));
    }

    //Private Helper Method to call the Server State Change Event
    void OnServerStateChange(ServerState state)
    {
        if (ProgramIsStopping)
        {
            return;
        }
        var handler = ServerStateChange;
        if (handler != null)
        {
            handler(this, new GenericTcpServerStateChangedEventArgs(state));
        }
    }

    /// <summary>
    /// Private Event Handler method to handle the closing of connections when the program stops
    /// </summary>
    /// <param name="programEventType"></param>
    void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
    {
        if (programEventType == eProgramStatusEventType.Stopping)
        {
            ProgramIsStopping = true;
            // kill bandaid things
            if (MonitorClientTimer != null)
                MonitorClientTimer.Stop();
            if (MonitorClient != null)
                MonitorClient.Disconnect();

            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Program stopping. Closing server");
            KillServer();
        }
    }

    //Private event handler method to raise the event that the server is ready to send data after a successful client shared key negotiation
    void OnServerClientReadyForCommunications(uint clientIndex)
    {
        ClientReadyAfterKeyExchange.Add(clientIndex);
        var handler = ServerClientReadyForCommunications;
        if (handler != null)
            handler(this, new GenericTcpServerSocketStatusChangeEventArgs(
                this, clientIndex, SecureServer.GetServerSocketStatusForSpecificClient(clientIndex)));
    }
    #endregion

    #region Monitor Client
    /// <summary>
    /// Starts the monitor client cycle. Timed wait, then call RunMonitorClient
    /// </summary>
    void StartMonitorClient()
    {
        if (MonitorClientTimer != null)
        {
            return;
        }
        MonitorClientTimer = new CTimer(o => RunMonitorClient(), 60000);
    }

    /// <summary>
    /// 
    /// </summary>
    void RunMonitorClient()
    {
        MonitorClient = new GenericSecureTcpIpClient_ForServer(Key + "-MONITOR", "127.0.0.1", Port, 2000);
        MonitorClient.SharedKeyRequired = this.SharedKeyRequired;
        MonitorClient.SharedKey = this.SharedKey;
        MonitorClient.ConnectionHasHungCallback = MonitorClientHasHungCallback;
        //MonitorClient.ConnectionChange += MonitorClient_ConnectionChange;
        MonitorClient.ClientReadyForCommunications += MonitorClient_IsReadyForComm;

        Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Starting monitor check");

        MonitorClient.Connect();
        // From here MonitorCLient either connects or hangs, MonitorClient will call back 

    }

    /// <summary>
    /// 
    /// </summary>
    void StopMonitorClient()
    {
        if (MonitorClient == null)
            return;

        MonitorClient.ClientReadyForCommunications -= MonitorClient_IsReadyForComm;
        MonitorClient.Disconnect();
        MonitorClient = null;
    }

    /// <summary>
    /// On monitor connect, restart the operation
    /// </summary>
    void MonitorClient_IsReadyForComm(object sender, GenericTcpServerClientReadyForcommunicationsEventArgs args)
    {
        if (args.IsReady)
        {
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Monitor client connection success. Disconnecting in 2s");
            MonitorClientTimer.Stop();
            MonitorClientTimer = null;
            MonitorClientFailureCount = 0;
            CrestronEnvironment.Sleep(2000);
            StopMonitorClient();
            StartMonitorClient();
        }
    }

    /// <summary>
    /// If the client hangs, add to counter and maybe fire the choke event
    /// </summary>
    void MonitorClientHasHungCallback()
    {
        MonitorClientFailureCount++;
        MonitorClientTimer.Stop();
        MonitorClientTimer = null;
        StopMonitorClient();
        if (MonitorClientFailureCount < MonitorClientMaxFailureCount)
        {
            Debug.Console(2, this, Debug.ErrorLogLevel.Warning, "Monitor client connection has hung {0} time{1}, maximum {2}",
                MonitorClientFailureCount, MonitorClientFailureCount > 1 ? "s" : "", MonitorClientMaxFailureCount);
            StartMonitorClient();
        }
        else
        {
            Debug.Console(2, this, Debug.ErrorLogLevel.Error,
                "\r***************************\rMonitor client connection has hung a maximum of {0} times. \r***************************",
                MonitorClientMaxFailureCount);

            var handler = ServerHasChoked;
            if (handler != null)
                handler();
            // Some external thing is in charge here.  Expected reset of program
        }
    }
    #endregion
}
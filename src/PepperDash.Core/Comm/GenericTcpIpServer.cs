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

namespace PepperDash.Core
{
    /// <summary>
    /// Generic TCP/IP server device
    /// </summary>
    public class GenericTcpIpServer : Device
    {
        #region Events
        /// <summary>
        /// Event for Receiving text
        /// </summary>
        public event EventHandler<GenericTcpServerCommMethodReceiveTextArgs> TextReceived;

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
        /// 
        /// </summary>
        CCriticalSection ServerCCSection = new CCriticalSection();


        /// <summary>
        /// A bandaid client that monitors whether the server is reachable
        /// </summary>
        GenericTcpIpClient_ForServer MonitorClient;

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
                if (myTcpServer != null)
                    return myTcpServer.State.ToString();
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
                if (myTcpServer != null)
                    return (myTcpServer.State & ServerState.SERVER_CONNECTED) == ServerState.SERVER_CONNECTED;
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
                if (myTcpServer != null)
                    return (myTcpServer.State & ServerState.SERVER_LISTENING) == ServerState.SERVER_LISTENING;
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
        /// The maximum number of clients.
        /// Should be set by parameter in SIMPL+ in the MAIN method, Should not ever need to be configurable
        /// </summary>
        public ushort MaxClients { get; set; }

        /// <summary>
        /// Number of clients currently connected.
        /// </summary>
        public ushort NumberOfClientsConnected
        {
            get
            {
                if (myTcpServer != null)
                    return (ushort)myTcpServer.NumberOfClientsConnected;
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
        TCPServer myTcpServer;

        /// <summary>
        /// 
        /// </summary>
        bool ProgramIsStopping;

        #endregion

        #region Constructors
        /// <summary>
        /// constructor S+ Does not accept a key. Use initialze with key to set the debug key on this device. If using with + make sure to set all properties manually.
        /// </summary>
        public GenericTcpIpServer()
            : base("Uninitialized Dynamic TCP Server")
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
        public GenericTcpIpServer(string key)
            : base("Uninitialized Dynamic TCP Server")
        {
            HeartbeatRequiredIntervalInSeconds = 15;
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
            BufferSize = 2000;
            MonitorClientMaxFailureCount = 3;
            Key = key;
        }

        /// <summary>
        /// Contstructor that sets all properties by calling the initialize method with a config object.
        /// </summary>
        /// <param name="serverConfigObject"></param>
        public GenericTcpIpServer(TcpServerConfigObject serverConfigObject)
            : base("Uninitialized Dynamic TCP Server")
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
        /// Initialze with server configuration object
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
                if (IsListening)
                    return;

                if (myTcpServer == null)
                {
                    myTcpServer = new TCPServer(Port, MaxClients);
                    if(HeartbeatRequired)
                        myTcpServer.SocketSendOrReceiveTimeOutInMs = (this.HeartbeatRequiredIntervalMs * 5);
                    
					// myTcpServer.HandshakeTimeout = 30;
                }
                else
                {
                    KillServer();
                    myTcpServer.PortNumber = Port;
                }

                myTcpServer.SocketStatusChange -= TcpServer_SocketStatusChange;
                myTcpServer.SocketStatusChange += TcpServer_SocketStatusChange;

                ServerStopped = false;
                myTcpServer.WaitForConnectionAsync(IPAddress.Any, TcpConnectCallback);
                OnServerStateChange(myTcpServer.State);
                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "TCP Server Status: {0}, Socket Status: {1}", myTcpServer.State, myTcpServer.ServerSocketStatus);

                // StartMonitorClient();


                ServerCCSection.Leave();
            }
            catch (Exception ex)
            {
                ServerCCSection.Leave();
                ErrorLog.Error("{1} Error with Dynamic Server: {0}", ex.ToString(), Key);
            }
        }

        /// <summary>
        /// Stop Listening
        /// </summary>
        public void StopListening()
        {
            try
            {
                Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Stopping Listener");
                if (myTcpServer != null)
                {
                    myTcpServer.Stop();
                    Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Server State: {0}", myTcpServer.State);
					OnServerStateChange(myTcpServer.State);
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
                myTcpServer.Disconnect(client);
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
            if (myTcpServer != null)
            {
                myTcpServer.SocketStatusChange -= TcpServer_SocketStatusChange;
                foreach (var index in ConnectedClientsIndexes.ToList()) // copy it here so that it iterates properly
                {
                    var i = index;
                    if (!myTcpServer.ClientConnected(index))
                        continue;
                    try
                    {
                        myTcpServer.Disconnect(i);
                        Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Disconnected client index: {0}", i);
                    }
                    catch (Exception ex)
                    {
                        Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error Disconnecting client index: {0}. Error: {1}", i, ex);
                    }
                }
                Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Server Status: {0}", myTcpServer.ServerSocketStatus);
            }

            Debug.Console(2, this, Debug.ErrorLogLevel.Notice, "Disconnected All Clients");
            ConnectedClientsIndexes.Clear();

            if (!ProgramIsStopping)
            {
                OnConnectionChange();
                OnServerStateChange(myTcpServer.State); //State shows both listening and connected
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
                            SocketErrorCodes error = myTcpServer.SendDataAsync(i, b, b.Length, (x, y, z) => { });
                            if (error != SocketErrorCodes.SOCKET_OK && error != SocketErrorCodes.SOCKET_OPERATION_PENDING)
                                Debug.Console(2, error.ToString());
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
                if (myTcpServer != null && myTcpServer.GetServerSocketStatusForSpecificClient(clientIndex) == SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    if (!SharedKeyRequired || (SharedKeyRequired && ClientReadyAfterKeyExchange.Contains(clientIndex)))
                        myTcpServer.SendDataAsync(clientIndex, b, b.Length, (x, y, z) => { });
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
        /// Gets the IP address based on the client index
        /// </summary>
        /// <param name="clientIndex"></param>
        /// <returns>IP address of the client</returns>
        public string GetClientIPAddress(uint clientIndex)
        {
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "GetClientIPAddress Index: {0}", clientIndex);
            if (!SharedKeyRequired || (SharedKeyRequired && ClientReadyAfterKeyExchange.Contains(clientIndex)))
            {
                var ipa = this.myTcpServer.GetAddressServerAcceptedConnectionFromForSpecificClient(clientIndex);
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
                address = myTcpServer.GetAddressServerAcceptedConnectionFromForSpecificClient(clientIndex);

                Debug.Console(1, this, Debug.ErrorLogLevel.Warning, "Heartbeat not received for Client index {2} IP: {0}, DISCONNECTING BECAUSE HEARTBEAT REQUIRED IS TRUE {1}",
                    address, string.IsNullOrEmpty(HeartbeatStringToMatch) ? "" : ("HeartbeatStringToMatch: " + HeartbeatStringToMatch), clientIndex);

                if (myTcpServer.GetServerSocketStatusForSpecificClient(clientIndex) == SocketStatus.SOCKET_STATUS_CONNECTED)
                    SendTextToClient("Heartbeat not received by server, closing connection", clientIndex);

                var discoResult = myTcpServer.Disconnect(clientIndex);
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
        /// <param name="clientIndex"></param>
        /// <param name="serverSocketStatus"></param>
        void TcpServer_SocketStatusChange(TCPServer server, uint clientIndex, SocketStatus serverSocketStatus)
        {
            try
            {

                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "SecureServerSocketStatusChange Index:{0} status:{1} Port:{2} IP:{3}", clientIndex, serverSocketStatus, this.myTcpServer.GetPortNumberServerAcceptedConnectionFromForSpecificClient(clientIndex), this.myTcpServer.GetLocalAddressServerAcceptedConnectionFromForSpecificClient(clientIndex));
                if (serverSocketStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    if (ConnectedClientsIndexes.Contains(clientIndex))
                        ConnectedClientsIndexes.Remove(clientIndex);
                    if (HeartbeatRequired && HeartbeatTimerDictionary.ContainsKey(clientIndex))
                    {
                        HeartbeatTimerDictionary[clientIndex].Stop();
                        HeartbeatTimerDictionary[clientIndex].Dispose();
                        HeartbeatTimerDictionary.Remove(clientIndex);
                    }
                    if (ClientReadyAfterKeyExchange.Contains(clientIndex))
                        ClientReadyAfterKeyExchange.Remove(clientIndex);
					if (WaitingForSharedKey.Contains(clientIndex))
						WaitingForSharedKey.Remove(clientIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error in Socket Status Change Callback. Error: {0}", ex);
            }
            onConnectionChange(clientIndex, server.GetServerSocketStatusForSpecificClient(clientIndex));
        }

        #endregion

        #region Methods Connected Callbacks
        /// <summary>
        /// Secure TCP Client Connected to Secure Server Callback
        /// </summary>
        /// <param name="server"></param>
        /// <param name="clientIndex"></param>
        void TcpConnectCallback(TCPServer server, uint clientIndex)
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

                        server.ReceiveDataAsync(clientIndex, TcpServerReceivedDataAsyncCallback);
                    }
                }
                else
                {
                    Debug.Console(1, this, Debug.ErrorLogLevel.Error, "Client attempt faulty.");
                    if (!ServerStopped)
                    {
                        server.WaitForConnectionAsync(IPAddress.Any, TcpConnectCallback);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error in Socket Status Connect Callback. Error: {0}", ex);
            }
            //Debug.Console(1, this, Debug.ErrorLogLevel, "((((((Server State bitfield={0}; maxclient={1}; ServerStopped={2}))))))",
            //    server.State, 
            //    MaxClients,
            //    ServerStopped);
            if ((server.State & ServerState.SERVER_LISTENING) != ServerState.SERVER_LISTENING && MaxClients > 1 && !ServerStopped)
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Waiting for next connection");
                server.WaitForConnectionAsync(IPAddress.Any, TcpConnectCallback);

            }
        }

        #endregion

        #region Methods - Send/Receive Callbacks
        /// <summary>
        /// Secure Received Data Async Callback
        /// </summary>
        /// <param name="myTCPServer"></param>
        /// <param name="clientIndex"></param>
        /// <param name="numberOfBytesReceived"></param>
        void TcpServerReceivedDataAsyncCallback(TCPServer myTCPServer, uint clientIndex, int numberOfBytesReceived)
        {
			if (numberOfBytesReceived > 0)
			{
				string received = "Nothing";
				try
				{
					byte[] bytes = myTCPServer.GetIncomingDataBufferForSpecificClient(clientIndex);
					received = System.Text.Encoding.GetEncoding(28591).GetString(bytes, 0, numberOfBytesReceived);
					if (WaitingForSharedKey.Contains(clientIndex))
					{
						received = received.Replace("\r", "");
						received = received.Replace("\n", "");
						if (received != SharedKey)
						{
							byte[] b = Encoding.GetEncoding(28591).GetBytes("Shared key did not match server. Disconnecting");
							Debug.Console(1, this, Debug.ErrorLogLevel.Warning, "Client at index {0} Shared key did not match the server, disconnecting client. Key: {1}", clientIndex, received);
							myTCPServer.SendData(clientIndex, b, b.Length);
							myTCPServer.Disconnect(clientIndex);
							return;
						}

						WaitingForSharedKey.Remove(clientIndex);
						byte[] success = Encoding.GetEncoding(28591).GetBytes("Shared Key Match");
						myTCPServer.SendDataAsync(clientIndex, success, success.Length, null);
						OnServerClientReadyForCommunications(clientIndex);
						Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Client with index {0} provided the shared key and successfully connected to the server", clientIndex);
					}

					else if (!string.IsNullOrEmpty(checkHeartbeat(clientIndex, received)))
						onTextReceived(received, clientIndex);
				}
				catch (Exception ex)
				{
					Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Error Receiving data: {0}. Error: {1}", received, ex);
				}
				if (myTCPServer.GetServerSocketStatusForSpecificClient(clientIndex) == SocketStatus.SOCKET_STATUS_CONNECTED)
					myTCPServer.ReceiveDataAsync(clientIndex, TcpServerReceivedDataAsyncCallback);
			}
			else
			{
				// If numberOfBytesReceived <= 0
				myTCPServer.Disconnect();
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
                    handler(this, new GenericTcpServerSocketStatusChangeEventArgs(myTcpServer, clientIndex, clientStatus));
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
                    this, clientIndex, myTcpServer.GetServerSocketStatusForSpecificClient(clientIndex)));
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
            MonitorClient = new GenericTcpIpClient_ForServer(Key + "-MONITOR", "127.0.0.1", Port, 2000);
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
                    "\r***************************\rMonitor client connection has hung a maximum of {0} times.\r***************************",
                    MonitorClientMaxFailureCount);

                var handler = ServerHasChoked;
                if (handler != null)
                    handler();
                // Some external thing is in charge here.  Expected reset of program
            }
        }
        #endregion
    }
}
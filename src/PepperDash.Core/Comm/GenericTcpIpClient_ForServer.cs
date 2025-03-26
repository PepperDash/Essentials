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
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace PepperDash.Core
{
    /// <summary>
    /// Generic TCP/IP client for server
    /// </summary>
    public class GenericTcpIpClient_ForServer : Device, IAutoReconnect
    {
        /// <summary>
        /// Band aid delegate for choked server
        /// </summary>
        internal delegate void ConnectionHasHungCallbackDelegate();

        #region Events

        //public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

        /// <summary>
        /// Notifies of text received
        /// </summary>
        public event EventHandler<GenericTcpServerCommMethodReceiveTextArgs> TextReceived;

        /// <summary>
        /// Notifies of socket status change
        /// </summary>
        public event EventHandler<GenericTcpServerSocketStatusChangeEventArgs> ConnectionChange;


        /// <summary>
        /// This is something of a band-aid callback. If the client times out during the connection process, because the server
        /// is stuck, this will fire.  It is intended to be used by the Server class monitor client, to help
        /// keep a watch on the server and reset it if necessary.
        /// </summary>
        internal ConnectionHasHungCallbackDelegate ConnectionHasHungCallback;

        /// <summary>
        /// For a client with a pre shared key, this will fire after the communication is established and the key exchange is complete. If you require
        /// a key and subscribe to the socket change event and try to send data on a connection the data sent will interfere with the key exchange and disconnect.
        /// </summary>
        public event EventHandler<GenericTcpServerClientReadyForcommunicationsEventArgs> ClientReadyForCommunications;

        #endregion

        #region Properties & Variables

        /// <summary>
        /// Address of server
        /// </summary>
        public string Hostname { get; set; }

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
        /// Defaults to 2000
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Semaphore on connect method
        /// </summary>
        bool IsTryingToConnect;

        /// <summary>
        /// Bool showing if socket is connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (Client != null)
                    return Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;
                else
                    return false;
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
        /// Client socket status Read only
        /// </summary>
        public SocketStatus ClientStatus
        {
            get
            {
                if (Client != null)
                    return Client.ClientStatus;
                else
                    return SocketStatus.SOCKET_STATUS_NO_CONNECT;
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
        /// private Timer for auto reconnect
        /// </summary>
        CTimer RetryTimer;


        /// <summary>
        /// 
        /// </summary>
        public bool HeartbeatEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort UHeartbeatEnabled
        {
            get { return (ushort)(HeartbeatEnabled ? 1 : 0); }
            set { HeartbeatEnabled = value == 1; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string HeartbeatString = "heartbeat";

        /// <summary>
        /// 
        /// </summary>
        public int HeartbeatInterval = 50000;

        CTimer HeartbeatSendTimer;
        CTimer HeartbeatAckTimer;
        /// <summary>
        /// Used to force disconnection on a dead connect attempt
        /// </summary>
        CTimer ConnectFailTimer;
        CTimer WaitForSharedKey;
        private int ConnectionCount;
        /// <summary>
        /// Internal secure client
        /// </summary>
        TCPClient Client;

        bool ProgramIsStopping;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="bufferSize"></param>
        public GenericTcpIpClient_ForServer(string key, string address, int port, int bufferSize)
            : base(key)
        {
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
            Hostname = address;
            Port = port;
            BufferSize = bufferSize;
            AutoReconnectIntervalMs = 5000;

        }

        /// <summary>
        /// Constructor for S+
        /// </summary>
        public GenericTcpIpClient_ForServer()
            : base("Uninitialized DynamicTcpClient")
        {
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
            AutoReconnectIntervalMs = 5000;
            BufferSize = 2000;
        }
        #endregion

        #region Methods

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
            if (programEventType == eProgramStatusEventType.Stopping || programEventType == eProgramStatusEventType.Paused)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Program stopping. Closing Client connection");
                ProgramIsStopping = true;
                Disconnect();
            }

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
                if (Client != null)
                {
                    Cleanup();
                }
                DisconnectCalledByUser = false;

                Client = new TCPClient(Hostname, Port, BufferSize);
                Client.SocketStatusChange += Client_SocketStatusChange;
                if(HeartbeatEnabled)
                    Client.SocketSendOrReceiveTimeOutInMs = (HeartbeatInterval * 5);
                Client.AddressClientConnectedTo = Hostname;
                Client.PortNumber = Port;
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

                Debug.Console(2, this,  "Making Connection Count:{0}", ConnectionCount);
                Client.ConnectToServerAsync(o =>
                {
                    Debug.Console(2, this, "ConnectToServerAsync Count:{0} Ran!", ConnectionCount);

                    if (ConnectFailTimer != null)
                    {
                        ConnectFailTimer.Stop();
                    }
                    IsTryingToConnect = false;

                    if (o.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                    {
                        Debug.Console(2, this, "Client connected to {0} on port {1}", o.AddressClientConnectedTo, o.LocalPortNumberOfClient);
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
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Client connection exception: {0}", ex.Message);
                IsTryingToConnect = false;
                CheckClosedAndTryReconnect();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disconnect()
        {
            Debug.Console(2, "Disconnect Called");

            DisconnectCalledByUser = true;
            if (IsConnected)
            {
                Client.DisconnectFromServer();

            }
            if (RetryTimer != null)
            {
                RetryTimer.Stop();
                RetryTimer = null;
            }
            Cleanup();
        }

        /// <summary>
        ///  Internal call to close up client. ALWAYS use this when disconnecting.
        /// </summary>
        void Cleanup()
        {
            IsTryingToConnect = false;

            if (Client != null)
            {
                //SecureClient.DisconnectFromServer();
                Debug.Console(2, this, "Disconnecting Client {0}", DisconnectCalledByUser ? ", Called by user" : "");
                Client.SocketStatusChange -= Client_SocketStatusChange;
                Client.Dispose();
                Client = null;
            }
            if (ConnectFailTimer != null)
            {
                ConnectFailTimer.Stop();
                ConnectFailTimer.Dispose();
                ConnectFailTimer = null;
            }
        }


        /// <summary>ff
        /// Called from Connect failure or Socket Status change if 
        /// auto reconnect and socket disconnected (Not disconnected by user)
        /// </summary>
        void CheckClosedAndTryReconnect()
        {
            if (Client != null)
            {
                Debug.Console(2, this, "Cleaning up remotely closed/failed connection.");
                Cleanup();
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
                RetryTimer = new CTimer(o => Connect(), rndTime);
            }
        }

        /// <summary>
        /// Receive callback
        /// </summary>
        /// <param name="client"></param>
        /// <param name="numBytes"></param>
        void Receive(TCPClient client, int numBytes)
        {
            if (numBytes > 0)
            {
                string str = string.Empty;

                try
                {
                    var bytes = client.IncomingDataBuffer.Take(numBytes).ToArray();
                    str = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
                    Debug.Console(2, this, "Client Received:\r--------\r{0}\r--------", str);
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
                                textHandler(this, new GenericTcpServerCommMethodReceiveTextArgs(str));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Console(1, this, "Error receiving data: {1}. Error: {0}", ex.Message, str);
                }
            }
            if (client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                client.ReceiveDataAsync(Receive);
        }

        void HeartbeatStart()
        {
            if (HeartbeatEnabled)
            {
                Debug.Console(2, this,  "Starting Heartbeat");
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
                Debug.Console(2, this,  "Stoping Heartbeat Send");
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
                ErrorLog.Error("Heartbeat timeout Error on Client: {0}, {1}", Key, ex);
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
                    if (Client != null && Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                    {
                        Client.SendDataAsync(bytes, bytes.Length, (c, n) =>
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
                    if (Client != null && Client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                        Client.SendData(bytes, bytes.Length);
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
        void Client_SocketStatusChange(TCPClient client, SocketStatus clientSocketStatus)
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
                if (Client == null || Client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
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
            if (handler != null)
                ConnectionChange(this, new GenericTcpServerSocketStatusChangeEventArgs(this, Client.ClientStatus));
        }

        /// <summary>
        /// Helper to fire ClientReadyForCommunications event
        /// </summary>
        void OnClientReadyForcommunications(bool isReady)
        {
            IsReadyForCommunication = isReady;
            if (this.IsReadyForCommunication) { HeartbeatStart(); }
            var handler = ClientReadyForCommunications;
            if (handler != null)
                handler(this, new GenericTcpServerClientReadyForcommunicationsEventArgs(IsReadyForCommunication));
        }
        #endregion
    }
    
}
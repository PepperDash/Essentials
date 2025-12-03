using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Newtonsoft.Json;

namespace PepperDash.Core
{
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
        /// Gets or sets the Port
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
        /// Ushort representation of client status
        /// </summary>
        [Obsolete]
        public ushort UClientStatus { get { return (ushort)ClientStatus; } }

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
        private CCriticalSection connectLock = new CCriticalSection();

        // private Timer for auto reconnect
        private CTimer RetryTimer;

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

            RetryTimer = new CTimer(o =>
            {
                Reconnect();
            }, Timeout.Infinite);
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

            RetryTimer = new CTimer(o =>
            {
                Reconnect();
            }, Timeout.Infinite);
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

            RetryTimer = new CTimer(o =>
            {
                Reconnect();
            }, Timeout.Infinite);
        }

        /// <summary>
        /// Initialize method
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
                Debug.Console(1, this, "Program stopping. Closing connection");
                Deactivate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Deactivate method
        /// </summary>
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
        /// Connect method
        /// </summary>
        public void Connect()
        {
            if (string.IsNullOrEmpty(Hostname))
            {
                Debug.Console(1, Debug.ErrorLogLevel.Warning, "GenericTcpIpClient '{0}': No address set", Key);
                return;
            }
            if (Port < 1 || Port > 65535)
            {
                {
                    Debug.Console(1, Debug.ErrorLogLevel.Warning, "GenericTcpIpClient '{0}': Invalid port", Key);
                    return;
                }
            }

            try
            {
                connectLock.Enter();
                if (IsConnected)
                {
                    Debug.Console(1, this, "Connection already connected. Exiting Connect()");
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
            finally
            {
                connectLock.Leave();
            }
        }

        private void Reconnect()
        {
            if (_client == null)
            {
                return;
            }
            try
            {
                connectLock.Enter();
                if (IsConnected || DisconnectCalledByUser == true)
                {
                    Debug.Console(1, this, "Reconnect no longer needed. Exiting Reconnect()");
                }
                else
                {
                    Debug.Console(1, this, "Attempting reconnect now");
                    _client.ConnectToServerAsync(ConnectToServerCallback);
                }
            }
            finally
            {
                connectLock.Leave();
            }
        }

        /// <summary>
        /// Disconnect method
        /// </summary>
        public void Disconnect()
        {
            try
            {
                connectLock.Enter();
                DisconnectCalledByUser = true;

                // Stop trying reconnects, if we are
                RetryTimer.Stop();
                DisconnectClient();
            }
            finally
            {
                connectLock.Leave();
            }
        }

        /// <summary>
        /// DisconnectClient method
        /// </summary>
        public void DisconnectClient()
        {
            if (_client != null)
            {
                Debug.Console(1, this, "Disconnecting client");
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
                Debug.Console(0, this, "Server connection result: {0}", c.ClientStatus);
                WaitAndTryReconnect();
            }
            else
            {
                Debug.Console(1, this, "Server connection result: {0}", c.ClientStatus);
            }
        }

        /// <summary>
        /// Disconnects, waits and attemtps to connect again
        /// </summary>
		void WaitAndTryReconnect()
        {
            CrestronInvoke.BeginInvoke(o =>
            {
                try
                {
                    connectLock.Enter();
                    if (!IsConnected && AutoReconnect && !DisconnectCalledByUser && _client != null)
                    {
                        DisconnectClient();
                        Debug.Console(1, this, "Attempting reconnect, status={0}", _client.ClientStatus);
                        RetryTimer.Reset(AutoReconnectIntervalMs);
                    }
                }
                finally
                {
                    connectLock.Leave();
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
                        this.PrintReceivedBytes(bytes);
                        bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
                    }
                    var textHandler = TextReceived;
                    if (textHandler != null)
                    {
                        var str = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);

                        this.PrintReceivedText(str);

                        textHandler(this, new GenericCommMethodReceiveTextArgs(str));
                    }
                }
                client.ReceiveDataAsync(Receive);
            }
        }

        /// <summary>
        /// SendText method
        /// </summary>
        public void SendText(string text)
        {
            var bytes = Encoding.GetEncoding(28591).GetBytes(text);
            // Check debug level before processing byte array
            this.PrintSentText(text);
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
        /// <summary>
        /// SendBytes method
        /// </summary>
        public void SendBytes(byte[] bytes)
        {
            this.PrintSentBytes(bytes);
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
                Debug.Console(0, this, "Socket status change {0} ({1})", clientSocketStatus, ClientStatusText);
                WaitAndTryReconnect();
            }
            else
            {
                Debug.Console(1, this, "Socket status change {0} ({1})", clientSocketStatus, ClientStatusText);
                _client.ReceiveDataAsync(Receive);
            }

            var handler = ConnectionChange;
            if (handler != null)
                ConnectionChange(this, new GenericSocketStatusChageEventArgs(this));
        }
    }

    /// <summary>
    /// Represents a TcpSshPropertiesConfig
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
        /// Gets or sets the Password
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
            DisableSshEcho = false;
        }
    }
}

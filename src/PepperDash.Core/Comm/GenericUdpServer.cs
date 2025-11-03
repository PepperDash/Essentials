
using System;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Newtonsoft.Json;
using PepperDash.Core.Logging;

namespace PepperDash.Core
{
    /// <summary>
    /// Generic UDP Server device
    /// </summary>
    public class GenericUdpServer : Device, ISocketStatusWithStreamDebugging
    {
        private const string SplusKey = "Uninitialized Udp Server";
        /// <summary>
        /// Object to enable stream debugging
        /// </summary>
        public CommunicationStreamDebugging StreamDebugging { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        /// <summary>
        /// This event will fire when a message is dequeued that includes the source IP and Port info if needed to determine the source of the received data.
        /// </summary>
		public event EventHandler<GenericUdpReceiveTextExtraArgs> DataRecievedExtra;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<GenericUdpConnectedEventArgs> UpdateConnectionStatus;

        /// <summary>
        /// 
        /// </summary>
        public SocketStatus ClientStatus
        {
            get
            {
                return Server.ServerStatus;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort UStatus
        {
            get { return (ushort)Server.ServerStatus; }
        }

        /// <summary>
        /// Address of server
        /// </summary>
        public string Hostname { get; set; }


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
        /// Indicates that the UDP Server is enabled
        /// </summary>
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// Numeric value indicating 
        /// </summary>
        public ushort UIsConnected
        {
            get { return IsConnected ? (ushort)1 : (ushort)0; }
        }

        /// <summary>
        /// Defaults to 2000
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// The server
        /// </summary>
        public UDPServer Server { get; private set; }

        /// <summary>
        /// Constructor for S+. Make sure to set key, address, port, and buffersize using init method
        /// </summary>
        public GenericUdpServer()
            : base(SplusKey)
        {
            StreamDebugging = new CommunicationStreamDebugging(SplusKey);
            BufferSize = 5000;

            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
            CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(CrestronEnvironment_EthernetEventHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="bufferSize"></param>
        public GenericUdpServer(string key, string address, int port, int bufferSize)
            : base(key)
        {
            StreamDebugging = new CommunicationStreamDebugging(key);
            Hostname = address;
            Port = port;
            BufferSize = bufferSize;

            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
            CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(CrestronEnvironment_EthernetEventHandler);
        }

        /// <summary>
        /// Call from S+ to initialize values
        /// </summary>
        /// <param name="key"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <summary>
        /// Initialize method
        /// </summary>
        public void Initialize(string key, string address, ushort port)
        {
            Key = key;
            Hostname = address;
            UPort = port;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ethernetEventArgs"></param>
        void CrestronEnvironment_EthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            // Re-enable the server if the link comes back up and the status should be connected
            if (ethernetEventArgs.EthernetEventType == eEthernetEventType.LinkUp
                && IsConnected)
            {
                Connect();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="programEventType"></param>
        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType != eProgramStatusEventType.Stopping)
                return;

            Debug.Console(1, this, "Program stopping. Disabling Server");
            Disconnect();
        }

        /// <summary>
        /// Connect method
        /// </summary>
        public void Connect()
        {
            if (Server == null)
            {
                try
                {
                    var address = IPAddress.Parse(Hostname);

                    Server = new UDPServer(address, Port, BufferSize);

                }
                catch (Exception ex)
                {
                    this.LogError("Error parsing IP Address '{ipAddress}': message: {message}", Hostname, ex.Message);
                    this.LogInformation("Creating UDPServer with default buffersize");

                    Server = new UDPServer();
                }

            }

            if (string.IsNullOrEmpty(Hostname))
            {
                Debug.Console(1, Debug.ErrorLogLevel.Warning, "GenericUdpServer '{0}': No address set", Key);
                return;
            }
            if (Port < 1 || Port > 65535)
            {
                {
                    Debug.Console(1, Debug.ErrorLogLevel.Warning, "GenericUdpServer '{0}': Invalid port", Key);
                    return;
                }
            }

            var status = Server.EnableUDPServer(Hostname, Port);

            Debug.Console(2, this, "SocketErrorCode: {0}", status);
            if (status == SocketErrorCodes.SOCKET_OK)
                IsConnected = true;

            var handler = UpdateConnectionStatus;
            if (handler != null)
                handler(this, new GenericUdpConnectedEventArgs(UIsConnected));

            // Start receiving data
            Server.ReceiveDataAsync(Receive);
        }

        /// <summary>
        /// Disconnect method
        /// </summary>
        public void Disconnect()
        {
            if (Server != null)
                Server.DisableUDPServer();

            IsConnected = false;

            var handler = UpdateConnectionStatus;
            if (handler != null)
                handler(this, new GenericUdpConnectedEventArgs(UIsConnected));
        }


        /// <summary>
        /// Recursive method to receive data
        /// </summary>
        /// <param name="server"></param>
        /// <param name="numBytes"></param>
        void Receive(UDPServer server, int numBytes)
        {
            Debug.Console(2, this, "Received {0} bytes", numBytes);

            try
            {
                if (numBytes <= 0)
                    return;

                var sourceIp = Server.IPAddressLastMessageReceivedFrom;
                var sourcePort = Server.IPPortLastMessageReceivedFrom;
                var bytes = server.IncomingDataBuffer.Take(numBytes).ToArray();
                var str = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);

                var dataRecivedExtra = DataRecievedExtra;
                if (dataRecivedExtra != null)
                    dataRecivedExtra(this, new GenericUdpReceiveTextExtraArgs(str, sourceIp, sourcePort, bytes));

                Debug.Console(2, this, "Bytes: {0}", bytes.ToString());
                var bytesHandler = BytesReceived;
                if (bytesHandler != null)
                {
                    this.PrintReceivedBytes(bytes);
                    bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
                }
                var textHandler = TextReceived;
                if (textHandler != null)
                {
                    this.PrintReceivedText(str);
                    textHandler(this, new GenericCommMethodReceiveTextArgs(str));
                }
            }
            catch (Exception ex)
            {
                this.LogException(ex, "GenericUdpServer Receive error");
            }
            finally
            {
                server.ReceiveDataAsync(Receive);
            }
        }

        /// <summary>
        /// General send method
        /// </summary>
        /// <param name="text"></param>
        /// <summary>
        /// SendText method
        /// </summary>
        public void SendText(string text)
        {
            var bytes = Encoding.GetEncoding(28591).GetBytes(text);

            if (IsConnected && Server != null)
            {
                this.PrintSentText(text);

                Server.SendData(bytes, bytes.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <summary>
        /// SendBytes method
        /// </summary>
        public void SendBytes(byte[] bytes)
        {
            this.PrintSentBytes(bytes);

            if (IsConnected && Server != null)
                Server.SendData(bytes, bytes.Length);
        }

    }

    /// <summary>
    /// Represents a GenericUdpReceiveTextExtraArgs
    /// </summary>
    public class GenericUdpReceiveTextExtraArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
		public string Text { get; private set; }
        /// <summary>
        /// 
        /// </summary>
		public string IpAddress { get; private set; }
        /// <summary>
        /// 
        /// </summary>
		public int Port { get; private set; }
        /// <summary>
        /// 
        /// </summary>
		public byte[] Bytes { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="bytes"></param>
		public GenericUdpReceiveTextExtraArgs(string text, string ipAddress, int port, byte[] bytes)
        {
            Text = text;
            IpAddress = ipAddress;
            Port = port;
            Bytes = bytes;
        }

        /// <summary>
        /// Stupid S+ Constructor
        /// </summary>
        public GenericUdpReceiveTextExtraArgs() { }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UdpServerPropertiesConfig
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int Port { get; set; }

        /// <summary>
        /// Defaults to 32768
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public UdpServerPropertiesConfig()
        {
            BufferSize = 32768;
        }
    }
}
using System;
using System.Text;
using System.Threading;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Org.BouncyCastle.Utilities;
using PepperDash.Core.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace PepperDash.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class GenericSshClient : Device, ISocketStatusWithStreamDebugging, IAutoReconnect
    {
        private const string SPlusKey = "Uninitialized SshClient";
        /// <summary>
        /// Object to enable stream debugging
        /// </summary>
        public CommunicationStreamDebugging StreamDebugging { get; private set; }

        /// <summary>
        /// Event that fires when data is received.  Delivers args with byte array
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

        /// <summary>
        /// Event that fires when data is received.  Delivered as text.
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        /// <summary>
        /// Event when the connection status changes.
        /// </summary>
        public event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;

        /// <summary>
        ///// 
        ///// </summary>
        //public event GenericSocketStatusChangeEventDelegate SocketStatusChange;

        /// <summary>
        /// Gets or sets the Hostname
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Port on server
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// True when the server is connected - when status == 2.
        /// </summary>
        public bool IsConnected
        {
            // returns false if no client or not connected
            get { return Client != null && ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED; }
        }

        /// <summary>
        /// S+ helper for IsConnected
        /// </summary>
        public ushort UIsConnected
        {
            get { return (ushort)(IsConnected ? 1 : 0); }
        }

        /// <summary>
        /// Socket status change event
        /// </summary>
        public SocketStatus ClientStatus
        {
            get { return _ClientStatus; }
            private set
            {
                if (_ClientStatus == value)
                    return;
                _ClientStatus = value;
                OnConnectionChange();
            }
        }
        SocketStatus _ClientStatus;

        /// <summary>
        /// Contains the familiar Simpl analog status values. This drives the ConnectionChange event
        /// and IsConnected with be true when this == 2.
        /// </summary>
        public ushort UStatus
        {
            get { return (ushort)_ClientStatus; }
        }

        /// <summary>
        /// Determines whether client will attempt reconnection on failure. Default is true
        /// </summary>
        public bool AutoReconnect { get; set; }

        /// <summary>
        /// Will be set and unset by connect and disconnect only
        /// </summary>
        public bool ConnectEnabled { get; private set; }

        /// <summary>
        /// S+ helper for AutoReconnect
        /// </summary>
        public ushort UAutoReconnect
        {
            get { return (ushort)(AutoReconnect ? 1 : 0); }
            set { AutoReconnect = value == 1; }
        }

        /// <summary>
        /// Gets or sets the AutoReconnectIntervalMs
        /// </summary>
        public int AutoReconnectIntervalMs { get; set; }

        SshClient Client;

        ShellStream TheStream;

        CTimer ReconnectTimer;

        //Lock object to prevent simulatneous connect/disconnect operations
        //private CCriticalSection connectLock = new CCriticalSection();
        private SemaphoreSlim connectLock = new SemaphoreSlim(1);

        private bool DisconnectLogged = false;

        /// <summary>
        /// Typical constructor.
        /// </summary>
        public GenericSshClient(string key, string hostname, int port, string username, string password) :
            base(key)
        {
            StreamDebugging = new CommunicationStreamDebugging(key);
			CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
			Key = key;
			Hostname = hostname;
			Port = port;
			Username = username;
			Password = password; 
			AutoReconnectIntervalMs = 5000;

            ReconnectTimer = new CTimer(o =>
	            {
                    if (ConnectEnabled)
                    {
                        Connect();
                    }
	            }, System.Threading.Timeout.Infinite);
		}

		/// <summary>
		/// S+ Constructor - Must set all properties before calling Connect
		/// </summary>
		public GenericSshClient()
			: base(SPlusKey)
		{
			CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
			AutoReconnectIntervalMs = 5000;

            ReconnectTimer = new CTimer(o =>
            {
                if (ConnectEnabled)
                {
                    Connect();
                }
            }, System.Threading.Timeout.Infinite);
		}

		/// <summary>
		/// Handles closing this up when the program shuts down
		/// </summary>
		void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
		{
			if (programEventType == eProgramStatusEventType.Stopping)
			{
				if (Client != null)
				{
					this.LogDebug("Program stopping. Closing connection");
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Connect method
        /// </summary>
        public void Connect()
        {
            // Don't go unless everything is here
            if (string.IsNullOrEmpty(Hostname) || Port < 1 || Port > 65535
                || Username == null || Password == null)
            {
                this.LogError("Connect failed.  Check hostname, port, username and password are set or not null");
                return;
            }

            ConnectEnabled = true;

            try
            {
                connectLock.Wait();
                if (IsConnected)
                {
                    this.LogDebug("Connection already connected.  Exiting Connect");
                }
                else
                {
                    this.LogDebug("Attempting connect");

                    // Cancel reconnect if running.
					if (ReconnectTimer != null)
					{
						ReconnectTimer.Stop();
					}

                    // Cleanup the old client if it already exists
                    if (Client != null)
                    {
                        this.LogDebug("Cleaning up disconnected client");
                        KillClient(SocketStatus.SOCKET_STATUS_BROKEN_LOCALLY);
                    }

                    // This handles both password and keyboard-interactive (like on OS-X, 'nixes)
                    KeyboardInteractiveAuthenticationMethod kauth = new KeyboardInteractiveAuthenticationMethod(Username);
                    kauth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(kauth_AuthenticationPrompt);
                    PasswordAuthenticationMethod pauth = new PasswordAuthenticationMethod(Username, Password);

                    this.LogDebug("Creating new SshClient");
                    ConnectionInfo connectionInfo = new ConnectionInfo(Hostname, Port, Username, pauth, kauth);
                    Client = new SshClient(connectionInfo);
                    Client.ErrorOccurred += Client_ErrorOccurred;

                    //Attempt to connect
                    ClientStatus = SocketStatus.SOCKET_STATUS_WAITING;
                    try
                    {
                        Client.Connect();
                        TheStream = Client.CreateShellStream("PDTShell", 0, 0, 0, 0, 65534);
                        if (TheStream.DataAvailable)
                        {
                            // empty the buffer if there is data
                            string str = TheStream.Read();
                        }
                        TheStream.DataReceived += Stream_DataReceived;
                        this.LogInformation("Connected");
                        ClientStatus = SocketStatus.SOCKET_STATUS_CONNECTED;
                        DisconnectLogged = false;
                    }
                    catch (SshConnectionException e)
                    {
                        var ie = e.InnerException; // The details are inside!!
                        var errorLogLevel = DisconnectLogged == true ? Debug.ErrorLogLevel.None : Debug.ErrorLogLevel.Error;

                        if (ie is SocketException)
                        {
                            this.LogException(ie, "CONNECTION failure: Cannot reach host");                            
                        }

                        if (ie is System.Net.Sockets.SocketException socketException)
                        {
                            this.LogException(ie, "Connection failure: Cannot reach {host} on {port}",
                                Hostname, Port);
                        }
                        if (ie is SshAuthenticationException)
                        {
                            this.LogException(ie, "Authentication failure for username {userName}", Username);
                        }
                        else
                            this.LogException(ie, "Error on connect");

                        DisconnectLogged = true;
                        KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                        if (AutoReconnect)
                        {
                            this.LogDebug("Checking autoreconnect: {autoReconnect}, {autoReconnectInterval}ms", AutoReconnect, AutoReconnectIntervalMs);
                            ReconnectTimer.Reset(AutoReconnectIntervalMs);
                        }
                    }
                    catch(SshOperationTimeoutException ex)
                    {
                        this.LogWarning("Connection attempt timed out: {message}", ex.Message);

                        DisconnectLogged = true;
                        KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                        if (AutoReconnect)
                        {
                            this.LogDebug("Checking autoreconnect: {0}, {1}ms", AutoReconnect, AutoReconnectIntervalMs);
                            ReconnectTimer.Reset(AutoReconnectIntervalMs);
                        }
                    }
                    catch (Exception e)
                    {
                        var errorLogLevel = DisconnectLogged == true ? Debug.ErrorLogLevel.None : Debug.ErrorLogLevel.Error;
                        this.LogException(e, "Unhandled exception on connect");
                        DisconnectLogged = true;
                        KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                        if (AutoReconnect)
                        {
                            this.LogDebug("Checking autoreconnect: {0}, {1}ms", AutoReconnect, AutoReconnectIntervalMs);
                            ReconnectTimer.Reset(AutoReconnectIntervalMs);
                        }
                    }
                }
            }
            finally
            {
                connectLock.Release();
            }
        }

  /// <summary>
  /// Disconnect method
  /// </summary>
		public void Disconnect()
		{
			ConnectEnabled = false;
			// Stop trying reconnects, if we are
			if (ReconnectTimer != null)
			{
				ReconnectTimer.Stop();
				// ReconnectTimer = null;
			}

            KillClient(SocketStatus.SOCKET_STATUS_BROKEN_LOCALLY);
        }

        /// <summary>
        /// Kills the stream, cleans up the client and sets it to null
        /// </summary>
        private void KillClient(SocketStatus status)
        {
            KillStream();

            try
            {
                if (Client != null)
                {
                    Client.ErrorOccurred -= Client_ErrorOccurred;
                    Client.Disconnect();
                    Client.Dispose();
                    Client = null;
                    ClientStatus = status;
                    this.LogDebug("Disconnected");
                }
            }
            catch (Exception ex)
            {
               this.LogException(ex,"Exception in Kill Client");
            }
        }

        /// <summary>
        /// Kills the stream
        /// </summary>
		void KillStream()
		{
            try
            {
                if (TheStream != null)
                {
                    TheStream.DataReceived -= Stream_DataReceived;
                    TheStream.Close();
                    TheStream.Dispose();
                    TheStream = null;
                    this.LogDebug("Disconnected stream");
                }
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Exception in Kill Stream:{0}");
            }
		}

		/// <summary>
		/// Handles the keyboard interactive authentication, should it be required.
		/// </summary>
		void kauth_AuthenticationPrompt(object sender, AuthenticationPromptEventArgs e)
		{
			foreach (AuthenticationPrompt prompt in e.Prompts)
				if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
					prompt.Response = Password;
		}
	
		/// <summary>
		/// Handler for data receive on ShellStream.  Passes data across to queue for line parsing.
		/// </summary>
		void Stream_DataReceived(object sender, ShellDataEventArgs e)
		{
            if (((ShellStream)sender).Length <= 0L)
            {
                return;
            }
            var response = ((ShellStream)sender).Read(); 
 
			var bytesHandler = BytesReceived;
            
		    if (bytesHandler != null)
		    {
                var bytes = Encoding.UTF8.GetBytes(response);
		        if (StreamDebugging.RxStreamDebuggingIsEnabled)
		        {
		            this.LogInformation("Received {1} bytes: '{0}'", ComTextHelper.GetEscapedText(bytes), bytes.Length);
		        }
                bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
		    }
				
			var textHandler = TextReceived;
			if (textHandler != null)
			{
                if (StreamDebugging.RxStreamDebuggingIsEnabled)
                    this.LogInformation("Received: '{0}'", ComTextHelper.GetDebugText(response));

                textHandler(this, new GenericCommMethodReceiveTextArgs(response));
            }
			
		}


		/// <summary>
		/// Error event handler for client events - disconnect, etc.  Will forward those events via ConnectionChange
		/// event
		/// </summary>
		void Client_ErrorOccurred(object sender, ExceptionEventArgs e)
		{
            CrestronInvoke.BeginInvoke(o =>
            {
                if (e.Exception is SshConnectionException || e.Exception is System.Net.Sockets.SocketException)
                    this.LogError("Disconnected by remote");
                else
                    this.LogException(e.Exception, "Unhandled SSH client error");
                try
                {
                    connectLock.Wait();
                    KillClient(SocketStatus.SOCKET_STATUS_BROKEN_REMOTELY);
                }
                finally
                {
                    connectLock.Release();
                }
                if (AutoReconnect && ConnectEnabled)
                {
                    this.LogDebug("Checking autoreconnect: {0}, {1}ms", AutoReconnect, AutoReconnectIntervalMs);
                    ReconnectTimer.Reset(AutoReconnectIntervalMs);
                }
            });
        }

        /// <summary>
        /// Helper for ConnectionChange event
        /// </summary>
        void OnConnectionChange()
        {
            if (ConnectionChange != null)
                ConnectionChange(this, new GenericSocketStatusChageEventArgs(this));
        }

        #region IBasicCommunication Members

		/// <summary>
		/// Sends text to the server
		/// </summary>
		/// <param name="text"></param>
  /// <summary>
  /// SendText method
  /// </summary>
		public void SendText(string text)
		{
		    try
		    {
		        if (Client != null && TheStream != null && IsConnected)
		        {
		            if (StreamDebugging.TxStreamDebuggingIsEnabled)
		                this.LogInformation(
		                              "Sending {length} characters of text: '{text}'",
		                              text.Length,
		                              ComTextHelper.GetDebugText(text));

		            TheStream.Write(text);
		            TheStream.Flush();
		        }
		        else
		        {
		            this.LogDebug("Client is null or disconnected.  Cannot Send Text");
		        }
		    }
		    catch (ObjectDisposedException)
            {
                this.LogError("ObjectDisposedException sending '{message}'. Restarting connection...", text.Trim());                

                KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                ReconnectTimer.Reset();
		    }
			catch (Exception ex)
			{
                this.LogException(ex, "Exception sending text: '{message}'", text);
			}
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
            try
            {
                if (Client != null && TheStream != null && IsConnected)
                {
                    if (StreamDebugging.TxStreamDebuggingIsEnabled)
                        this.LogInformation("Sending {0} bytes: '{1}'", bytes.Length, ComTextHelper.GetEscapedText(bytes));

                    TheStream.Write(bytes, 0, bytes.Length);
                    TheStream.Flush();
                }
                else
                {
                    this.LogDebug("Client is null or disconnected.  Cannot Send Bytes");
                }
            }
            catch (ObjectDisposedException ex)
            {
                this.LogException(ex, "ObjectDisposedException sending {message}", ComTextHelper.GetEscapedText(bytes));

                KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                ReconnectTimer.Reset();
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Exception sending {message}", ComTextHelper.GetEscapedText(bytes));
            }           
		}
    #endregion

}

//*****************************************************************************************************
//*****************************************************************************************************
/// <summary>
/// Represents a SshConnectionChangeEventArgs
/// </summary>
public class SshConnectionChangeEventArgs : EventArgs
	{
        /// <summary>
        /// Connection State
        /// </summary>
		public bool IsConnected { get; private set; }

  /// <summary>
  /// Gets or sets the UIsConnected
  /// </summary>
		public ushort UIsConnected { get { return (ushort)(Client.IsConnected ? 1 : 0); } }

  /// <summary>
  /// Gets or sets the Client
  /// </summary>
		public GenericSshClient Client { get; private set; }

  /// <summary>
  /// Gets or sets the Status
  /// </summary>
		public ushort Status { get { return Client.UStatus; } }

        /// <summary>
        ///  S+ Constructor
        /// </summary>
        public SshConnectionChangeEventArgs() { }

        /// <summary>
        /// EventArgs class
        /// </summary>
        /// <param name="isConnected">Connection State</param>
        /// <param name="client">The Client</param>
		public SshConnectionChangeEventArgs(bool isConnected, GenericSshClient client)
        {
            IsConnected = isConnected;
            Client = client;
        }
    }
}
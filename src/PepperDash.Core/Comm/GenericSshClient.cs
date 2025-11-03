using System;
using System.Collections.Generic;
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
    /// SSH Client
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
            get { return client != null && ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED; }
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
            get { lock (_stateLock) { return _ClientStatus; } }
            private set
            {
                bool shouldFireEvent = false;
                lock (_stateLock)
                {
                    if (_ClientStatus != value)
                    {
                        _ClientStatus = value;
                        shouldFireEvent = true;
                    }
                }
                // Fire event outside lock to avoid deadlock
                if (shouldFireEvent)
                    OnConnectionChange();
            }
        }

        private SocketStatus _ClientStatus;
        private bool _ConnectEnabled;

        /// <summary>
        /// Contains the familiar Simpl analog status values. This drives the ConnectionChange event
        /// and IsConnected with be true when this == 2.
        /// </summary>
        public ushort UStatus
        {
            get { lock (_stateLock) { return (ushort)_ClientStatus; } }
        }

        /// <summary>
        /// Determines whether client will attempt reconnection on failure. Default is true
        /// </summary>
        public bool AutoReconnect { get; set; }

        /// <summary>
        /// Will be set and unset by connect and disconnect only
        /// </summary>
        public bool ConnectEnabled
        {
            get { lock (_stateLock) { return _ConnectEnabled; } }
            private set { lock (_stateLock) { _ConnectEnabled = value; } }
        }

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

        private SshClient client;

        private ShellStream shellStream;

        private readonly Timer reconnectTimer;

        //Lock object to prevent simulatneous connect/disconnect operations
        //private CCriticalSection connectLock = new CCriticalSection();
        private readonly SemaphoreSlim connectLock = new SemaphoreSlim(1);

        // Thread-safety lock for state changes
        private readonly object _stateLock = new object();

        private bool disconnectLogged = false;

        /// <summary>
        /// When true, turns off echo for the SSH session
        /// </summary>
        public bool DisableEcho { get; set; }

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

            reconnectTimer = new Timer(o =>
                {
                    if (ConnectEnabled) // Now thread-safe property access
                    {
                        Connect();
                    }
                }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// S+ Constructor - Must set all properties before calling Connect
        /// </summary>
        public GenericSshClient()
            : base(SPlusKey)
        {
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
            AutoReconnectIntervalMs = 5000;

            reconnectTimer = new Timer(o =>
            {
                if (ConnectEnabled) // Now thread-safe property access
                {
                    Connect();
                }
            }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Handles closing this up when the program shuts down
        /// </summary>
        private void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
            {
                if (client != null)
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
                    StopReconnectTimer();

                    // Cleanup the old client if it already exists
                    if (client != null)
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
                    client = new SshClient(connectionInfo);
                    client.ErrorOccurred += Client_ErrorOccurred;

                    //Attempt to connect
                    ClientStatus = SocketStatus.SOCKET_STATUS_WAITING;
                    try
                    {
                        client.Connect();

                        var modes = new Dictionary<TerminalModes, uint>();

                        if (DisableEcho)
                        {
                            modes.Add(TerminalModes.ECHO, 0);
                        }

                        shellStream = client.CreateShellStream("PDTShell", 0, 0, 0, 0, 65534, modes);
                        if (shellStream.DataAvailable)
                        {
                            // empty the buffer if there is data
                            shellStream.Read();
                        }
                        shellStream.DataReceived += Stream_DataReceived;
                        this.LogInformation("Connected");
                        ClientStatus = SocketStatus.SOCKET_STATUS_CONNECTED;
                        disconnectLogged = false;
                    }
                    catch (SshConnectionException e)
                    {
                        var ie = e.InnerException; // The details are inside!!

                        if (ie is SocketException)
                        {
                            this.LogError("CONNECTION failure: Cannot reach host");
                            this.LogVerbose(ie, "Exception details: ");
                        }

                        if (ie is System.Net.Sockets.SocketException socketException)
                        {
                            this.LogError("Connection failure: Cannot reach {host} on {port}",
                                Hostname, Port);
                            this.LogVerbose(socketException, "SocketException details: ");
                        }
                        if (ie is SshAuthenticationException)
                        {
                            this.LogError("Authentication failure for username {userName}", Username);
                            this.LogVerbose(ie, "AuthenticationException details: ");
                        }
                        else
                        {
                            this.LogError("Error on connect: {error}", ie.Message);
                            this.LogVerbose(ie, "Exception details: ");
                        }

                        disconnectLogged = true;
                        KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                        if (AutoReconnect)
                        {
                            this.LogDebug("Checking autoreconnect: {autoReconnect}, {autoReconnectInterval}ms", AutoReconnect, AutoReconnectIntervalMs);
                            StartReconnectTimer();
                        }
                    }
                    catch (SshOperationTimeoutException ex)
                    {
                        this.LogWarning("Connection attempt timed out: {message}", ex.Message);

                        disconnectLogged = true;
                        KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                        if (AutoReconnect)
                        {
                            this.LogDebug("Checking autoreconnect: {0}, {1}ms", AutoReconnect, AutoReconnectIntervalMs);
                            StartReconnectTimer();
                        }
                    }
                    catch (Exception e)
                    {
                        this.LogError("Unhandled exception on connect: {error}", e.Message);
                        this.LogVerbose(e, "Exception details: ");
                        disconnectLogged = true;
                        KillClient(SocketStatus.SOCKET_STATUS_CONNECT_FAILED);
                        if (AutoReconnect)
                        {
                            this.LogDebug("Checking autoreconnect: {0}, {1}ms", AutoReconnect, AutoReconnectIntervalMs);
                            StartReconnectTimer();
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
            StopReconnectTimer();

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
                if (client != null)
                {
                    client.ErrorOccurred -= Client_ErrorOccurred;
                    client.Disconnect();
                    client.Dispose();
                    client = null;
                    ClientStatus = status;
                    this.LogDebug("Disconnected");
                }
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Exception in Kill Client");
            }
        }

        /// <summary>
        /// Kills the stream
        /// </summary>
        private void KillStream()
        {
            try
            {
                if (shellStream != null)
                {
                    shellStream.DataReceived -= Stream_DataReceived;
                    shellStream.Close();
                    shellStream.Dispose();
                    shellStream = null;
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
        private void kauth_AuthenticationPrompt(object sender, AuthenticationPromptEventArgs e)
        {
            foreach (AuthenticationPrompt prompt in e.Prompts)
                if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                    prompt.Response = Password;
        }

        /// <summary>
        /// Handler for data receive on ShellStream.  Passes data across to queue for line parsing.
        /// </summary>
        private void Stream_DataReceived(object sender, ShellDataEventArgs e)
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
                this.PrintReceivedBytes(bytes);
                bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
            }

            var textHandler = TextReceived;
            if (textHandler != null)
            {
                this.PrintReceivedText(response);

                textHandler(this, new GenericCommMethodReceiveTextArgs(response));
            }

        }


        /// <summary>
        /// Error event handler for client events - disconnect, etc.  Will forward those events via ConnectionChange
        /// event
        /// </summary>
        private void Client_ErrorOccurred(object sender, ExceptionEventArgs e)
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
                    StartReconnectTimer();
                }
            });
        }

        /// <summary>
        /// Helper for ConnectionChange event
        /// </summary>
        private void OnConnectionChange()
        {
            ConnectionChange?.Invoke(this, new GenericSocketStatusChageEventArgs(this));
        }

        #region IBasicCommunication Members

        /// <summary>
        /// Sends text to the server
        /// </summary>
        /// <param name="text">The text to send</param>
        public void SendText(string text)
        {
            try
            {
                if (client != null && shellStream != null && IsConnected)
                {
                    this.PrintSentText(text);

                    shellStream.Write(text);
                    shellStream.Flush();
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
                StartReconnectTimer();
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Exception sending text: '{message}'", text);
            }
        }

        /// <summary>
        /// Sends Bytes to the server
        /// </summary>
        /// <param name="bytes">The bytes to send</param>
        public void SendBytes(byte[] bytes)
        {
            try
            {
                if (client != null && shellStream != null && IsConnected)
                {
                    this.PrintSentBytes(bytes);

                    shellStream.Write(bytes, 0, bytes.Length);
                    shellStream.Flush();
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
                StartReconnectTimer();
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Exception sending {message}", ComTextHelper.GetEscapedText(bytes));
            }
        }
        #endregion

        /// <summary>
        /// Safely starts the reconnect timer with exception handling
        /// </summary>
        private void StartReconnectTimer()
        {
            try
            {
                reconnectTimer?.Change(AutoReconnectIntervalMs, System.Threading.Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                // Timer was disposed, ignore
                this.LogDebug("Attempted to start timer but it was already disposed");
            }
        }

        /// <summary>
        /// Safely stops the reconnect timer with exception handling
        /// </summary>
        private void StopReconnectTimer()
        {
            try
            {
                reconnectTimer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                // Timer was disposed, ignore
                this.LogDebug("Attempted to stop timer but it was already disposed");
            }
        }

        /// <summary>
        /// Deactivate method - properly dispose of resources
        /// </summary>
        public override bool Deactivate()
        {
            try
            {
                this.LogDebug("Deactivating SSH client - disposing resources");

                // Stop trying reconnects
                ConnectEnabled = false;
                StopReconnectTimer();

                // Disconnect and cleanup client
                KillClient(SocketStatus.SOCKET_STATUS_BROKEN_LOCALLY);

                // Dispose timer
                try
                {
                    reconnectTimer?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }

                // Dispose semaphore
                try
                {
                    connectLock?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }

                return base.Deactivate();
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Error during SSH client deactivation");
                return false;
            }
        }

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
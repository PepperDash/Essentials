using Crestron.SimplSharp;
using Org.BouncyCastle.Asn1.X509;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System;
using System.IO;
using System.Security.Authentication;
using WebSocketSharp;
using WebSocketSharp.Server;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace PepperDash.Core
{
    /// <summary>
    /// Provides a WebSocket-based logging sink for debugging purposes, allowing log events to be broadcast to connected
    /// WebSocket clients.
    /// </summary>
    /// <remarks>This class implements the <see cref="ILogEventSink"/> interface and is designed to send
    /// formatted log events to WebSocket clients connected to a secure WebSocket server. The server is hosted locally
    /// and uses a self-signed certificate for SSL/TLS encryption.</remarks>
    public class DebugWebsocketSink : ILogEventSink, IKeyed
    {
        private HttpServer _httpsServer;
        
        private string _path = "/debug/join/";
        private const string _certificateName = "selfCres";
        private const string _certificatePassword = "cres12345";

        /// <summary>
        /// Gets the port number on which the HTTPS server is currently running.
        /// </summary>
        public int Port 
        { get 
            { 
                
                if(_httpsServer == null) return 0;
                return _httpsServer.Port;
            } 
        }

        /// <summary>
        /// Gets the WebSocket URL for the current server instance.
        /// </summary>
        /// <remarks>The URL is dynamically constructed based on the server's current IP address, port,
        /// and WebSocket path.</remarks>
        public string Url
        {
            get
            {
                if (_httpsServer == null) return "";
                return $"wss://{CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0)}:{_httpsServer.Port}{_httpsServer.WebSocketServices[_path].Path}";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the HTTPS server is currently listening for incoming connections.
        /// </summary>
        public bool IsRunning { get => _httpsServer?.IsListening ?? false; }        

        /// <inheritdoc/>
        public string Key => "DebugWebsocketSink";

        private readonly ITextFormatter _textFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugWebsocketSink"/> class with the specified text formatter.
        /// </summary>
        /// <remarks>This constructor initializes the WebSocket sink and ensures that a certificate is
        /// available for secure communication. If the required certificate does not exist, it will be created
        /// automatically. Additionally, the sink is configured to stop the server when the program is
        /// stopping.</remarks>
        /// <param name="formatProvider">The text formatter used to format log messages. If null, a default JSON formatter is used.</param>
        public DebugWebsocketSink(ITextFormatter formatProvider)
        {

            _textFormatter = formatProvider ?? new JsonFormatter();

            if (!File.Exists($"\\user\\{_certificateName}.pfx"))
                CreateCert();

            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type == eProgramStatusEventType.Stopping)
                {
                    StopServer();
                }
            };
        }

        private static void CreateCert()
        {
            try
            {
                //Debug.Console(0,"CreateCert Creating Utility");
                CrestronConsole.PrintLine("CreateCert Creating Utility");
                //var utility = new CertificateUtility();
                var utility = new BouncyCertificate();
                //Debug.Console(0, "CreateCert Calling CreateCert");
                CrestronConsole.PrintLine("CreateCert Calling CreateCert");
                //utility.CreateCert();
                var ipAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);
                var hostName = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);
                var domainName = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, 0);

                //Debug.Console(0, "DomainName: {0} | HostName: {1} | {1}.{0}@{2}", domainName, hostName, ipAddress);
                CrestronConsole.PrintLine(string.Format("DomainName: {0} | HostName: {1} | {1}.{0}@{2}", domainName, hostName, ipAddress));

                var certificate = utility.CreateSelfSignedCertificate(string.Format("CN={0}.{1}", hostName, domainName), new[] { string.Format("{0}.{1}", hostName, domainName), ipAddress }, new[] { KeyPurposeID.id_kp_serverAuth, KeyPurposeID.id_kp_clientAuth });
                //Crestron fails to let us do this...perhaps it should be done through their Dll's but haven't tested
                //Debug.Print($"CreateCert Storing Certificate To My.LocalMachine");
                //utility.AddCertToStore(certificate, StoreName.My, StoreLocation.LocalMachine);
                //Debug.Console(0, "CreateCert Saving Cert to \\user\\");
                CrestronConsole.PrintLine("CreateCert Saving Cert to \\user\\");
                utility.CertificatePassword = _certificatePassword;
                utility.WriteCertificate(certificate, @"\user\", _certificateName);
                //Debug.Console(0, "CreateCert Ending CreateCert");
                CrestronConsole.PrintLine("CreateCert Ending CreateCert");
            }
            catch (Exception ex)
            {
                //Debug.Console(0, "WSS CreateCert Failed\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
                CrestronConsole.PrintLine(string.Format("WSS CreateCert Failed\r\n{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// Sends a log event to all connected WebSocket clients.
        /// </summary>
        /// <remarks>The log event is formatted using the configured text formatter and then broadcasted  
        /// to all clients connected to the WebSocket server. If the WebSocket server is not          initialized or not
        /// listening, the method exits without performing any action.</remarks>
        /// <param name="logEvent">The log event to be formatted and broadcasted. Cannot be null.</param>
        public void Emit(LogEvent logEvent)
        {
            if (_httpsServer == null || !_httpsServer.IsListening) return;

            var sw = new StringWriter();
            _textFormatter.Format(logEvent, sw);

            _httpsServer.WebSocketServices[_path].Sessions.Broadcast(sw.ToString());           
        }

        /// <summary>
        /// Starts the WebSocket server on the specified port and configures it with the appropriate certificate.
        /// </summary>
        /// <remarks>This method initializes the WebSocket server and binds it to the specified port.  It
        /// also applies the server's certificate for secure communication. Ensure that the port is not already in use
        /// and that the certificate file is accessible.</remarks>
        /// <param name="port">The port number on which the WebSocket server will listen. Must be a valid, non-negative port number.</param>
        public void StartServerAndSetPort(int port)
        {
            Debug.Console(0, "Starting Websocket Server on port: {0}", port);


            Start(port, $"\\user\\{_certificateName}.pfx", _certificatePassword);
        }

        private void Start(int port, string certPath = "", string certPassword = "")
        {
            try
            {
                _httpsServer = new HttpServer(port, true);                

                if (!string.IsNullOrWhiteSpace(certPath))
                {
                    Debug.Console(0, "Assigning SSL Configuration");

                    _httpsServer.SslConfiguration.ServerCertificate = new X509Certificate2(certPath, certPassword);
                    _httpsServer.SslConfiguration.ClientCertificateRequired = false;
                    _httpsServer.SslConfiguration.CheckCertificateRevocation = false;
                    _httpsServer.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                    //this is just to test, you might want to actually validate
                    _httpsServer.SslConfiguration.ClientCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            Debug.Console(0, "HTTPS ClientCerticateValidation Callback triggered");
                            return true;
                        };
                }
                Debug.Console(0, "Adding Debug Client Service");
                _httpsServer.AddWebSocketService<DebugClient>(_path);
                Debug.Console(0, "Assigning Log Info");
                _httpsServer.Log.Level = LogLevel.Trace;
                _httpsServer.Log.Output = (d, s) =>
                {
                    uint level;

                    switch(d.Level)
                    {
                        case WebSocketSharp.LogLevel.Fatal:
                            level = 3;
                            break;
                        case WebSocketSharp.LogLevel.Error:
                            level = 2;
                            break;
                        case WebSocketSharp.LogLevel.Warn:
                            level = 1;
                            break;
                        case WebSocketSharp.LogLevel.Info:
                            level = 0;
                            break;
                        case WebSocketSharp.LogLevel.Debug:
                            level = 4;
                            break;
                        case WebSocketSharp.LogLevel.Trace:
                            level = 5;
                            break;
                        default:
                            level = 4;
                            break;
                    }
                    
                    Debug.Console(level, "{1} {0}\rCaller:{2}\rMessage:{3}\rs:{4}", d.Level.ToString(), d.Date.ToString(), d.Caller.ToString(), d.Message, s);
                };
                Debug.Console(0, "Starting");

                _httpsServer.Start();
                Debug.Console(0, "Ready");
            }
            catch (Exception ex)
            {
                Debug.Console(0, "WebSocket Failed to start {0}", ex.Message);
            }
        }

        /// <summary>
        /// Stops the WebSocket server if it is currently running.
        /// </summary>
        /// <remarks>This method halts the WebSocket server and releases any associated resources.  After
        /// calling this method, the server will no longer accept or process incoming connections.</remarks>
        public void StopServer()
        {
            Debug.Console(0, "Stopping Websocket Server");
            _httpsServer?.Stop();

            _httpsServer = null;
        }
    }

    /// <summary>
    /// Configures the logger to write log events to a debug WebSocket sink.
    /// </summary>
    /// <remarks>This extension method allows you to direct log events to a WebSocket sink for debugging
    /// purposes.</remarks>
    public static class DebugWebsocketSinkExtensions
    {
        /// <summary>
        /// Configures a logger to write log events to a debug WebSocket sink.
        /// </summary>
        /// <remarks>This method adds a sink that writes log events to a WebSocket for debugging purposes.
        /// It is typically used during development to stream log events in real-time.</remarks>
        /// <param name="loggerConfiguration">The logger sink configuration to apply the WebSocket sink to.</param>
        /// <param name="formatProvider">An optional text formatter to format the log events. If not provided, a default formatter will be used.</param>
        /// <returns>A <see cref="LoggerConfiguration"/> object that can be used to further configure the logger.</returns>
        public static LoggerConfiguration DebugWebsocketSink(
                             this LoggerSinkConfiguration loggerConfiguration,
                                              ITextFormatter formatProvider = null)
        {
            return loggerConfiguration.Sink(new DebugWebsocketSink(formatProvider));
        }
    }

    /// <summary>
    /// Represents a WebSocket client for debugging purposes, providing connection lifecycle management and message
    /// handling functionality.
    /// </summary>
    /// <remarks>The <see cref="DebugClient"/> class extends <see cref="WebSocketBehavior"/> to handle
    /// WebSocket connections, including events for opening, closing, receiving messages, and errors. It tracks the
    /// duration of the connection and logs relevant events for debugging.</remarks>
    public class DebugClient : WebSocketBehavior
    {
        private DateTime _connectionTime;

        /// <summary>
        /// Gets the duration of time the WebSocket connection has been active.
        /// </summary>
        public TimeSpan ConnectedDuration
        {
            get
            {
                if (Context.WebSocket.IsAlive)
                {
                    return DateTime.Now - _connectionTime;
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugClient"/> class.
        /// </summary>
        /// <remarks>This constructor creates a new <see cref="DebugClient"/> instance and logs its
        /// creation using the <see cref="Debug.Console(int, string)"/> method with a debug level of 0.</remarks>
        public DebugClient()
        {
            Debug.Console(0, "DebugClient Created");
        }

        /// <inheritdoc/>
        protected override void OnOpen()
        {
            base.OnOpen();

            var url = Context.WebSocket.Url;
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "New WebSocket Connection from: {0}", url);

            _connectionTime = DateTime.Now;
        }

        /// <inheritdoc/>
        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            Debug.Console(0, "WebSocket UiClient Message: {0}", e.Data);
        }

        /// <inheritdoc/>
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "WebSocket UiClient Closing: {0} reason: {1}", e.Code, e.Reason);

        }

        /// <inheritdoc/>
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);

            Debug.Console(2, Debug.ErrorLogLevel.Notice, "WebSocket UiClient Error: {0} message: {1}", e.Exception, e.Message);
        }
    }
}

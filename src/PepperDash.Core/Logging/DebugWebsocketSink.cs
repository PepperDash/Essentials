using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using WebSocketSharp.Server;
using Crestron.SimplSharp;
using WebSocketSharp;
using System.Security.Authentication;
using WebSocketSharp.Net;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
using System.IO;
using Org.BouncyCastle.Asn1.X509;
using Serilog.Formatting;
using Newtonsoft.Json.Linq;
using Serilog.Formatting.Json;

namespace PepperDash.Core
{
    /// <summary>
    /// Represents a DebugWebsocketSink
    /// </summary>
    public class DebugWebsocketSink : ILogEventSink
    {
        private HttpServer _httpsServer;

        private string _path = "/debug/join/";
        private const string _certificateName = "selfCres";
        private const string _certificatePassword = "cres12345";

        public int Port
        {
            get
            {

                if (_httpsServer == null) return 0;
                return _httpsServer.Port;
            }
        }

        public string Url
        {
            get
            {
                if (_httpsServer == null) return "";
                return $"wss://{CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0)}:{_httpsServer.Port}{_httpsServer.WebSocketServices[_path].Path}";
            }
        }

        /// <summary>
        /// Gets or sets the IsRunning
        /// </summary>
        public bool IsRunning { get => _httpsServer?.IsListening ?? false; }


        private readonly ITextFormatter _textFormatter;

        public DebugWebsocketSink(ITextFormatter formatProvider)
        {

            _textFormatter = formatProvider ?? new JsonFormatter();

            if (!File.Exists($"\\user\\{_certificateName}.pfx"))
                CreateCert(null);

            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type == eProgramStatusEventType.Stopping)
                {
                    StopServer();
                }
            };
        }

        private void CreateCert(string[] args)
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
        /// Emit method
        /// </summary>
        public void Emit(LogEvent logEvent)
        {
            if (_httpsServer == null || !_httpsServer.IsListening) return;

            var sw = new StringWriter();
            _textFormatter.Format(logEvent, sw);

            _httpsServer.WebSocketServices.Broadcast(sw.ToString());

        }

        /// <summary>
        /// StartServerAndSetPort method
        /// </summary>
        public void StartServerAndSetPort(int port)
        {
            Debug.LogInformation("Starting Websocket Server on port: {port}", port);


            Start(port, $"\\user\\{_certificateName}.pfx", _certificatePassword);
        }

        private void Start(int port, string certPath = "", string certPassword = "")
        {
            try
            {
                _httpsServer = new HttpServer(port, true);


                if (!string.IsNullOrWhiteSpace(certPath))
                {
                    Debug.LogInformation("Assigning SSL Configuration");
                    _httpsServer.SslConfiguration = new ServerSslConfiguration(new X509Certificate2(certPath, certPassword))
                    {
                        ClientCertificateRequired = false,
                        CheckCertificateRevocation = false,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                        //this is just to test, you might want to actually validate
                        ClientCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            Debug.LogInformation("HTTPS ClientCerticateValidation Callback triggered");
                            return true;
                        }
                    };
                }
                Debug.LogInformation("Adding Debug Client Service");
                _httpsServer.AddWebSocketService<DebugClient>(_path);
                Debug.LogInformation("Assigning Log Info");
                _httpsServer.Log.Level = LogLevel.Trace;
                _httpsServer.Log.Output = WriteWebSocketInternalLog;
                Debug.LogInformation("Starting");

                _httpsServer.Start();
                Debug.LogInformation("Ready");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex, "WebSocket Failed to start: {message}", ex.Message);
            }
        }

        /// <summary>
        /// StopServer method
        /// </summary>
        public void StopServer()
        {
            Debug.LogInformation("Stopping Websocket Server");

            try
            {
                if (_httpsServer == null || !_httpsServer.IsListening)
                {
                    return;
                }

                // Prevent close-sequence internal websocket logs from re-entering the logging pipeline.
                _httpsServer.Log.Output = (d, s) => { };

                var serviceHost = _httpsServer.WebSocketServices[_path];

                if (serviceHost == null)
                {
                    _httpsServer.Stop();
                    _httpsServer = null;
                    return;
                }

                serviceHost.Sessions.Broadcast("Server is stopping");

                foreach (var session in serviceHost.Sessions.Sessions)
                {
                    if (session?.Context?.WebSocket != null && session.Context.WebSocket.IsAlive)
                    {
                        session.Context.WebSocket.Close(1001, "Server is stopping");
                    }
                }

                _httpsServer.Stop();

                _httpsServer = null;

            }
            catch (Exception ex)
            {
                Debug.LogError(ex, "WebSocket Failed to stop gracefully {0}", ex.Message);
                Debug.LogVerbose("Stack Trace\r\n{0}", ex.StackTrace);
            }
        }

        private static void WriteWebSocketInternalLog(LogData data, string supplemental)
        {
            try
            {
                if (data == null)
                {
                    return;
                }

                var message = string.IsNullOrWhiteSpace(data.Message) ? "<none>" : data.Message;
                var details = string.IsNullOrWhiteSpace(supplemental) ? string.Empty : string.Format(" | details: {0}", supplemental);

                // Use direct console output to avoid recursive log sink calls.
                CrestronConsole.PrintLine(string.Format("WS[{0}] {1} | message: {2}{3}", data.Level, data.Date, message, details));
            }
            catch
            {
                // Never throw from websocket log callback.
            }
        }
    }

    public static class DebugWebsocketSinkExtensions
    {
        /// <summary>
        /// DebugWebsocketSink method
        /// </summary>
        public static LoggerConfiguration DebugWebsocketSink(
                             this LoggerSinkConfiguration loggerConfiguration,
                                              ITextFormatter formatProvider = null)
        {
            return loggerConfiguration.Sink(new DebugWebsocketSink(formatProvider));
        }
    }

    /// <summary>
    /// Represents a DebugClient
    /// </summary>
    public class DebugClient : WebSocketBehavior
    {
        private DateTime _connectionTime;

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

        public DebugClient()
        {
            Debug.LogInformation("DebugClient Created");
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            var url = Context.WebSocket.Url;
            Debug.LogInformation("New WebSocket Connection from: {url}", url);

            _connectionTime = DateTime.Now;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            Debug.LogInformation("WebSocket UiClient Message: {data}", e.Data);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            Debug.LogInformation("WebSocket UiClient Closing: {code} reason: {reason}", e.Code, e.Reason);

        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);

            Debug.LogError(e.Exception, "WebSocket UiClient Error: {message}", e.Message);
        }
    }
}

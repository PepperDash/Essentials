using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using WebSocketSharp.Server;
using Crestron.SimplSharp;
using WebSocketSharp;
using System.Security.Authentication;
using WebSocketSharp.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Serilog.Formatting;
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

        private static string CertPath =>
    $"{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}{_certificateName}.pfx";


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
                if (_httpsServer == null || !_httpsServer.IsListening) return "";
                var service = _httpsServer.WebSocketServices[_path];
                if (service == null) return "";

                // Use CSLAN IP if available, otherwise fallback to primary IP. This ensures we provide a reachable URL in dual-stack environments.
                var cslanIp = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 1);
                if (!string.IsNullOrEmpty(cslanIp) && cslanIp != "Invalid Value")
                    return $"wss://{cslanIp}:{_httpsServer.Port}{service.Path}";
                else
                    return $"wss://{CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0)}:{_httpsServer.Port}{service.Path}";
            }
        }

        /// <summary>
        /// Gets or sets the IsRunning
        /// </summary>
        public bool IsRunning { get => _httpsServer?.IsListening ?? false; }


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

            if (!File.Exists(CertPath))
                CreateCert();

            try
            {
                CrestronEnvironment.ProgramStatusEventHandler += type =>
                {
                    if (type == eProgramStatusEventType.Stopping)
                        StopServer();
                };
            }
            catch
            {
                // CrestronEnvironment is not available in test / dev environments — safe to skip.
            }
        }

        private static void CreateCert()
        {
            // NOTE: This method is called from the constructor, which is itself called during Debug's static
            // constructor before _logger is assigned. Do NOT call any Debug.Log* methods here — use
            // CrestronConsole.PrintLine only, to avoid a NullReferenceException that would poison the Debug type.
            try
            {
                var ipAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);
                var hostName = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);
                var domainName = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, 0);

                CrestronConsole.PrintLine(string.Format("CreateCert: DomainName: {0} | HostName: {1} | {1}.{0}@{2}", domainName, hostName, ipAddress));

                var subjectName = string.Format("CN={0}.{1}", hostName, domainName);
                var fqdn = string.Format("{0}.{1}", hostName, domainName);

                var random = new SecureRandom();

                // Generate RSA 2048 key pair
                var keyPairGenerator = new RsaKeyPairGenerator();
                keyPairGenerator.Init(new KeyGenerationParameters(random, 2048));
                var keyPair = keyPairGenerator.GenerateKeyPair();

                // Build certificate
                var certGenerator = new X509V3CertificateGenerator();
                certGenerator.SetSerialNumber(BigInteger.ValueOf(Math.Abs(DateTime.UtcNow.Ticks)));
                certGenerator.SetIssuerDN(new X509Name(subjectName));
                certGenerator.SetSubjectDN(new X509Name(subjectName));
                certGenerator.SetNotBefore(DateTime.UtcNow);
                certGenerator.SetNotAfter(DateTime.UtcNow.AddYears(2));
                certGenerator.SetPublicKey(keyPair.Public);

                // Extended Key Usage: server + client auth
                certGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false,
                    new ExtendedKeyUsage(new[] { KeyPurposeID.id_kp_serverAuth, KeyPurposeID.id_kp_clientAuth }));

                // Subject Alternative Names: DNS + IP
                System.Net.IPAddress parsedIp;
                if (System.Net.IPAddress.TryParse(ipAddress, out parsedIp))
                {
                    certGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false,
                        new GeneralNames(new GeneralName[] {
                            new GeneralName(GeneralName.DnsName, fqdn),
                            new GeneralName(GeneralName.IPAddress, ipAddress)
                        }));
                }
                else
                {
                    certGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false,
                        new GeneralNames(new GeneralName(GeneralName.DnsName, fqdn)));
                }

                // Sign with SHA256withRSA
                var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private, random);
                var certificate = certGenerator.Generate(signatureFactory);

                // Export as PKCS12/PFX
                var pkcs12Store = new Pkcs12StoreBuilder().Build();
                var certEntry = new X509CertificateEntry(certificate);
                pkcs12Store.SetCertificateEntry(_certificateName, certEntry);
                pkcs12Store.SetKeyEntry(_certificateName, new AsymmetricKeyEntry(keyPair.Private), new[] { certEntry });

                var separator = Path.DirectorySeparatorChar;
                var outputPath = string.Format("{0}user{1}{2}.pfx", separator, separator, _certificateName);

                using (var ms = new MemoryStream())
                {
                    pkcs12Store.Save(ms, _certificatePassword.ToCharArray(), random);
                    File.WriteAllBytes(outputPath, ms.ToArray());
                }

                CrestronConsole.PrintLine(string.Format("CreateCert: Certificate written to {0}", outputPath));
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine(string.Format("WSS CreateCert Failed: {0}\r\n{1}", ex.Message, ex.StackTrace));
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
        /// Starts the WebSocket server on the specified port and configures it with the appropriate certificate.
        /// </summary>
        /// <remarks>This method initializes the WebSocket server and binds it to the specified port.  It
        /// also applies the server's certificate for secure communication. Ensure that the port is not already in use
        /// and that the certificate file is accessible.</remarks>
        /// <param name="port">The port number on which the WebSocket server will listen. Must be a valid, non-negative port number.</param>
        public void StartServerAndSetPort(int port)
        {
            Debug.LogInformation("Starting Websocket Server on port: {0}", port);


            Start(port, CertPath, _certificatePassword);
        }

        private static X509Certificate2 LoadOrRecreateCert(string certPath, string certPassword)
        {
            try
            {
                // EphemeralKeySet is required on Linux/OpenSSL (Crestron 4-series) to avoid
                // key-container persistence failures, and avoids the private key export restriction.
                return new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.EphemeralKeySet);
            }
            catch (Exception ex)
            {
                // Cert is stale or was generated by an incompatible library (e.g. old BouncyCastle output).
                // Delete it, regenerate with the BCL path, and retry once.
                CrestronConsole.PrintLine(string.Format("SSL cert load failed ({0}); regenerating...", ex.Message));
                try { File.Delete(certPath); } catch { }
                CreateCert();
                return new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.EphemeralKeySet);
            }
        }

        private void Start(int port, string certPath = "", string certPassword = "")
        {
            try
            {
                _httpsServer = new HttpServer(port, true);

                if (!string.IsNullOrWhiteSpace(certPath))
                {
                    Debug.LogInformation("Assigning SSL Configuration");

                    _httpsServer.SslConfiguration.ServerCertificate = LoadOrRecreateCert(certPath, certPassword);
                    _httpsServer.SslConfiguration.ClientCertificateRequired = false;
                    _httpsServer.SslConfiguration.CheckCertificateRevocation = false;
                    _httpsServer.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                    //this is just to test, you might want to actually validate
                    _httpsServer.SslConfiguration.ClientCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            Debug.LogInformation("HTTPS ClientCerticateValidation Callback triggered");
                            return true;
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
                Debug.LogError(ex, "WebSocket Failed to start {0}", ex.Message);
                Debug.LogVerbose("Stack Trace:\r{0}", ex.StackTrace);
                // Null out the server so callers can detect failure via IsRunning / Url null guards.
                _httpsServer = null;
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
            Debug.Console(0, "DebugClient Created");
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            var url = Context.WebSocket.Url;
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "New WebSocket Connection from: {0}", url);

            _connectionTime = DateTime.Now;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            Debug.Console(0, "WebSocket UiClient Message: {0}", e.Data);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "WebSocket UiClient Closing: {0} reason: {1}", e.Code, e.Reason);

        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);

            Debug.Console(2, Debug.ErrorLogLevel.Notice, "WebSocket UiClient Error: {0} message: {1}", e.Exception, e.Message);
        }
    }
}

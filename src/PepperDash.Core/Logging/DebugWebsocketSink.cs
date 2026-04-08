extern alias NewtonsoftJson;

using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Crestron.SimplSharp;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using JObject = NewtonsoftJson::Newtonsoft.Json.Linq.JObject;
using Serilog.Formatting.Json;
using System.IO;
using WebSocketSharp;
using WebSocketSharp.Server;
using WebSocketSharp.Net;

namespace PepperDash.Core;

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

    private readonly string _path = "/debug/join/";
    private const string _certificateName = "selfCres";
    private const string _certificatePassword = "cres12345";
    private static string CertPath =>
        $"{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}{_certificateName}.pfx";

    /// <summary>
    /// Gets the port number on which the HTTPS server is currently running.
    /// </summary>
    public int Port
    {
        get
        {

            if (_httpsServer == null) return 0;
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
            if (_httpsServer == null || !_httpsServer.IsListening) return "";
            var service = _httpsServer.WebSocketServices[_path];
            if (service == null) return "";
            return $"wss://{CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0)}:{_httpsServer.Port}{service.Path}";
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

            using var rsa = RSA.Create(2048);

            var request = new CertificateRequest(
                subjectName,
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // Subject Key Identifier
            request.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

            // Extended Key Usage: server + client auth
            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection
                    {
                        new Oid("1.3.6.1.5.5.7.3.1"), // id-kp-serverAuth
                        new Oid("1.3.6.1.5.5.7.3.2")  // id-kp-clientAuth
                    },
                    false));

            // Subject Alternative Names: DNS + IP
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName(fqdn);
            if (System.Net.IPAddress.TryParse(ipAddress, out var ip))
                sanBuilder.AddIpAddress(ip);
            request.CertificateExtensions.Add(sanBuilder.Build());

            var notBefore = DateTimeOffset.UtcNow;
            var notAfter = notBefore.AddYears(2);

            using var cert = request.CreateSelfSigned(notBefore, notAfter);

            var separator = Path.DirectorySeparatorChar;
            var outputPath = string.Format("{0}user{1}{2}.pfx", separator, separator, _certificateName);

            var pfxBytes = cert.Export(X509ContentType.Pfx, _certificatePassword);
            File.WriteAllBytes(outputPath, pfxBytes);

            CrestronConsole.PrintLine(string.Format("CreateCert: Certificate written to {0}", outputPath));
        }
        catch (Exception ex)
        {
            CrestronConsole.PrintLine(string.Format("WSS CreateCert Failed: {0}\r\n{1}", ex.Message, ex.StackTrace));
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
    /// Stops the WebSocket server if it is currently running.
    /// </summary>
    /// <remarks>This method halts the WebSocket server and releases any associated resources.  After
    /// calling this method, the server will no longer accept or process incoming connections.</remarks>
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
    public DebugClient()
    {
        Debug.LogInformation("DebugClient Created");
    }

    /// <inheritdoc/>
    protected override void OnOpen()
    {
        base.OnOpen();

        var url = Context.WebSocket.Url;
        Debug.LogInformation("New WebSocket Connection from: {0}", url);

        _connectionTime = DateTime.Now;
    }

    /// <inheritdoc/>
    protected override void OnMessage(MessageEventArgs e)
    {
        base.OnMessage(e);

        Debug.LogVerbose("WebSocket UiClient Message: {0}", e.Data);
    }

    /// <inheritdoc/>
    protected override void OnClose(CloseEventArgs e)
    {
        base.OnClose(e);

        Debug.LogDebug("WebSocket UiClient Closing: {0} reason: {1}", e.Code, e.Reason);
    }

    /// <inheritdoc/>
    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        base.OnError(e);

        Debug.LogError(e.Exception, "WebSocket UiClient Error: {0} message: {1}", e.Exception, e.Message);
        Debug.LogVerbose("Stack Trace:\r{0}", e.Exception.StackTrace);
    }
}

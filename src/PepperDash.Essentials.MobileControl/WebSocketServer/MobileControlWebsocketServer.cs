using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Prng;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Web;
using PepperDash.Essentials.RoomBridges;
using PepperDash.Essentials.WebApiHandlers;
using Serilog.Events;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;


namespace PepperDash.Essentials.WebSocketServer
{
    /// <summary>
    /// Represents a MobileControlWebsocketServer
    /// </summary>
    public class MobileControlWebsocketServer : EssentialsDevice
    {
        private readonly string userAppPath = Global.FilePathPrefix + "mcUserApp" + Global.DirectorySeparator;

        private readonly string localConfigFolderName = "_local-config";

        private readonly string appConfigFileName = "_config.local.json";
        private readonly string appConfigCsFileName = "_config.cs.json";

        private const string certificateName = "selfCres";

        private const string certificatePassword = "cres12345";

        /// <summary>
        /// Where the key is the join token and the value is the room key
        /// </summary>
        //private Dictionary<string, JoinToken> _joinTokens;

        private HttpServer _server;

        /// <summary>
        /// Gets the HttpServer instance
        /// </summary>
        public HttpServer Server => _server;

        /// <summary>
        /// Gets the collection of UI client contexts
        /// </summary>
        public Dictionary<string, UiClientContext> UiClientContexts { get; private set; }

        private readonly ConcurrentDictionary<string, UiClient> uiClients = new ConcurrentDictionary<string, UiClient>();

        /// <summary>
        /// Stores pending client registrations using composite key: token-clientId
        /// This ensures the correct client ID is matched even when connections establish out of order
        /// </summary>
        private readonly ConcurrentDictionary<string, string> pendingClientRegistrations = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Stores pending client registrations with timestamp for legacy clients
        /// Key is token, Value is list of (clientId, timestamp) tuples
        /// Most recent registration is used to handle duplicate join requests
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentBag<(string clientId, DateTime timestamp)>> legacyClientRegistrations = new ConcurrentDictionary<string, ConcurrentBag<(string, DateTime)>>();

        /// <summary>
        /// Gets the collection of UI clients
        /// </summary>
        public IReadOnlyDictionary<string, UiClient> UiClients => uiClients;

        private readonly MobileControlSystemController _parent;

        private WebSocketServerSecretProvider _secretProvider;

        private ServerTokenSecrets _secret;

        private static readonly HttpClient LogClient = new HttpClient();

        private string SecretProviderKey
        {
            get
            {
                return string.Format("{0}:{1}-tokens", Global.ControlSystem.ProgramNumber, Key);
            }
        }

        private string LanIpAddress => CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter));

        private readonly System.Net.IPAddress csIpAddress;

        private readonly System.Net.IPAddress csSubnetMask;

        /// <summary>
        /// The path for the WebSocket messaging
        /// </summary>
        private readonly string _wsPath = "/mc/api/ui/join/";

        /// <summary>
        /// Gets the WebSocket path
        /// </summary>
        public string WsPath => _wsPath;

        /// <summary>
        /// The path to the location of the files for the user app (single page Angular app)
        /// </summary>
        private readonly string _appPath = string.Format("{0}mcUserApp", Global.FilePathPrefix);

        /// <summary>
        /// The base HREF that the user app uses
        /// </summary>
        private string _userAppBaseHref = "/mc/app";

        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the HTTP scheme to use for generated URLs, based on whether the direct server is configured as secure
        /// </summary>
        private string HttpScheme => _parent.Config.DirectServer.Secure ? "https" : "http";

        /// <summary>
        /// Gets the WebSocket scheme to use for generated URLs, based on whether the direct server is configured as secure
        /// </summary>
        private string WsScheme => _parent.Config.DirectServer.Secure ? "wss" : "ws";

        /// <summary>
        /// Gets the user app URL prefix
        /// </summary>
        public string UserAppUrlPrefix
        {
            get
            {
                return string.Format("{0}://{1}:{2}{3}?token=",
                    HttpScheme,
                    CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0),
                    Port,
                    _userAppBaseHref);

            }
        }

        /// <summary>
        /// Gets the count of connected UI clients
        /// </summary>
        public int ConnectedUiClientsCount
        {
            get
            {
                return uiClients.Values.Where(c => c.Context.WebSocket.IsAlive).Count();
            }
        }

        /// <summary>
        /// Initializes a new instance of the MobileControlWebsocketServer class.
        /// </summary>
        public MobileControlWebsocketServer(string key, int customPort, MobileControlSystemController parent)
            : base(key)
        {
            _parent = parent;

            // Set the default port to be 50000 plus the slot number of the program
            Port = 50000 + (int)Global.ControlSystem.ProgramNumber;

            if (customPort != 0)
            {
                Port = customPort;
            }

            if (parent.Config.DirectServer.AutomaticallyForwardPortToCSLAN == true)
            {
                try
                {
                    this.LogInformation("Automatically forwarding port {port} to CS LAN", Port);

                    var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
                    var csIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                    var result = CrestronEthernetHelper.AddPortForwarding((ushort)Port, (ushort)Port, csIp, CrestronEthernetHelper.ePortMapTransport.TCP);

                    if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                    {
                        this.LogError("Error adding port forwarding: {error}", result);
                    }
                }
                catch (ArgumentException)
                {
                    this.LogInformation("This processor does not have a CS LAN", this);
                }
                catch (Exception ex)
                {
                    this.LogError("Error automatically forwarding port to CS LAN: {message}", ex.Message);
                    this.LogDebug(ex, "Stack Trace");
                }
            }

            try
            {
                var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
                var csSubnetMask = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, csAdapterId);
                var csIpAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                this.csSubnetMask = System.Net.IPAddress.Parse(csSubnetMask);
                this.csIpAddress = System.Net.IPAddress.Parse(csIpAddress);
            }
            catch (ArgumentException)
            {
                if (parent.Config.DirectServer.AutomaticallyForwardPortToCSLAN == false)
                {
                    this.LogInformation("This processor does not have a CS LAN");
                }
            }


            UiClientContexts = new Dictionary<string, UiClientContext>();

            //_joinTokens = new Dictionary<string, JoinToken>();

            if (Global.Platform == eDevicePlatform.Appliance)
            {
                AddConsoleCommands();
            }

            AddPreActivationAction(() => AddWebApiPaths());
        }

        private void AddWebApiPaths()
        {
            var apiServer = DeviceManager.AllDevices.OfType<EssentialsWebApi>().FirstOrDefault();

            if (apiServer == null)
            {
                this.LogInformation("No API Server available");
                return;
            }

            var routes = new List<HttpCwsRoute>
            {
                new HttpCwsRoute($"device/{Key}/client")
                {
                    Name = "ClientHandler",
                    RouteHandler = new UiClientHandler(this)
                },

                new HttpCwsRoute($"device/{Key}/deleteAllUiClients")
                {
                    Name = "DeleteAllClientsHandler",
                    RouteHandler = new DeleteAllUiClientsHandler(this)
                },
            };

            apiServer.AddRoute(routes);
        }

        private void AddConsoleCommands()
        {
            CrestronConsole.AddNewConsoleCommand(GenerateClientTokenFromConsole, "MobileAddUiClient", "Adds a client and generates a token. ? for more help", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(RemoveToken, "MobileRemoveUiClient", "Removes a client. ? for more help", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand((s) => PrintClientInfo(), "MobileGetClientInfo", "Displays the current client info", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(RemoveAllTokens, "MobileRemoveAllClients", "Removes all clients", ConsoleAccessLevelEnum.AccessOperator);
        }

        private struct AllowedNetwork
        {
            public byte[] Address;
            public int PrefixLength;
        }

        // null = no filtering configured
        private List<AllowedNetwork> _allowedNetworks;

        // Last time a rate-limited message was logged, keyed by message kind + source address
        private readonly ConcurrentDictionary<string, DateTime> _lastLogged = new ConcurrentDictionary<string, DateTime>();

        private static readonly TimeSpan _logInterval = TimeSpan.FromSeconds(60);

        private const int MaxLoggedPathLength = 200;

        private const int MaxRateLimitEntries = 512;

        /// <summary>
        /// Parses allowedClientNetworks. Invalid entries are logged and skipped.
        /// </summary>
        private void LoadAllowedNetworks()
        {
            var configured = _parent.Config.DirectServer.AllowedClientNetworks;

            if (configured == null || configured.Count == 0)
            {
                _allowedNetworks = null;
                return;
            }

            var parsed = new List<AllowedNetwork>();

            foreach (var entry in configured)
            {
                if (TryParseCidr(entry, out var network))
                {
                    parsed.Add(network);
                    continue;
                }

                this.LogWarning("Ignoring invalid allowedClientNetworks entry '{entry}'. Expected CIDR notation like 192.168.10.0/24", entry);
            }

            _allowedNetworks = parsed;

            this.LogInformation("Restricting HTTP clients to the Control Subnet, loopback and {count} configured network(s)", parsed.Count);
        }

        private static bool TryParseCidr(string value, out AllowedNetwork network)
        {
            network = default(AllowedNetwork);

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var parts = value.Trim().Split('/');

            if (parts.Length > 2 || !System.Net.IPAddress.TryParse(parts[0], out var address))
            {
                return false;
            }

            var bytes = address.GetAddressBytes();
            var prefix = bytes.Length * 8;

            if (parts.Length == 2 && (!int.TryParse(parts[1], out prefix) || prefix < 0 || prefix > bytes.Length * 8))
            {
                return false;
            }

            network = new AllowedNetwork { Address = bytes, PrefixLength = prefix };
            return true;
        }

        /// <summary>
        /// True for ::ffff:a.b.c.d, the form an IPv4 client takes on a dual-stack listener.
        /// </summary>
        private static bool IsIPv4MappedBytes(byte[] bytes)
        {
            for (var i = 0; i < 10; i++)
            {
                if (bytes[i] != 0)
                {
                    return false;
                }
            }

            return bytes[10] == 0xFF && bytes[11] == 0xFF;
        }

        private static bool IsInNetwork(byte[] remote, AllowedNetwork network)
        {
            if (remote.Length != network.Address.Length)
            {
                return false;
            }

            var fullBytes = network.PrefixLength / 8;
            var remainingBits = network.PrefixLength % 8;

            for (var i = 0; i < fullBytes; i++)
            {
                if (remote[i] != network.Address[i])
                {
                    return false;
                }
            }

            if (remainingBits == 0)
            {
                return true;
            }

            var mask = (byte)(0xFF << (8 - remainingBits));
            return (remote[fullBytes] & mask) == (network.Address[fullBytes] & mask);
        }

        /// <summary>
        /// True if a request from this address should be served.
        /// </summary>
        private bool IsClientAllowed(System.Net.IPAddress remote)
        {
            if (_allowedNetworks == null)
            {
                return true;
            }

            if (remote == null)
            {
                return false;
            }

            var bytes = remote.GetAddressBytes();

            // An IPv4 address can arrive as an IPv4-mapped IPv6 address (::ffff:a.b.c.d)
            if (bytes.Length == 16 && IsIPv4MappedBytes(bytes))
            {
                var v4 = new byte[4];
                Array.Copy(bytes, 12, v4, 0, 4);
                bytes = v4;
                remote = new System.Net.IPAddress(v4);
            }

            // After unwrapping: IsLoopback is false for ::ffff:127.0.0.1
            if (System.Net.IPAddress.IsLoopback(remote))
            {
                return true;
            }

            // Only compare like with like: IsInSameSubnet throws for an IPv6 client against the IPv4 Control Subnet
            if (csIpAddress != null && csSubnetMask != null && remote.AddressFamily == csIpAddress.AddressFamily
                && remote.IsInSameSubnet(csIpAddress, csSubnetMask))
            {
                return true;
            }

            foreach (var network in _allowedNetworks)
            {
                if (IsInNetwork(bytes, network))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Drops the connection without a response if the client is not allowed. Returns true if it was refused.
        /// </summary>
        private bool RejectIfNotAllowed(HttpListenerRequest req, HttpListenerResponse res)
        {
            var remote = req.RemoteEndPoint?.Address;

            if (IsClientAllowed(remote))
            {
                return false;
            }

            LogRateLimited("rejected", remote, () =>
                this.LogWarning("Refused HTTP request from {host}: not in the Control Subnet or allowedClientNetworks", remote));

            RecordUnwantedRequest(remote);

            DropConnection(res);
            return true;
        }

        /// <summary>
        /// Closes the connection without writing a response.
        /// </summary>
        /// <remarks>
        /// Used for requests we are refusing. Writing even a short reply means a send on a socket the other end
        /// may already have reset, which throws from inside the HTTP stack (seen in the field as
        /// "Unable to write data to the transport connection: Connection reset by peer" from
        /// HttpListenerResponse.Close). Abort() writes nothing, so there is nothing to fail.
        /// </remarks>
        private void DropConnection(HttpListenerResponse res)
        {
            try
            {
                res.Abort();
            }
            catch (Exception ex)
            {
                // The connection is already gone, which is the outcome we wanted
                this.LogDebug("Exception dropping connection: {message}", ex.Message);
            }
        }

        /// <summary>
        /// Runs the log action at most once per interval for each (kind, address) pair, so that a scan
        /// producing hundreds of requests cannot flood the log.
        /// </summary>
        private void LogRateLimited(string kind, System.Net.IPAddress remote, Action log)
        {
            var key = kind + "|" + (remote?.ToString() ?? "unknown");
            var now = DateTime.UtcNow;

            // Check and update together, or a burst of concurrent requests from one source would each see a
            // stale timestamp and all log
            lock (_lastLogged)
            {
                if (_lastLogged.Count > MaxRateLimitEntries)
                {
                    _lastLogged.Clear();
                }

                if (_lastLogged.TryGetValue(key, out var last) && now - last < _logInterval)
                {
                    return;
                }

                _lastLogged[key] = now;
            }

            log();
        }

        private static string TruncateForLog(string value)
        {
            if (value == null || value.Length <= MaxLoggedPathLength)
            {
                return value;
            }

            return value.Substring(0, MaxLoggedPathLength) + "...(" + value.Length + " chars)";
        }

        // ------------------------------------------------------------------------------------------------
        // Automatic blocking of addresses that send a burst of unwanted requests (off unless configured)
        // ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Counts events per key inside a sliding window. Not thread-safe: callers lock.
        /// </summary>
        private sealed class SlidingWindowCounter
        {
            private readonly Dictionary<string, Queue<DateTime>> _events = new Dictionary<string, Queue<DateTime>>();
            private readonly TimeSpan _window;
            private readonly int _maxKeys;

            public SlidingWindowCounter(TimeSpan window, int maxKeys)
            {
                _window = window;
                _maxKeys = maxKeys;
            }

            /// <summary>
            /// Records an event and returns how many fall inside the window, including this one.
            /// </summary>
            public int Record(string key, DateTime now)
            {
                Queue<DateTime> queue;

                if (!_events.TryGetValue(key, out queue))
                {
                    // Bounded memory. Losing counts only delays a block; it can never cause one.
                    if (_events.Count >= _maxKeys)
                    {
                        _events.Clear();
                    }

                    queue = new Queue<DateTime>();
                    _events[key] = queue;
                }

                queue.Enqueue(now);

                while (queue.Count > 0 && now - queue.Peek() > _window)
                {
                    queue.Dequeue();
                }

                return queue.Count;
            }

            public void Reset(string key)
            {
                _events.Remove(key);
            }
        }

        private static readonly System.Text.RegularExpressions.Regex Ipv4Literal = new System.Text.RegularExpressions.Regex(
            @"^(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}\z");

        /// <summary>
        /// True only for a plain dotted-decimal IPv4 address. Anything that goes into a console command
        /// has to pass this first.
        /// </summary>
        private static bool IsIpv4Literal(string value)
        {
            return value != null && Ipv4Literal.IsMatch(value);
        }

        /// <summary>
        /// True if the output of listblocked names this address as an entry of its own.
        /// </summary>
        private static bool BlockListContains(string listOutput, string address)
        {
            if (string.IsNullOrEmpty(listOutput) || !IsIpv4Literal(address))
            {
                return false;
            }

            return System.Text.RegularExpressions.Regex.IsMatch(
                listOutput,
                @"(^|\s)" + System.Text.RegularExpressions.Regex.Escape(address) + @"(\s|$)",
                System.Text.RegularExpressions.RegexOptions.Multiline);
        }

        /// <summary>
        /// Requests that every ordinary browser makes and this server has never answered.
        /// </summary>
        /// <remarks>
        /// The app's index.html sets its own &lt;base&gt; from an inline script, but the browser's preload
        /// scanner requests ./assets/* first, relative to /mc/, so each page load asks for /mc/assets/* and gets a
        /// 404 before the real requests succeed under /mc/app/assets/. Browsers also ask for /favicon.ico. These
        /// must not count as unwanted traffic, or a person reloading the app a few times would be blocked.
        /// </remarks>
        private static bool IsBenignBrowserRequest(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var queryStart = path.IndexOf('?');
            var withoutQuery = queryStart >= 0 ? path.Substring(0, queryStart) : path;

            return withoutQuery.StartsWith("/mc/assets/", StringComparison.Ordinal)
                || string.Equals(withoutQuery, "/favicon.ico", StringComparison.OrdinalIgnoreCase);
        }

        // Used by builds before the file was made per-instance. Read once and then removed.
        private const string LegacyAutoBlockFileName = "autoBlockedIps.json";

        private const int SuspiciousWindowSeconds = 60;

        private const int MaxTrackedAddresses = 512;

        private bool _autoBlockEnabled;
        private bool _autoBlockDryRun;
        private int _autoBlockThreshold = 10;
        private TimeSpan _autoBlockDuration = TimeSpan.FromMinutes(30);
        private int _autoBlockMaxConcurrent = 8;
        private List<AllowedNetwork> _neverBlockNetworks = new List<AllowedNetwork>();
        private System.Net.IPAddress _lanIpAddress;
        private CTimer _autoBlockTimer;
        private int _expiryRunning;

        private readonly object _autoBlockLock = new object();
        private readonly SlidingWindowCounter _unwantedRequests = new SlidingWindowCounter(TimeSpan.FromSeconds(SuspiciousWindowSeconds), MaxTrackedAddresses);

        // address -> when Essentials removes the block (UTC). Only blocks Essentials added are ever listed here,
        // so a block someone added by hand is never removed.
        private readonly Dictionary<string, DateTime> _autoBlocked = new Dictionary<string, DateTime>();

        // Addresses whose ADDBLOCKEDIP is queued or running. Expiry skips them, so a delayed command can never run
        // after its record has been removed and leave a block nobody owns.
        private readonly HashSet<string> _pendingBlocks = new HashSet<string>();

        // One file per instance: each server owns the blocks it added, and more than one controller can be configured
        private string AutoBlockFilePath
        {
            get
            {
                var safeKey = new string(Key.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
                return Global.FilePathPrefix + "autoBlockedIps-" + safeKey + ".json";
            }
        }

        private List<AllowedNetwork> ParseNetworks(List<string> entries, string settingName)
        {
            var parsed = new List<AllowedNetwork>();

            if (entries == null)
            {
                return parsed;
            }

            foreach (var entry in entries)
            {
                AllowedNetwork network;

                if (TryParseCidr(entry, out network))
                {
                    parsed.Add(network);
                    continue;
                }

                this.LogWarning("Ignoring invalid {setting} entry '{entry}'. Expected CIDR notation like 192.168.10.0/24", settingName, entry);
            }

            return parsed;
        }

        private void LoadAutoBlockSettings()
        {
            var config = _parent.Config.DirectServer.AutoBlock;
            var wanted = config != null && config.Enabled;

            if (CrestronEnvironment.DevicePlatform != eDevicePlatform.Appliance)
            {
                if (wanted)
                {
                    this.LogWarning("autoBlock needs a 4-series appliance and is ignored on this platform");
                }

                return;
            }

            // Blocks survive a reboot, so anything Essentials added before has to be removed on schedule even if
            // the setting has since been turned off.
            LoadAutoBlockedFromDisk();

            if (wanted)
            {
                _autoBlockDryRun = config.DryRun;
                _autoBlockThreshold = Math.Max(3, config.RequestsPerMinute);
                _autoBlockDuration = TimeSpan.FromMinutes(Math.Min(1440, Math.Max(1, config.BlockMinutes)));
                _autoBlockMaxConcurrent = Math.Min(64, Math.Max(1, config.MaxConcurrentBlocks));
                _neverBlockNetworks = ParseNetworks(config.NeverBlock, "autoBlock.neverBlock");

                try
                {
                    var lanAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);
                    _lanIpAddress = System.Net.IPAddress.Parse(CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, lanAdapterId));
                }
                catch (Exception ex)
                {
                    this.LogDebug("Could not read the LAN address for the auto-block exemptions: {message}", ex.Message);
                }

                _autoBlockEnabled = true;

                this.LogInformation(
                    "Auto-block is on{dryRun}: {threshold} unwanted requests in a minute blocks an address for {minutes} minutes (at most {max} at once)",
                    _autoBlockDryRun ? " (dry run, nothing will be blocked)" : string.Empty,
                    _autoBlockThreshold, (int)_autoBlockDuration.TotalMinutes, _autoBlockMaxConcurrent);
            }

            bool outstanding;
            lock (_autoBlockLock)
            {
                outstanding = _autoBlocked.Count > 0;
            }

            if (_autoBlockEnabled || outstanding)
            {
                _autoBlockTimer = new CTimer(CheckAutoBlockExpiry, null, 5000, 30000);
            }
        }

        /// <summary>
        /// The IPv4 form of an address, unwrapping IPv4-mapped IPv6. False for anything else.
        /// </summary>
        private static bool TryGetIpv4(System.Net.IPAddress address, out System.Net.IPAddress ipv4)
        {
            ipv4 = null;

            if (address == null)
            {
                return false;
            }

            var bytes = address.GetAddressBytes();

            if (bytes.Length == 16 && IsIPv4MappedBytes(bytes))
            {
                var v4 = new byte[4];
                Array.Copy(bytes, 12, v4, 0, 4);
                ipv4 = new System.Net.IPAddress(v4);
                return true;
            }

            if (bytes.Length == 4)
            {
                ipv4 = address;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Addresses that are never blocked: this processor, its Control Subnet, loopback and anything the
        /// installer has listed as trusted.
        /// </summary>
        private bool IsExemptFromAutoBlock(System.Net.IPAddress ipv4)
        {
            if (System.Net.IPAddress.IsLoopback(ipv4))
            {
                return true;
            }

            if (csIpAddress != null && csSubnetMask != null && ipv4.IsInSameSubnet(csIpAddress, csSubnetMask))
            {
                return true;
            }

            if (ipv4.Equals(csIpAddress) || ipv4.Equals(_lanIpAddress))
            {
                return true;
            }

            var bytes = ipv4.GetAddressBytes();

            foreach (var network in _neverBlockNetworks)
            {
                if (IsInNetwork(bytes, network))
                {
                    return true;
                }
            }

            if (_allowedNetworks != null)
            {
                foreach (var network in _allowedNetworks)
                {
                    if (IsInNetwork(bytes, network))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Counts a request that no legitimate client sends. Blocks the address once it sends too many.
        /// </summary>
        private void RecordUnwantedRequest(System.Net.IPAddress remote)
        {
            if (!_autoBlockEnabled)
            {
                return;
            }

            System.Net.IPAddress ipv4;

            if (!TryGetIpv4(remote, out ipv4) || IsExemptFromAutoBlock(ipv4))
            {
                return;
            }

            var address = ipv4.ToString();
            int count;

            lock (_autoBlockLock)
            {
                if (_autoBlocked.ContainsKey(address))
                {
                    return;
                }

                count = _unwantedRequests.Record(address, DateTime.UtcNow);

                if (count < _autoBlockThreshold)
                {
                    return;
                }

                _unwantedRequests.Reset(address);
            }

            BlockAddress(address, count);
        }

        private void BlockAddress(string address, int count)
        {
            if (!IsIpv4Literal(address))
            {
                return;
            }

            if (_autoBlockDryRun)
            {
                LogRateLimited("dryrun", System.Net.IPAddress.Parse(address), () =>
                    this.LogWarning("[dry run] Would block {address} for {minutes} minutes: {count} unwanted requests within a minute", address, (int)_autoBlockDuration.TotalMinutes, count));
                return;
            }

            lock (_autoBlockLock)
            {
                // Another burst may have crossed the threshold since RecordUnwantedRequest released the lock.
                // Without this a second ADDBLOCKEDIP would fail as a duplicate and drop the first one's entry.
                if (_autoBlocked.ContainsKey(address))
                {
                    return;
                }

                if (_autoBlocked.Count >= _autoBlockMaxConcurrent)
                {
                    LogRateLimited("autoblock-cap", null, () =>
                        this.LogWarning("Not blocking {address}: already holding {max} automatic blocks", address, _autoBlockMaxConcurrent));
                    return;
                }

                // Recorded before the command runs. If the program stopped between the two, the block would
                // otherwise outlive it with nobody responsible for removing it. For the same reason, no record
                // on disk means no block.
                _autoBlocked[address] = DateTime.UtcNow + _autoBlockDuration;

                if (!SaveAutoBlocked())
                {
                    _autoBlocked.Remove(address);
                    this.LogWarning("Not blocking {address}: the block could not be recorded, so it could not be removed after a restart", address);
                    return;
                }

                _pendingBlocks.Add(address);
            }

            // The console call can take a moment, so keep it off the request thread
            try
            {
                CrestronInvoke.BeginInvoke(o => ExecuteBlock(address, count));
            }
            catch (Exception ex)
            {
                this.LogError("Could not queue the block for {address}: {message}", address, ex.Message);

                lock (_autoBlockLock)
                {
                    _pendingBlocks.Remove(address);
                    _autoBlocked.Remove(address);
                    SaveAutoBlocked();
                }
            }
        }

        private void ExecuteBlock(string address, int count)
        {
            try
            {
                var response = string.Empty;
                CrestronConsole.SendControlSystemCommand("addblockedip " + address, ref response);

                if (response != null && response.IndexOf("Added IP", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lock (_autoBlockLock)
                    {
                        // The block lasts from when it was actually added, however long the command waited in the queue
                        _pendingBlocks.Remove(address);
                        _autoBlocked[address] = DateTime.UtcNow + _autoBlockDuration;
                        SaveAutoBlocked();
                    }

                    this.LogWarning("Blocked {address} for {minutes} minutes: {count} unwanted requests within a minute", address, (int)_autoBlockDuration.TotalMinutes, count);
                    return;
                }

                this.LogWarning("Could not block {address}. The console said: {response}", address, response == null ? string.Empty : response.Trim());
            }
            catch (Exception ex)
            {
                this.LogError("Exception blocking {address}: {message}", address, ex.Message);
            }

            // Not blocked, so Essentials has nothing to remove later
            lock (_autoBlockLock)
            {
                _pendingBlocks.Remove(address);
                _autoBlocked.Remove(address);
                SaveAutoBlocked();
            }
        }

        private void CheckAutoBlockExpiry(object unused)
        {
            // One pass at a time: the console calls can outlast the timer interval
            if (System.Threading.Interlocked.CompareExchange(ref _expiryRunning, 1, 0) != 0)
            {
                return;
            }

            try
            {
                List<string> due;

                lock (_autoBlockLock)
                {
                    var now = DateTime.UtcNow;
                    due = _autoBlocked.Where(kv => kv.Value <= now && !_pendingBlocks.Contains(kv.Key)).Select(kv => kv.Key).ToList();
                }

                foreach (var address in due)
                {
                    RemoveBlock(address);
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception removing expired blocks: {message}", ex.Message);
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _expiryRunning, 0);
            }
        }

        private void RemoveBlock(string address)
        {
            if (IsIpv4Literal(address))
            {
                var response = string.Empty;
                CrestronConsole.SendControlSystemCommand("remblockedip " + address, ref response);

                // Confirm it is gone rather than trusting the wording of the reply. If it is still listed the entry
                // stays and the next pass tries again, so a failed removal is never forgotten.
                var list = string.Empty;

                // A failed query leaves the list empty, which would read as "removed"
                if (!CrestronConsole.SendControlSystemCommand("listblocked", ref list))
                {
                    this.LogWarning("Could not confirm {address} was unblocked: listblocked failed. Will retry", address);
                    return;
                }

                if (BlockListContains(list, address))
                {
                    this.LogWarning("{address} is still blocked after trying to remove it. Will retry", address);
                    return;
                }
            }

            lock (_autoBlockLock)
            {
                _autoBlocked.Remove(address);
                SaveAutoBlocked();
            }

            this.LogInformation("Unblocked {address}: its automatic block expired", address);
        }

        /// <summary>
        /// Writes the list of blocks Essentials owns. Returns false if it could not be written.
        /// </summary>
        /// <remarks>Caller holds _autoBlockLock.</remarks>
        private bool SaveAutoBlocked()
        {
            try
            {
                var path = AutoBlockFilePath;

                if (_autoBlocked.Count == 0)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    return true;
                }

                // Written to a temporary file and then renamed over the real one, so a power loss mid-write cannot
                // leave a truncated file that loses track of every block
                var data = _autoBlocked.ToDictionary(kv => kv.Key, kv => kv.Value.ToString("o"));
                var tempPath = path + ".tmp";
                File.WriteAllText(tempPath, JsonConvert.SerializeObject(data, Formatting.Indented));

                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, null);
                }
                else
                {
                    File.Move(tempPath, path);
                }

                return true;
            }
            catch (Exception ex)
            {
                this.LogError("Could not save the list of automatic blocks: {message}", ex.Message);
                return false;
            }
        }

        private void LoadAutoBlockedFromDisk()
        {
            var path = AutoBlockFilePath;
            var legacyPath = Global.FilePathPrefix + LegacyAutoBlockFileName;

            ReadAutoBlockedFile(path);

            if (!File.Exists(legacyPath))
            {
                return;
            }

            // Take over blocks recorded under the old shared file name, then remove it once they are saved here
            if (ReadAutoBlockedFile(legacyPath))
            {
                lock (_autoBlockLock)
                {
                    if (!SaveAutoBlocked())
                    {
                        return;
                    }
                }

                try
                {
                    File.Delete(legacyPath);
                }
                catch (Exception ex)
                {
                    this.LogDebug("Could not remove {path}: {message}", legacyPath, ex.Message);
                }
            }
        }

        /// <summary>
        /// Adds the entries in a block-list file to the blocks Essentials owns. Returns true if the file was read.
        /// </summary>
        private bool ReadAutoBlockedFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));

                if (data == null)
                {
                    return true;
                }

                lock (_autoBlockLock)
                {
                    foreach (var entry in data)
                    {
                        DateTime expiry;

                        if (IsIpv4Literal(entry.Key) && DateTime.TryParse(entry.Value, null, System.Globalization.DateTimeStyles.RoundtripKind, out expiry))
                        {
                            _autoBlocked[entry.Key] = expiry.ToUniversalTime();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                this.LogError("Could not read the list of automatic blocks from {path}: {message}", path, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Initialize method
        /// </summary>
        /// <inheritdoc />
        public override void Initialize()
        {
            try
            {
                base.Initialize();

                LoadAllowedNetworks();

                LoadAutoBlockSettings();

                _server = new HttpServer(Port, _parent.Config.DirectServer.Secure);

                _server.OnGet += Server_OnGet;

                _server.OnOptions += Server_OnOptions;

                // Always subscribed so POST requests go through the allowlist; log forwarding is gated inside
                _server.OnPost += Server_OnPost;

                if (_parent.Config.DirectServer.Secure)
                {
                    this.LogInformation("Adding SSL Configuration to server");
                    _server.SslConfiguration = new ServerSslConfiguration(new X509Certificate2($"\\user\\{certificateName}.pfx", certificatePassword))
                    {
                        ClientCertificateRequired = false,
                        CheckCertificateRevocation = false,
                        EnabledSslProtocols = SslProtocols.Tls12
                    };
                }

                _server.Log.Output = (data, message) => Utilities.ConvertWebsocketLog(data, message, this);

                // setting to trace to allow logging level to be controlled by appdebug
                _server.Log.Level = LogLevel.Trace;

                CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironment_ProgramStatusEventHandler;

                _server.Start();

                if (_server.IsListening)
                {
                    this.LogInformation("Mobile Control WebSocket Server listening on port {port}", _server.Port);
                }

                CrestronEnvironment.ProgramStatusEventHandler += OnProgramStop;

                RetrieveSecret();

                CreateFolderStructure();

                AddClientsForTouchpanels();
            }
            catch (Exception ex)
            {
                this.LogError("Exception initializing direct server: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace");
            }
        }

        /// <summary>
        /// Set the internal logging level for the Websocket Server
        /// </summary>
        public void SetWebsocketLogLevel(LogLevel level)
        {
            CrestronConsole.ConsoleCommandResponse($"Setting direct server debug level to {level}", level.ToString());
            _server.Log.Level = level;
        }

        private void AddClientsForTouchpanels()
        {
            var touchpanels = DeviceManager.AllDevices
                .OfType<IMobileControlTouchpanelController>().Where(tp => tp.UseDirectServer);


            var touchpanelsToAdd = new List<IMobileControlTouchpanelController>();

            if (_secret != null)
            {
                var newTouchpanels = touchpanels.Where(tp => !_secret.Tokens.Any(t => t.Value.TouchpanelKey != null && t.Value.TouchpanelKey.Equals(tp.Key, StringComparison.InvariantCultureIgnoreCase)));

                touchpanelsToAdd.AddRange(newTouchpanels);
            }
            else
            {
                touchpanelsToAdd.AddRange(touchpanels);
            }

            foreach (var client in touchpanelsToAdd)
            {
                var bridge = _parent.GetRoomBridge(client.DefaultRoomKey);

                if (bridge == null)
                {
                    this.LogWarning("Unable to find room with key: {defaultRoomKey}", client.DefaultRoomKey);
                    return;
                }

                var (key, path) = GenerateClientToken(bridge, client.Key);

                if (key == null)
                {
                    this.LogWarning("Unable to generate a client for {clientKey}", client.Key);
                    continue;
                }
            }

            var lanAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);

            var processorIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, lanAdapterId);

            foreach (var touchpanel in touchpanels.Select(tp =>
            {
                var token = _secret.Tokens.FirstOrDefault((t) => t.Value.TouchpanelKey.Equals(tp.Key, StringComparison.InvariantCultureIgnoreCase));

                var messenger = _parent.GetRoomBridge(tp.DefaultRoomKey);

                return new { token.Key, Touchpanel = tp, Messenger = messenger };
            }))
            {
                if (touchpanel.Key == null)
                {
                    this.LogWarning("Token for touchpanel {touchpanelKey} not found", touchpanel.Touchpanel.Key);
                    continue;
                }

                if (touchpanel.Messenger == null)
                {
                    this.LogWarning("Unable to find room messenger for {defaultRoomKey}", touchpanel.Touchpanel.DefaultRoomKey);
                    continue;
                }

                string ip = processorIp;

                if (_parent.Config.DirectServer.CSLanUiDeviceKeys != null && _parent.Config.DirectServer.CSLanUiDeviceKeys.Any(k => k.Equals(touchpanel.Touchpanel.Key, StringComparison.InvariantCultureIgnoreCase)) && csIpAddress != null)
                {
                    ip = csIpAddress.ToString();
                }

                var appUrl = $"{HttpScheme}://{ip}:{Port}/mc/app?token={touchpanel.Key}";

                this.LogVerbose("Sending URL {appUrl} to touchpanel {touchpanelKey}", appUrl, touchpanel.Touchpanel.Key);

                touchpanel.Touchpanel.SetAppUrl(appUrl);
            }
        }

        private void OnProgramStop(eProgramStatusEventType programEventType)
        {
            switch (programEventType)
            {
                case eProgramStatusEventType.Stopping:
                    _server.Stop();
                    break;
            }
        }

        private void CreateFolderStructure()
        {
            if (!Directory.Exists(userAppPath))
            {
                Directory.CreateDirectory(userAppPath);
            }

            if (!Directory.Exists($"{userAppPath}{localConfigFolderName}"))
            {
                Directory.CreateDirectory($"{userAppPath}{localConfigFolderName}");
            }

            using (var sw = new StreamWriter(File.Open($"{userAppPath}{localConfigFolderName}{Global.DirectorySeparator}{appConfigFileName}", FileMode.Create, FileAccess.ReadWrite)))
            {
                // Write the LAN application configuration file. Used when a request comes in for the application config from the LAN 
                var lanAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);

                this.LogDebug("LAN Adapter ID: {lanAdapterId}", lanAdapterId);

                var processorIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, lanAdapterId);

                var config = GetApplicationConfig(processorIp);

                var contents = JsonConvert.SerializeObject(config, Formatting.Indented);

                sw.Write(contents);
            }

            short csAdapterId;
            try
            {
                csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
            }
            catch (ArgumentException)
            {
                this.LogDebug("This processor does not have a CS LAN");
                return;
            }

            if (csAdapterId == -1)
            {
                this.LogDebug("CS LAN Adapter not found");
                return;
            }

            this.LogDebug("CS LAN Adapter ID: {csAdapterId}. Adding CS Config", csAdapterId);

            using (var sw = new StreamWriter(File.Open($"{userAppPath}{localConfigFolderName}{Global.DirectorySeparator}{appConfigCsFileName}", FileMode.Create, FileAccess.ReadWrite)))
            {
                // Write the CS application configuration file. Used when a request comes in for the application config from the CS
                var processorIp = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                var config = GetApplicationConfig(processorIp);

                var contents = JsonConvert.SerializeObject(config, Formatting.Indented);

                sw.Write(contents);
            }
        }

        private MobileControlApplicationConfig GetApplicationConfig(string processorIp)
        {
            try
            {
                var config = new MobileControlApplicationConfig
                {
                    ApiPath = string.Format("{0}://{1}:{2}/mc/api", HttpScheme, processorIp, Port),
                    GatewayAppPath = "",
                    LogoPath = _parent.Config.ApplicationConfig?.LogoPath ?? "logo/logo.png",
                    EnableDev = _parent.Config.ApplicationConfig?.EnableDev ?? false,
                    IconSet = _parent.Config.ApplicationConfig?.IconSet ?? MCIconSet.GOOGLE,
                    LoginMode = _parent.Config.ApplicationConfig?.LoginMode ?? "room-list",
                    Modes = _parent.Config.ApplicationConfig?.Modes ?? new Dictionary<string, McMode>
                    {
                        {
                            "room-list",
                            new McMode {
                                ListPageText = "Please select your room",
                                LoginHelpText = "Please select your room from the list, then enter the code shown on the display.",
                                PasscodePageText = "Please enter the code shown on this room's display"
                            }
                        }
                    },
                    Logging = _parent.Config.ApplicationConfig?.Logging ?? false,
                    PartnerMetadata = _parent.Config.ApplicationConfig?.PartnerMetadata ?? new List<MobileControlPartnerMetadata>()
                };

                return config;
            }
            catch (Exception ex)
            {
                this.LogError("Error getting application configuration: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace");

                return null;
            }
        }

        /// <summary>
        /// Attempts to retrieve secrets previously stored in memory
        /// </summary>
        private void RetrieveSecret()
        {
            try
            {
                // Add secret provider
                _secretProvider = new WebSocketServerSecretProvider(SecretProviderKey);

                // Check for existing secrets
                var secret = _secretProvider.GetSecret(SecretProviderKey);

                if (secret != null)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Secret successfully retrieved", this);

                    Debug.LogMessage(LogEventLevel.Debug, "Secret: {0}", this, secret.Value.ToString());


                    // populate the local secrets object
                    _secret = JsonConvert.DeserializeObject<ServerTokenSecrets>(secret.Value.ToString());

                    if (_secret != null && _secret.Tokens != null)
                    {
                        // populate the _uiClient collection
                        foreach (var token in _secret.Tokens)
                        {
                            if (token.Value == null)
                            {
                                this.LogWarning("Token value is null");
                                continue;
                            }

                            this.LogInformation("Adding token: {key} for room: {roomKey}", token.Key, token.Value.RoomKey);

                            if (UiClientContexts == null)
                            {
                                UiClientContexts = new Dictionary<string, UiClientContext>();
                            }

                            UiClientContexts.Add(token.Key, new UiClientContext(token.Value));
                        }
                    }

                    if (UiClientContexts.Count > 0)
                    {
                        this.LogInformation("Restored {uiClientCount} UiClients from secrets data", UiClientContexts.Count);

                        foreach (var client in UiClientContexts)
                        {
                            var key = client.Key;
                            var path = _wsPath + key;
                            var roomKey = client.Value.Token.RoomKey;

                            _server.AddWebSocketService(path, () =>
                            {
                                this.LogInformation("Building a UiClient with ID {id}", client.Value.Token.Id);
                                return BuildUiClient(roomKey, client.Value.Token, key);
                            });
                        }
                    }
                }
                else
                {
                    this.LogWarning("No secret found");
                }

                this.LogDebug("{uiClientCount} UiClients restored from secrets data", UiClientContexts.Count);
            }
            catch (Exception ex)
            {
                this.LogError("Exception retrieving secret: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace");
            }
        }

        /// <summary>
        /// UpdateSecret method
        /// </summary>
        public void UpdateSecret()
        {
            try
            {
                if (_secret == null)
                {
                    this.LogError("Secret is null");

                    _secret = new ServerTokenSecrets(string.Empty);
                }

                _secret.Tokens.Clear();

                foreach (var uiClientContext in UiClientContexts)
                {
                    _secret.Tokens.Add(uiClientContext.Key, uiClientContext.Value.Token);
                }

                var serializedSecret = JsonConvert.SerializeObject(_secret);

                _secretProvider.SetSecret(SecretProviderKey, serializedSecret);
            }
            catch (Exception ex)
            {
                this.LogError("Exception updating secret: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace");
            }
        }

        /// <summary>
        /// Generates a new token based on validating a room key and grant code passed in.  If valid, returns a token and adds a service to the server for that token's path
        /// </summary>
        /// <param name="s"></param>
        private void GenerateClientTokenFromConsole(string s)
        {
            if (s == "?" || string.IsNullOrEmpty(s))
            {
                CrestronConsole.ConsoleCommandResponse(@"[RoomKey] [GrantCode] Validates the room key against the grant code and returns a token for use in a UI client");
                return;
            }

            var values = s.Split(' ');

            if (values.Length < 2)
            {
                CrestronConsole.ConsoleCommandResponse("Invalid number of arguments.  Please provide a room key and a grant code");
                return;
            }


            var roomKey = values[0];
            var grantCode = values[1];

            var bridge = _parent.GetRoomBridge(roomKey);

            if (bridge == null)
            {
                CrestronConsole.ConsoleCommandResponse(string.Format("Unable to find room with key: {0}", roomKey));
                return;
            }

            var (token, path) = ValidateGrantCode(grantCode, bridge);

            if (token == null)
            {
                CrestronConsole.ConsoleCommandResponse("Grant Code is not valid");
                return;
            }

            CrestronConsole.ConsoleCommandResponse($"Added new WebSocket UiClient service at path: {path}");
            CrestronConsole.ConsoleCommandResponse($"Token: {token}");
        }

        /// <summary>
        /// Validates the grant code against the room key
        /// </summary>
        public (string, string) ValidateGrantCode(string grantCode, string roomKey)
        {
            var bridge = _parent.GetRoomBridge(roomKey);

            if (bridge == null)
            {
                this.LogWarning("Unable to find room with key: {roomKey}", roomKey);
                return (null, null);
            }

            return ValidateGrantCode(grantCode, bridge);
        }

        /// <summary>
        /// Validates the grant code against the room key
        /// </summary>
        public (string, string) ValidateGrantCode(string grantCode, MobileControlBridgeBase bridge)
        {
            // TODO: Authenticate grant code passed in
            // For now, we just generate a random guid as the token and use it as the ClientId as well
            var grantCodeIsValid = true;

            if (grantCodeIsValid)
            {
                if (_secret == null)
                {
                    _secret = new ServerTokenSecrets(grantCode);
                }

                return GenerateClientToken(bridge, "");
            }
            else
            {
                return (null, null);
            }
        }

        /// <summary>
        /// Generates a new client token for the specified bridge
        /// </summary>
        public (string, string) GenerateClientToken(MobileControlBridgeBase bridge, string touchPanelKey = "")
        {
            var key = Guid.NewGuid().ToString();

            var token = new JoinToken { Code = bridge.UserCode, RoomKey = bridge.RoomKey, Uuid = _parent.SystemUuid, TouchpanelKey = touchPanelKey };

            UiClientContexts.Add(key, new UiClientContext(token));

            var path = _wsPath + key;

            _server.AddWebSocketService(path, () =>
            {
                this.LogInformation("Building a UiClient with ID {id}", token.Id);
                return BuildUiClient(bridge.RoomKey, token, key);
            });

            this.LogInformation("Added new WebSocket UiClient for path: {path}", path);
            this.LogInformation("Token: {@token}", token);

            this.LogVerbose("{serviceCount} websocket services present", _server.WebSocketServices.Count);

            UpdateSecret();

            return (key, path);
        }

        private UiClient BuildUiClient(string roomKey, JoinToken token, string key)
        {
            // Get the most recent unused clientId for this token (legacy support)
            // New clients will override this ID in OnOpen with the validated query parameter value
            var clientId = "pending";
            if (legacyClientRegistrations.TryGetValue(key, out var registrations))
            {
                // Get most recent registration
                var sorted = registrations.OrderByDescending(r => r.timestamp).ToList();
                if (sorted.Any())
                {
                    clientId = sorted.First().clientId;
                    // Remove it from the bag
                    var newBag = new ConcurrentBag<(string, DateTime)>(sorted.Skip(1));
                    legacyClientRegistrations.TryUpdate(key, newBag, registrations);
                    this.LogVerbose("Assigned most recent legacy clientId {clientId} for token {token}", clientId, key);
                }
            }

            var c = new UiClient($"uiclient-{key}-{roomKey}-{clientId}", clientId, token.Token, token.TouchpanelKey);
            this.LogInformation("Constructing UiClient with key {key} and temporary ID (will be set from query param)", key);
            c.Controller = _parent;
            c.RoomKey = roomKey;
            c.TokenKey = key; // Store the URL token key for filtering
            c.Server = this; // Give UiClient access to server for ID registration

            // Don't add to uiClients yet - will be added in OnOpen after ID is set from query param

            c.ConnectionClosed += (o, a) =>
            {
                uiClients.TryRemove(a.ClientId, out _);
                // Clean up any pending registrations for this token
                var keysToRemove = pendingClientRegistrations.Keys
                    .Where(k => k.StartsWith($"{key}-"))
                    .ToList();
                foreach (var k in keysToRemove)
                {
                    pendingClientRegistrations.TryRemove(k, out _);
                }

                // Clean up legacy registrations if empty
                if (legacyClientRegistrations.TryGetValue(key, out var legacyBag) && legacyBag.IsEmpty)
                {
                    legacyClientRegistrations.TryRemove(key, out _);
                }
            };
            return c;
        }

        /// <summary>
        /// Registers a UiClient with its validated client ID after WebSocket connection
        /// </summary>
        /// <param name="client">The UiClient to register</param>
        /// <param name="clientId">The validated client ID</param>
        /// <param name="tokenKey">The token key for validation</param>
        /// <returns>True if registration successful, false if validation failed</returns>
        public bool RegisterUiClient(UiClient client, string clientId, string tokenKey)
        {
            var registrationKey = $"{tokenKey}-{clientId}";

            // Verify this clientId was generated during a join request for this token
            if (!pendingClientRegistrations.TryRemove(registrationKey, out _))
            {
                this.LogWarning("Client attempted to connect with unregistered or expired clientId {clientId} for token {token}", clientId, tokenKey);
                return false;
            }

            // Registration is valid - add to active clients
            uiClients.AddOrUpdate(clientId, client, (id, existingClient) =>
            {
                this.LogWarning("Replacing existing client with duplicate id {id}", id);
                return client;
            });

            this.LogInformation("Successfully registered UiClient with ID {clientId} for token {token}", clientId, tokenKey);
            return true;
        }

        /// <summary>
        /// Updates a client's ID when a mismatch is detected between stored ID and message ID
        /// </summary>
        /// <param name="oldClientId">The current/old client ID</param>
        /// <param name="newClientId">The new client ID from the message</param>
        /// <param name="tokenKey">The token key for validation</param>
        /// <returns>True if update successful, false otherwise</returns>
        public bool UpdateClientId(string oldClientId, string newClientId, string tokenKey)
        {
            if (string.IsNullOrEmpty(oldClientId) || string.IsNullOrEmpty(newClientId))
            {
                this.LogWarning("Cannot update client ID with null or empty values");
                return false;
            }

            if (oldClientId == newClientId)
            {
                return true; // No update needed
            }

            // Verify the new clientId was registered for this token
            var registrationKey = $"{tokenKey}-{newClientId}";
            if (!pendingClientRegistrations.TryRemove(registrationKey, out _))
            {
                this.LogWarning("Cannot update to unregistered clientId {newClientId} for token {token}", newClientId, tokenKey);
                return false;
            }

            // Get the existing client
            if (!uiClients.TryRemove(oldClientId, out var client))
            {
                this.LogWarning("Cannot find client with old ID {oldClientId}", oldClientId);
                return false;
            }

            // Update the client's ID
            client.UpdateId(newClientId);

            // Re-add with new ID
            if (!uiClients.TryAdd(newClientId, client))
            {
                // If add fails, try to restore old entry
                uiClients.TryAdd(oldClientId, client);
                client.UpdateId(oldClientId);
                this.LogError("Failed to update client ID from {oldClientId} to {newClientId}", oldClientId, newClientId);
                return false;
            }

            this.LogInformation("Successfully updated client ID from {oldClientId} to {newClientId}", oldClientId, newClientId);
            return true;
        }

        /// <summary>
        /// Registers a UiClient using legacy flow (for backwards compatibility with older clients)
        /// </summary>
        /// <param name="client">The UiClient to register</param>
        public void RegisterLegacyUiClient(UiClient client)
        {
            if (string.IsNullOrEmpty(client.Id))
            {
                this.LogError("Cannot register client with null or empty ID");
                return;
            }

            uiClients.AddOrUpdate(client.Id, client, (id, existingClient) =>
            {
                this.LogWarning("Replacing existing client with duplicate id {id} (legacy flow)", id);
                return client;
            });

            this.LogInformation("Successfully registered UiClient with ID {clientId} using legacy flow", client.Id);
        }

        /// <summary>
        /// Prints out the session data for each path
        /// </summary>
        public void PrintSessionData()
        {
            foreach (var path in _server.WebSocketServices.Paths)
            {
                this.LogInformation("Path: {path}", path);
                this.LogInformation("  Session Count: {sessionCount}", _server.WebSocketServices[path].Sessions.Count);
                this.LogInformation("  Active Session Count: {activeSessionCount}", _server.WebSocketServices[path].Sessions.ActiveIDs.Count());
                this.LogInformation("  Inactive Session Count: {inactiveSessionCount}", _server.WebSocketServices[path].Sessions.InactiveIDs.Count());
                this.LogInformation("  Active Clients:");
                foreach (var session in _server.WebSocketServices[path].Sessions.IDs)
                {
                    this.LogInformation("    Client ID: {id}", (_server.WebSocketServices[path].Sessions[session] as UiClient)?.Id);
                }
            }
        }

        /// <summary>
        /// Removes all clients from the server
        /// </summary>
        public void RemoveAllTokens(string s)
        {
            if (s == "?" || string.IsNullOrEmpty(s))
            {
                CrestronConsole.ConsoleCommandResponse(@"Remove all clients from the server.  To execute add 'confirm' to command");
                return;
            }

            if (s != "confirm")
            {
                CrestronConsole.ConsoleCommandResponse(@"To remove all clients, add 'confirm' to the command");
                return;
            }

            foreach (var client in UiClientContexts)
            {
                if (client.Value.Client != null && client.Value.Client.Context.WebSocket.IsAlive)
                {
                    client.Value.Client.Context.WebSocket.Close(CloseStatusCode.Normal, "Server Shutting Down");
                }

                var path = _wsPath + client.Key;
                if (_server.RemoveWebSocketService(path))
                {
                    CrestronConsole.ConsoleCommandResponse(string.Format("Client removed with token: {0}", client.Key));
                }
                else
                {
                    CrestronConsole.ConsoleCommandResponse(string.Format("Unable to remove client with token : {0}", client.Key));
                }
            }

            UiClientContexts.Clear();

            UpdateSecret();
        }

        /// <summary>
        /// Removes a client with the specified token value
        /// </summary>
        /// <param name="s"></param>
        private void RemoveToken(string s)
        {
            if (s == "?" || string.IsNullOrEmpty(s))
            {
                CrestronConsole.ConsoleCommandResponse(@"[token] Removes the client with the specified token value");
                return;
            }

            var key = s;

            if (UiClientContexts.ContainsKey(key))
            {
                var uiClientContext = UiClientContexts[key];

                if (uiClientContext.Client != null && uiClientContext.Client.Context.WebSocket.IsAlive)
                {
                    uiClientContext.Client.Context.WebSocket.Close(CloseStatusCode.Normal, "Token removed from server");
                }

                var path = _wsPath + key;
                if (_server.RemoveWebSocketService(path))
                {
                    UiClientContexts.Remove(key);

                    UpdateSecret();

                    CrestronConsole.ConsoleCommandResponse(string.Format("Client removed with token: {0}", key));
                }
                else
                {
                    CrestronConsole.ConsoleCommandResponse(string.Format("Unable to remove client with token : {0}", key));
                }
            }
            else
            {
                CrestronConsole.ConsoleCommandResponse(string.Format("Unable to find client with token: {0}", key));
            }
        }

        /// <summary>
        /// Prints out info about current client IDs
        /// </summary>
        private void PrintClientInfo()
        {
            CrestronConsole.ConsoleCommandResponse("Mobile Control UI Client Info:\r");

            CrestronConsole.ConsoleCommandResponse(string.Format("{0} clients found:\r", UiClientContexts.Count));

            foreach (var client in UiClientContexts)
            {
                CrestronConsole.ConsoleCommandResponse(string.Format("RoomKey: {0} Token: {1}\r", client.Value.Token.RoomKey, client.Key));
            }
        }

        private void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
            {
                if (_autoBlockTimer != null)
                {
                    _autoBlockTimer.Stop();
                    _autoBlockTimer.Dispose();
                }

                foreach (var client in UiClients.Values)
                {
                    if (client != null && client.Context.WebSocket.IsAlive)
                    {
                        client.Context.WebSocket.Close(CloseStatusCode.Normal, "Server Shutting Down");
                    }
                }

                StopServer();
            }
        }

        /// <summary>
        /// Adds headers to a response telling the client not to cache it. Some panel browsers hold on to
        /// a cached copy of the app and never re-request it, which leaves them running a stale build.
        /// </summary>
        /// <param name="res">The response to add the headers to</param>
        private static void AddNoCacheHeaders(HttpListenerResponse res)
        {
            res.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            res.AddHeader("Pragma", "no-cache");
            res.AddHeader("Expires", "0");
        }

        /// <summary>
        /// Handler for GET requests to server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_OnGet(object sender, HttpRequestEventArgs e)
        {
            try
            {
                var req = e.Request;
                var res = e.Response;

                if (RejectIfNotAllowed(req, res))
                {
                    return;
                }

                res.ContentEncoding = Encoding.UTF8;

                res.AddHeader("Access-Control-Allow-Origin", "*");

                AddNoCacheHeaders(res);

                var path = req.RawUrl;
                var remote = req.RemoteEndPoint?.Address;

                // Source address included so a scan can be attributed to a host. Path is truncated
                // because a hostile request can carry an arbitrarily long one.
                this.LogVerbose("GET Request received at path: {path} from host {host}", TruncateForLog(path), remote);

                // Call for user app to join the room with a token
                if (path.StartsWith("/mc/api/ui/joinroom"))
                {
                    HandleJoinRequest(req, res);
                }
                // Call to get the server version
                else if (path.StartsWith("/mc/api/version"))
                {
                    HandleVersionRequest(res);
                }
                else if (path.StartsWith("/mc/app/logo"))
                {
                    HandleImageRequest(req, res);
                }
                // Call to serve the user app
                else if (path.StartsWith(_userAppBaseHref))
                {
                    HandleUserAppRequest(req, res, path);
                }
                else
                {
                    // All other paths. Browsers make a couple of these on every page load, so those are neither
                    // logged at Information nor counted towards an automatic block.
                    if (!IsBenignBrowserRequest(path))
                    {
                        LogRateLimited("unrecognised", remote, () =>
                            this.LogInformation("Unrecognised request path from {host}: {path}", remote, TruncateForLog(path)));

                        RecordUnwantedRequest(remote);
                    }

                    if (_parent.Config.DirectServer.DropUnrecognisedRequests == true)
                    {
                        // No reply: nothing legitimate asks for these paths, and a reply is a write that can
                        // fail on a connection the client has already reset
                        DropConnection(res);
                    }
                    else
                    {
                        res.StatusCode = 404;
                        res.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception in OnGet handler: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace");
            }
        }

        private async void Server_OnPost(object sender, HttpRequestEventArgs e)
        {
            try
            {
                var req = e.Request;
                var res = e.Response;

                if (RejectIfNotAllowed(req, res))
                {
                    return;
                }

                res.AddHeader("Access-Control-Allow-Origin", "*");

                AddNoCacheHeaders(res);

                var path = req.RawUrl;
                var remote = req.RemoteEndPoint?.Address;
                var ip = remote?.ToString();

                this.LogVerbose("POST Request received at path: {path} from host {host}", TruncateForLog(path), ip);

                if (path.StartsWith("/mc/api/log"))
                {
                    // The app posts here whether or not forwarding is on, so this is not an unwanted request.
                    // Acknowledged without reading the body, so a large or slow upload costs nothing when it is off.
                    if (!_parent.Config.DirectServer.Logging.EnableRemoteLogging)
                    {
                        res.StatusCode = 200;
                        res.Close();
                        return;
                    }

                    var body = new StreamReader(req.InputStream).ReadToEnd();

                    res.StatusCode = 200;
                    res.Close();

                    // remote log collector has no dedicated secure flag; keep it on http regardless of DirectServer.Secure
                    var logRequest = new HttpRequestMessage(HttpMethod.Post, $"http://{_parent.Config.DirectServer.Logging.Host}:{_parent.Config.DirectServer.Logging.Port}/logs")
                    {
                        Content = new StringContent(body, Encoding.UTF8, "application/json"),
                    };

                    logRequest.Headers.Add("x-pepperdash-host", ip);

                    await LogClient.SendAsync(logRequest);

                    this.LogVerbose("Log data sent to {host}:{port}", _parent.Config.DirectServer.Logging.Host, _parent.Config.DirectServer.Logging.Port);
                }
                else
                {
                    // Treated like an unrecognised GET: logged, counted towards an automatic block, then dropped or 404
                    LogRateLimited("unrecognised", remote, () =>
                        this.LogInformation("Unrecognised POST path from {host}: {path}", remote, TruncateForLog(path)));

                    RecordUnwantedRequest(remote);

                    if (_parent.Config.DirectServer.DropUnrecognisedRequests == true)
                    {
                        DropConnection(res);
                    }
                    else
                    {
                        res.StatusCode = 404;
                        res.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogException(ex, "Caught an exception in the OnPost handler");
            }
        }

        private void Server_OnOptions(object sender, HttpRequestEventArgs e)
        {
            try
            {
                var res = e.Response;

                if (RejectIfNotAllowed(e.Request, res))
                {
                    return;
                }

                res.AddHeader("Access-Control-Allow-Origin", "*");
                res.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                res.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With, remember-me");

                AddNoCacheHeaders(res);

                res.StatusCode = 200;
                res.Close();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Caught an exception in the OnPost handler", this);
            }
        }

        /// <summary>
        /// Handle the request to join the room with a token
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        private void HandleJoinRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            var qp = req.QueryString;
            var token = qp["token"];

            // Each join mints a single-use clientId; the panel webview must never replay a cached
            // response, or it reconnects forever with an already-consumed id (1008 loop).
            res.Headers.Add("Cache-Control", "no-store");
            res.Headers.Add("Pragma", "no-cache");

            this.LogVerbose("Join Room Request with token: {token}", token);

            byte[] body;

            if (!UiClientContexts.TryGetValue(token, out UiClientContext clientContext))
            {
                var message = "Token invalid or has expired";
                res.StatusCode = 401;
                res.ContentType = "application/json";
                this.LogVerbose("{message}", message);
                body = Encoding.UTF8.GetBytes(message);
                res.ContentLength64 = body.LongLength;
                res.Close(body, true);
                return;
            }

            var bridge = _parent.GetRoomBridge(clientContext.Token.RoomKey);

            if (bridge == null)
            {
                var message = string.Format("Unable to find bridge with key: {0}", clientContext.Token.RoomKey);
                res.StatusCode = 404;
                res.ContentType = "application/json";
                this.LogVerbose("{message}", message);
                body = Encoding.UTF8.GetBytes(message);
                res.ContentLength64 = body.LongLength;
                res.Close(body, true);
                return;
            }

            res.StatusCode = 200;
            res.ContentType = "application/json";

            var devices = DeviceManager.GetDevices();
            Dictionary<string, DeviceInterfaceInfo> deviceInterfaces = new Dictionary<string, DeviceInterfaceInfo>();

            foreach (var device in devices)
            {
                var interfaces = device?.GetType().GetInterfaces().Select((i) => i.Name).ToList() ?? new List<string>();

                deviceInterfaces.Add(device.Key, new DeviceInterfaceInfo
                {
                    Key = device.Key,
                    Name = (device as IKeyName)?.Name ?? "",
                    Interfaces = interfaces
                });
            }

            // Generate a client ID for this join request
            var clientId = $"{Utilities.GetNextClientId()}";
            var now = DateTime.UtcNow;

            // Store in pending registrations for new clients that send clientId via query param
            var registrationKey = $"{token}-{clientId}";
            pendingClientRegistrations.TryAdd(registrationKey, clientId);

            // For legacy clients, store with timestamp instead of FIFO queue
            var legacyBag = legacyClientRegistrations.GetOrAdd(token, _ => new ConcurrentBag<(string, DateTime)>());
            legacyBag.Add((clientId, now));

            this.LogVerbose("Assigning ClientId: {clientId} for token: {token} at {timestamp}", clientId, token, now);

            // Construct WebSocket URL with clientId query parameter
            var wsUrl = $"{WsScheme}://{CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0)}:{Port}{_wsPath}{token}?clientId={clientId}";

            // Construct the response object
            JoinResponse jRes = new JoinResponse
            {
                ClientId = clientId,
                RoomKey = bridge.RoomKey,
                SystemUuid = _parent.SystemUuid,
                RoomUuid = _parent.SystemUuid,
                Config = _parent.GetConfigWithPluginVersion(),
                CodeExpires = new DateTime().AddYears(1),
                UserCode = bridge.UserCode,
                UserAppUrl = string.Format("{0}://{1}:{2}/mc/app",
                HttpScheme,
                CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0),
                Port),
                WebSocketUrl = wsUrl,
                EnableDebug = false,
                DeviceInterfaceSupport = deviceInterfaces
            };

            // Serialize to JSON and convert to Byte[]
            var json = JsonConvert.SerializeObject(jRes);
            body = Encoding.UTF8.GetBytes(json);
            res.ContentLength64 = body.LongLength;

            // Send the response
            res.Close(body, true);
        }

        /// <summary>
        /// Handles a server version request
        /// </summary>
        /// <param name="res"></param>
        private void HandleVersionRequest(HttpListenerResponse res)
        {
            res.StatusCode = 200;
            res.ContentType = "application/json";
            res.Headers.Add("Cache-Control", "no-store");
            res.Headers.Add("Pragma", "no-cache");
            var version = new Version() { ServerVersion = _parent.GetConfigWithPluginVersion().RuntimeInfo.PluginVersion };
            var message = JsonConvert.SerializeObject(version);
            this.LogVerbose("{message}", message);

            var body = Encoding.UTF8.GetBytes(message);
            res.ContentLength64 = body.LongLength;
            res.Close(body, true);
        }

        /// <summary>
        /// Handler to return images requested by the user app
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        private void HandleImageRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            var path = req.RawUrl;

            Debug.LogMessage(LogEventLevel.Verbose, "Requesting Image: {0}", this, path);

            var imageBasePath = Global.DirectorySeparator + "html" + Global.DirectorySeparator + "logo" + Global.DirectorySeparator;

            var image = path.Split('/').Last();

            var filePath = imageBasePath + image;

            Debug.LogMessage(LogEventLevel.Verbose, "Retrieving Image: {0}", this, filePath);

            if (File.Exists(filePath))
            {
                if (filePath.EndsWith(".png"))
                {
                    res.ContentType = "image/png";
                }
                else if (filePath.EndsWith(".jpg"))
                {
                    res.ContentType = "image/jpeg";
                }
                else if (filePath.EndsWith(".gif"))
                {
                    res.ContentType = "image/gif";
                }
                else if (filePath.EndsWith(".svg"))
                {
                    res.ContentType = "image/svg+xml";
                }
                byte[] contents = File.ReadAllBytes(filePath);
                res.ContentLength64 = contents.LongLength;
                res.Close(contents, true);
            }
            else
            {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                res.Close();
            }
        }

        /// <summary>
        /// Handles requests to serve files for the Angular single page app
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        /// <param name="path"></param>
        private void HandleUserAppRequest(HttpListenerRequest req, HttpListenerResponse res, string path)
        {
            this.LogVerbose("Requesting User app file");

            string filePath = path.Split('?')[0];

            // remove the token from the path if found
            //string filePath = path.Replace(string.Format("?token={0}", token), "");

            // if there's no file suffix strip any extra path data after the base href
            // Note: this used to be `_userAppBaseHref += "/"` inside the condition, which silently appended a
            // slash to the shared field the first time it was evaluated and changed every later request's
            // path handling. Compare against a copy instead.
            if (filePath != _userAppBaseHref && !filePath.Contains(".") && (!filePath.EndsWith(_userAppBaseHref) || !filePath.EndsWith(_userAppBaseHref + "/")))
            {
                var suffix = filePath.Substring(_userAppBaseHref.Length, filePath.Length - _userAppBaseHref.Length);
                if (suffix != "/")
                {
                    //Debug.Console(2, this, "Suffix: {0}", suffix);
                    filePath = filePath.Replace(suffix, "");
                }
            }

            // swap the base href prefix for the file path prefix
            filePath = filePath.Replace(_userAppBaseHref, _appPath);

            this.LogVerbose("filepath: {filePath}", filePath);


            // append index.html if no specific file is specified
            if (!filePath.Contains("."))
            {
                if (filePath.EndsWith("/"))
                {
                    filePath += "index.html";
                }
                else
                {
                    filePath += "/index.html";
                }
            }

            // Set ContentType based on file type
            if (filePath.EndsWith(".html"))
            {
                this.LogVerbose("Client requesting User App");

                res.ContentType = "text/html";
            }
            else
            {
                if (path.EndsWith(".js"))
                {
                    res.ContentType = "application/javascript";
                }
                else if (path.EndsWith(".css"))
                {
                    res.ContentType = "text/css";
                }
                else if (path.EndsWith(".json"))
                {
                    res.ContentType = "application/json";
                }
            }

            this.LogVerbose("Attempting to serve file: {filePath}", filePath);

            var remoteIp = req.RemoteEndPoint.Address;

            // Check if the request is coming from the CS LAN and if so, send the CS config instead of the LAN config
            if (csSubnetMask != null && csIpAddress != null && remoteIp.IsInSameSubnet(csIpAddress, csSubnetMask) && filePath.Contains(appConfigFileName))
            {
                filePath = filePath.Replace(appConfigFileName, appConfigCsFileName);
            }

            byte[] contents;
            if (File.Exists(filePath))
            {
                this.LogVerbose("File found: {filePath}", filePath);
                contents = File.ReadAllBytes(filePath);
            }
            else
            {
                this.LogWarning("File not found: {filePath}", filePath);
                res.StatusCode = (int)HttpStatusCode.NotFound;
                res.Close();
                return;
            }

            res.ContentLength64 = contents.LongLength;
            res.Close(contents, true);
        }

        /// <summary>
        /// StopServer method
        /// </summary>
        public void StopServer()
        {
            this.LogVerbose("Stopping WebSocket Server");
            _server.Stop(CloseStatusCode.Normal, "Server Shutting Down");
        }

        /// <summary>
        /// Sends a message to all connectd clients
        /// </summary>
        /// <param name="message"></param>
        /// <summary>
        /// SendMessageToAllClients method
        /// </summary>
        public void SendMessageToAllClients(string message)
        {
            foreach (var client in uiClients.Values)
            {
                if (!client.Context.WebSocket.IsAlive)
                {
                    continue;
                }

                client.Context.WebSocket.Send(message);
            }
        }

        /// <summary>
        /// Sends a message to a specific client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="message"></param>
        /// <summary>
        /// SendMessageToClient method
        /// </summary>
        public void SendMessageToClient(object clientId, string message)
        {
            if (clientId == null)
            {
                return;
            }

            if (uiClients.TryGetValue((string)clientId, out var client))
            {
                var socket = client.Context.WebSocket;

                if (!socket.IsAlive)
                {
                    this.LogError("Unable to send message to client {id}. Client is disconnected: {message}", clientId, message);
                    return;
                }
                socket.Send(message);
            }
            else
            {
                this.LogWarning("Unable to find client with ID: {clientId}", clientId);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a MobileControlConfig
    /// </summary>
    public class MobileControlConfig
    {
        /// <summary>
        /// Gets or sets the ServerUrl
        /// </summary>
        [JsonProperty("serverUrl")]
        public string ServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the ClientAppUrl
        /// </summary>
        [JsonProperty("clientAppUrl")]
        public string ClientAppUrl { get; set; }

        /// <summary>
        /// Gets or sets the DirectServer
        /// </summary>
        [JsonProperty("directServer")]
        public MobileControlDirectServerPropertiesConfig DirectServer { get; set; }

        /// <summary>
        /// Gets or sets the ApplicationConfig
        /// </summary>
        [JsonProperty("applicationConfig")]
        public MobileControlApplicationConfig ApplicationConfig { get; set; } = null;

        /// <summary>
        /// Gets or sets the EnableApiServer
        /// </summary>
        [JsonProperty("enableApiServer")]
        public bool EnableApiServer { get; set; } = true;

        /// <summary>
        /// Enables subscriptions for messengers
        /// </summary>
        [JsonProperty("enableMessengerSubscriptions")]
        [Obsolete("This property is obsolete and will be removed in a future version. All messengers are now subscription based.")]
        public bool EnableMessengerSubscriptions { get; set; }
    }

    /// <summary>
    /// Represents a MobileControlDirectServerPropertiesConfig
    /// </summary>
    public class MobileControlDirectServerPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the EnableDirectServer
        /// </summary>
        [JsonProperty("enableDirectServer")]
        public bool EnableDirectServer { get; set; }

        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the Logging
        /// </summary>
        [JsonProperty("logging")]
        public MobileControlLoggingConfig Logging { get; set; }

        /// <summary>
        /// Gets or sets the AutomaticallyForwardPortToCSLAN
        /// </summary>
        [JsonProperty("automaticallyForwardPortToCSLAN")]
        public bool? AutomaticallyForwardPortToCSLAN { get; set; }

        /// <summary>
        /// Gets or sets the networks (CIDR notation) allowed to make HTTP requests to the direct server
        /// </summary>
        /// <remarks>
        /// Example: ["192.168.10.0/24", "192.168.5.10/32"]. When the list has any entries, HTTP requests
        /// (GET, POST, OPTIONS) from any other address have the connection closed without a response,
        /// except loopback and clients on the Control Subnet, which are always allowed.
        /// When null or empty, no filtering is done (default).
        /// Invalid entries are logged and skipped, so a list containing only invalid entries still turns
        /// filtering on. Does not apply to websocket connections, which are already gated by a per-client token.
        /// </remarks>
        [JsonProperty("allowedClientNetworks")]
        public List<string> AllowedClientNetworks { get; set; }

        /// <summary>
        /// Gets or sets whether a request for a path the server does not handle gets no reply
        /// </summary>
        /// <remarks>
        /// When true the connection is closed without a response. When false or absent (default) the server
        /// replies 404, as it always has. Replying means writing to a connection the client may already have
        /// reset, which throws from inside the HTTP stack, so noisy environments may prefer true.
        /// </remarks>
        [JsonProperty("dropUnrecognisedRequests")]
        public bool? DropUnrecognisedRequests { get; set; }

        /// <summary>
        /// Gets or sets the automatic blocking of addresses that send a burst of unwanted requests
        /// </summary>
        /// <remarks>
        /// Absent or "enabled": false (default) means no automatic blocking.
        /// </remarks>
        [JsonProperty("autoBlock")]
        public MobileControlAutoBlockConfig AutoBlock { get; set; }

        /// <summary>
        /// Gets or sets the CSLanUiDeviceKeys
        /// </summary>
        /// <remarks>
        /// A list of device keys for the CS LAN UI. These devices will get the CS LAN IP address instead of the LAN IP Address
        /// </remarks>
        [JsonProperty("csLanUiDeviceKeys")]
        public List<string> CSLanUiDeviceKeys { get; set; }

        /// <summary>
        /// Get or set the Secure property
        /// </summary>
        /// <remarks>
        /// Indicates whether the connection is secure (HTTPS).
        /// </remarks>
        [JsonProperty("Secure")]
        public bool Secure { get; set; }

        /// <summary>
        /// Initializes a new instance of the MobileControlDirectServerPropertiesConfig class.
        /// </summary>
        public MobileControlDirectServerPropertiesConfig()
        {
            Logging = new MobileControlLoggingConfig();
        }
    }

    /// <summary>
    /// Represents a MobileControlLoggingConfig
    /// </summary>
    /// <summary>
    /// Settings for blocking, at the processor, an address that sends a burst of unwanted requests
    /// </summary>
    /// <remarks>
    /// Counts requests for unrecognised paths and requests refused by allowedClientNetworks. When one address
    /// reaches requestsPerMinute within a minute it is added to the processor's blocked-IP list (a total block,
    /// every port) and removed again after blockMinutes. The processor's own lockout setting does not apply to
    /// manual blocks, so Essentials removes only the blocks it added. 4-series appliances only.
    /// Never blocks loopback, the Control Subnet, the processor's own addresses, allowedClientNetworks or neverBlock.
    /// </remarks>
    public class MobileControlAutoBlockConfig
    {
        /// <summary>
        /// Gets or sets whether automatic blocking is on (default false)
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets whether to only log what would be blocked, without blocking anything
        /// </summary>
        [JsonProperty("dryRun")]
        public bool DryRun { get; set; }

        /// <summary>
        /// Gets or sets how many unwanted requests from one address within a minute trigger a block (default 10, minimum 3)
        /// </summary>
        [JsonProperty("requestsPerMinute")]
        public int RequestsPerMinute { get; set; } = 10;

        /// <summary>
        /// Gets or sets how long a block lasts, in minutes (default 30, 1 to 1440)
        /// </summary>
        [JsonProperty("blockMinutes")]
        public int BlockMinutes { get; set; } = 30;

        /// <summary>
        /// Gets or sets the most blocks Essentials will hold at once (default 8, 1 to 64)
        /// </summary>
        [JsonProperty("maxConcurrentBlocks")]
        public int MaxConcurrentBlocks { get; set; } = 8;

        /// <summary>
        /// Gets or sets networks (CIDR notation) that are never blocked, for example VPN and monitoring hosts
        /// </summary>
        [JsonProperty("neverBlock")]
        public List<string> NeverBlock { get; set; }
    }

    public class MobileControlLoggingConfig
    {

        /// <summary>
        /// Gets or sets the EnableRemoteLogging
        /// </summary>
        [JsonProperty("enableRemoteLogging")]
        public bool EnableRemoteLogging { get; set; }


        /// <summary>
        /// Gets or sets the Host
        /// </summary>
        [JsonProperty("host")]
        public string Host { get; set; }


        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }
    }

    /// <summary>
    /// Represents a MobileControlRoomBridgePropertiesConfig
    /// </summary>
    public class MobileControlRoomBridgePropertiesConfig
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the RoomKey
        /// </summary>
        [JsonProperty("roomKey")]
        public string RoomKey { get; set; }
    }

    /// <summary>
    /// Represents a MobileControlSimplRoomBridgePropertiesConfig
    /// </summary>
    public class MobileControlSimplRoomBridgePropertiesConfig
    {
        /// <summary>
        /// Gets or sets the EiscId
        /// </summary>
        [JsonProperty("eiscId")]
        public string EiscId { get; set; }
    }

    /// <summary>
    /// Represents a MobileControlApplicationConfig
    /// </summary>
    public class MobileControlApplicationConfig
    {
        /// <summary>
        /// Gets or sets the ApiPath
        /// </summary>
        [JsonProperty("apiPath")]
        public string ApiPath { get; set; }

        /// <summary>
        /// Gets or sets the GatewayAppPath
        /// </summary>
        [JsonProperty("gatewayAppPath")]
        public string GatewayAppPath { get; set; }

        /// <summary>
        /// Gets or sets the EnableDev
        /// </summary>
        [JsonProperty("enableDev")]
        public bool? EnableDev { get; set; }

        /// <summary>
        /// Gets or sets the LogoPath
        /// </summary>
        [JsonProperty("logoPath")]
        public string LogoPath { get; set; }

        /// <summary>
        /// Gets or sets the IconSet
        /// </summary>
        [JsonProperty("iconSet")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MCIconSet? IconSet { get; set; }

        /// <summary>
        /// Gets or sets the LoginMode
        /// </summary>
        [JsonProperty("loginMode")]
        public string LoginMode { get; set; }

        /// <summary>
        /// Gets or sets the Modes
        /// </summary>
        [JsonProperty("modes")]
        public Dictionary<string, McMode> Modes { get; set; }

        /// <summary>
        /// Gets or sets the Logging
        /// </summary>
        [JsonProperty("enableRemoteLogging")]
        public bool Logging { get; set; }

        /// <summary>
        /// Gets or sets the PartnerMetadata
        /// </summary>
        [JsonProperty("partnerMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public List<MobileControlPartnerMetadata> PartnerMetadata { get; set; }
    }

    /// <summary>
    /// Represents a MobileControlPartnerMetadata
    /// </summary>
    public class MobileControlPartnerMetadata
    {
        /// <summary>
        /// Gets or sets the Role
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the LogoPath
        /// </summary>
        [JsonProperty("logoPath")]
        public string LogoPath { get; set; }
    }

    /// <summary>
    /// Represents a McMode
    /// </summary>
    public class McMode
    {
        /// <summary>
        /// Gets or sets the ListPageText
        /// </summary>
        [JsonProperty("listPageText")]
        public string ListPageText { get; set; }

        /// <summary>
        /// Gets or sets the LoginHelpText
        /// </summary>
        [JsonProperty("loginHelpText")]
        public string LoginHelpText { get; set; }

        /// <summary>
        /// Gets or sets the PasscodePageText
        /// </summary>
        [JsonProperty("passcodePageText")]
        public string PasscodePageText { get; set; }
    }

    /// <summary>
    /// Enumeration of MCIconSet values
    /// </summary>
    public enum MCIconSet
    {
        /// <summary>
        /// Google icon set
        /// </summary>
        GOOGLE,

        /// <summary>
        /// Habanero icon set
        /// </summary>
        HABANERO,

        /// <summary>
        /// Neo icon set
        /// </summary>
        NEO
    }
}

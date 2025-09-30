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
        /// Enable subscriptions for Messengers
        /// </summary>
        [JsonProperty("enableMessengerSubscriptions")]
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
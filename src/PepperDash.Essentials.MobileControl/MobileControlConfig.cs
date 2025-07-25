using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a MobileControlConfig
    /// </summary>
    public class MobileControlConfig
    {
        [JsonProperty("serverUrl")]
        public string ServerUrl { get; set; }

        [JsonProperty("clientAppUrl")]
        public string ClientAppUrl { get; set; }

        [JsonProperty("directServer")]
        public MobileControlDirectServerPropertiesConfig DirectServer { get; set; }

        [JsonProperty("applicationConfig")]
        /// <summary>
        /// Gets or sets the ApplicationConfig
        /// </summary>
        public MobileControlApplicationConfig ApplicationConfig { get; set; } = null;

        [JsonProperty("enableApiServer")]
        /// <summary>
        /// Gets or sets the EnableApiServer
        /// </summary>
        public bool EnableApiServer { get; set; } = true;
    }

    /// <summary>
    /// Represents a MobileControlDirectServerPropertiesConfig
    /// </summary>
    public class MobileControlDirectServerPropertiesConfig
    {
        [JsonProperty("enableDirectServer")]
        /// <summary>
        /// Gets or sets the EnableDirectServer
        /// </summary>
        public bool EnableDirectServer { get; set; }

        [JsonProperty("port")]
        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public int Port { get; set; }

        [JsonProperty("logging")]
        /// <summary>
        /// Gets or sets the Logging
        /// </summary>
        public MobileControlLoggingConfig Logging { get; set; }

        [JsonProperty("automaticallyForwardPortToCSLAN")]
        public bool? AutomaticallyForwardPortToCSLAN { get; set; }

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
        [JsonProperty("enableRemoteLogging")]
        /// <summary>
        /// Gets or sets the EnableRemoteLogging
        /// </summary>
        public bool EnableRemoteLogging { get; set; }

        [JsonProperty("host")]
        /// <summary>
        /// Gets or sets the Host
        /// </summary>
        public string Host { get; set; }

        [JsonProperty("port")]
        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public int Port { get; set; }



    }

    /// <summary>
    /// Represents a MobileControlRoomBridgePropertiesConfig
    /// </summary>
    public class MobileControlRoomBridgePropertiesConfig
    {
        [JsonProperty("key")]
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }

        [JsonProperty("roomKey")]
        /// <summary>
        /// Gets or sets the RoomKey
        /// </summary>
        public string RoomKey { get; set; }
    }

    /// <summary>
    /// Represents a MobileControlSimplRoomBridgePropertiesConfig
    /// </summary>
    public class MobileControlSimplRoomBridgePropertiesConfig
    {
        [JsonProperty("eiscId")]
        /// <summary>
        /// Gets or sets the EiscId
        /// </summary>
        public string EiscId { get; set; }
    }

    public class MobileControlApplicationConfig
    {
        [JsonProperty("apiPath")]
        public string ApiPath { get; set; }

        [JsonProperty("gatewayAppPath")]
        /// <summary>
        /// Gets or sets the GatewayAppPath
        /// </summary>
        public string GatewayAppPath { get; set; }

        [JsonProperty("enableDev")]
        public bool? EnableDev { get; set; }

        [JsonProperty("logoPath")]
        /// <summary>
        /// Gets or sets the LogoPath
        /// </summary>
        public string LogoPath { get; set; }

        [JsonProperty("iconSet")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MCIconSet? IconSet { get; set; }

        [JsonProperty("loginMode")]
        public string LoginMode { get; set; }

        [JsonProperty("modes")]
        public Dictionary<string, McMode> Modes { get; set; }

        [JsonProperty("enableRemoteLogging")]
        /// <summary>
        /// Gets or sets the Logging
        /// </summary>
        public bool Logging { get; set; }

        [JsonProperty("partnerMetadata", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the PartnerMetadata
        /// </summary>
        public List<MobileControlPartnerMetadata> PartnerMetadata { get; set; }
    }

    /// <summary>
    /// Represents a MobileControlPartnerMetadata
    /// </summary>
    public class MobileControlPartnerMetadata
    {
        [JsonProperty("role")]
        /// <summary>
        /// Gets or sets the Role
        /// </summary>
        public string Role { get; set; }

        [JsonProperty("description")]
        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }

        [JsonProperty("logoPath")]
        /// <summary>
        /// Gets or sets the LogoPath
        /// </summary>
        public string LogoPath { get; set; }
    }

    /// <summary>
    /// Represents a McMode
    /// </summary>
    public class McMode
    {
        [JsonProperty("listPageText")]
        /// <summary>
        /// Gets or sets the ListPageText
        /// </summary>
        public string ListPageText { get; set; }
        [JsonProperty("loginHelpText")]
        /// <summary>
        /// Gets or sets the LoginHelpText
        /// </summary>
        public string LoginHelpText { get; set; }

        [JsonProperty("passcodePageText")]
        /// <summary>
        /// Gets or sets the PasscodePageText
        /// </summary>
        public string PasscodePageText { get; set; }
    }

    /// <summary>
    /// Enumeration of MCIconSet values
    /// </summary>
    public enum MCIconSet
    {
        GOOGLE,
        HABANERO,
        NEO
    }
}
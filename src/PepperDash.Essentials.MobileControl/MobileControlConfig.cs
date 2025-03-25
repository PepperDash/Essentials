using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public class MobileControlConfig
    {
        [JsonProperty("serverUrl")]
        public string ServerUrl { get; set; }

        [JsonProperty("clientAppUrl")]
        public string ClientAppUrl { get; set; }

#if SERIES4
        [JsonProperty("directServer")]
        public MobileControlDirectServerPropertiesConfig DirectServer { get; set; }

        [JsonProperty("applicationConfig")]
        public MobileControlApplicationConfig ApplicationConfig { get; set; }

        [JsonProperty("enableApiServer")]
        public bool EnableApiServer { get; set; }
#endif

        [JsonProperty("roomBridges")]
        [Obsolete("No longer necessary")]
        public List<MobileControlRoomBridgePropertiesConfig> RoomBridges { get; set; }

        public MobileControlConfig()
        {
            RoomBridges = new List<MobileControlRoomBridgePropertiesConfig>();

#if SERIES4
            EnableApiServer = true; // default to true
            ApplicationConfig = null;
#endif
        }
    }

    public class MobileControlDirectServerPropertiesConfig
    {
        [JsonProperty("enableDirectServer")]
        public bool EnableDirectServer { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("logging")]
        public MobileControlLoggingConfig Logging { get; set; }

        [JsonProperty("automaticallyForwardPortToCSLAN")]
        public bool? AutomaticallyForwardPortToCSLAN { get; set; }

        public MobileControlDirectServerPropertiesConfig()
        {
            Logging = new MobileControlLoggingConfig();
        }
    }

    public class MobileControlLoggingConfig
    {
        [JsonProperty("enableRemoteLogging")]
        public bool EnableRemoteLogging { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        

    }

    public class MobileControlRoomBridgePropertiesConfig
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("roomKey")]
        public string RoomKey { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MobileControlSimplRoomBridgePropertiesConfig
    {
        [JsonProperty("eiscId")]
        public string EiscId { get; set; }
    }

    public class MobileControlApplicationConfig
    {
        [JsonProperty("apiPath")]
        public string ApiPath { get; set; }

        [JsonProperty("gatewayAppPath")]
        public string GatewayAppPath { get; set; }

        [JsonProperty("enableDev")]
        public bool? EnableDev { get; set; }

        [JsonProperty("logoPath")]
        /// <summary>
        /// Client logo to be used in header and/or splash screen
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
        public bool Logging { get; set; }

        [JsonProperty("partnerMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public List<MobileControlPartnerMetadata> PartnerMetadata { get; set; }
    }

    public class MobileControlPartnerMetadata
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("logoPath")]
        public string LogoPath { get; set; }
    }

    public class McMode
    {
        [JsonProperty("listPageText")]
        public string ListPageText { get; set; }
        [JsonProperty("loginHelpText")]
        public string LoginHelpText { get; set; }

        [JsonProperty("passcodePageText")]
        public string PasscodePageText { get; set; }
    }

    public enum MCIconSet
    {
        GOOGLE,
        HABANERO,
        NEO
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials
{
    /// <summary>
    /// Configuration class for sending data to Mobile Control Edge or a client using the Direct Server
    /// </summary>
    public class MobileControlEssentialsConfig : EssentialsConfig
    {
        /// <summary>
        /// Current versions for the system
        /// </summary>
        [JsonProperty("runtimeInfo")]
        public MobileControlRuntimeInfo RuntimeInfo { get; set; }

        /// <summary>
        /// Create Configuration for Mobile Control. Used as part of the data sent to a client
        /// </summary>
        /// <param name="config">The base configuration</param>
        public MobileControlEssentialsConfig(EssentialsConfig config)
            : base()
        {
            Devices = config.Devices;
            Info = config.Info;
            JoinMaps = config.JoinMaps;
            Rooms = config.Rooms;
            SourceLists = config.SourceLists;
            DestinationLists = config.DestinationLists;
            SystemUrl = config.SystemUrl;
            TemplateUrl = config.TemplateUrl;
            TieLines = config.TieLines;

            if (Info == null)
                Info = new InfoConfig();

            RuntimeInfo = new MobileControlRuntimeInfo();
        }
    }

    /// <summary>
    /// Represents a MobileControlRuntimeInfo
    /// </summary>
    public class MobileControlRuntimeInfo
    {

        /// <summary>
        /// Gets or sets the PluginVersion
        /// </summary>
        [JsonProperty("pluginVersion")]
        public string PluginVersion { get; set; }

        /// <summary>
        /// Essentials Version
        /// </summary>
        [JsonProperty("essentialsVersion")]
        public string EssentialsVersion { get; set; }

        /// <summary>
        /// PepperDash Core Version
        /// </summary>
        [JsonProperty("pepperDashCoreVersion")]
        public string PepperDashCoreVersion { get; set; }


        /// <summary>
        /// List of Plugins loaded on this system
        /// </summary>
        [JsonProperty("essentialsPlugins")]
        public List<LoadedAssembly> EssentialsPlugins { get; set; }
    }
}
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;
using System.Collections.Generic;


namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a MobileControlEssentialsConfig
    /// </summary>
    public class MobileControlEssentialsConfig : EssentialsConfig
    {
        [JsonProperty("runtimeInfo")]
        public MobileControlRuntimeInfo RuntimeInfo { get; set; }

        public MobileControlEssentialsConfig(EssentialsConfig config)
            : base()
        {
            // TODO: Consider using Reflection to iterate properties
            this.Devices = config.Devices;
            this.Info = config.Info;
            this.JoinMaps = config.JoinMaps;
            this.Rooms = config.Rooms;
            this.SourceLists = config.SourceLists;
            this.DestinationLists = config.DestinationLists;
            this.SystemUrl = config.SystemUrl;
            this.TemplateUrl = config.TemplateUrl;
            this.TieLines = config.TieLines;

            if (this.Info == null)
                this.Info = new InfoConfig();

            RuntimeInfo = new MobileControlRuntimeInfo();
        }
    }

    /// <summary>
    /// Represents a MobileControlRuntimeInfo
    /// </summary>
    public class MobileControlRuntimeInfo
    {
        [JsonProperty("pluginVersion")]
        /// <summary>
        /// Gets or sets the PluginVersion
        /// </summary>
        public string PluginVersion { get; set; }

        [JsonProperty("essentialsVersion")]
        public string EssentialsVersion { get; set; }

        [JsonProperty("pepperDashCoreVersion")]
        public string PepperDashCoreVersion { get; set; }

        [JsonProperty("essentialsPlugins")]
        /// <summary>
        /// Gets or sets the EssentialsPlugins
        /// </summary>
        public List<LoadedAssembly> EssentialsPlugins { get; set; }
    }
}
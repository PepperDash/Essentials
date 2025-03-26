using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;
using System.Collections.Generic;


namespace PepperDash.Essentials
{
    /// <summary>
    /// Used to overlay additional config data from mobile control on
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
    /// Used to add any additional runtime information from mobile control to be send to the API
    /// </summary>
    public class MobileControlRuntimeInfo
    {
        [JsonProperty("pluginVersion")]
        public string PluginVersion { get; set; }

        [JsonProperty("essentialsVersion")]
        public string EssentialsVersion { get; set; }

        [JsonProperty("pepperDashCoreVersion")]
        public string PepperDashCoreVersion { get; set; }

        [JsonProperty("essentialsPlugins")]
        public List<LoadedAssembly> EssentialsPlugins { get; set; }
    }
}
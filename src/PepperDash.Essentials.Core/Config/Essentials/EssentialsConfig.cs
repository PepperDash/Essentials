

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Config
{
	/// <summary>
	/// Loads the ConfigObject from the file
	/// </summary>
	public class EssentialsConfig : BasicConfig
	{
        /// <summary>
        /// Gets or sets the SystemUrl
        /// </summary>
        [JsonProperty("system_url")]
        public string SystemUrl { get; set; }

        /// <summary>
        /// Gets or sets the TemplateUrl
        /// </summary>
        [JsonProperty("template_url")]
        public string TemplateUrl { get; set; }

        /// <summary>
        /// Gets the SystemUuid extracted from the SystemUrl
        /// </summary>
		[JsonProperty("systemUuid")]
		public string SystemUuid
        {
            get
            {
				if (string.IsNullOrEmpty(SystemUrl))
					return "missing url";

				if (SystemUrl.Contains("#"))
				{
					var result = Regex.Match(SystemUrl, @"https?:\/\/.*\/systems\/(.*)\/#.*");
					string uuid = result.Groups[1].Value;
					return uuid;
				} else
				{
                    var result = Regex.Match(SystemUrl, @"https?:\/\/.*\/systems\/(.*)\/.*");
                    string uuid = result.Groups[1].Value;
                    return uuid;
                }
            }
        }

        /// <summary>
        /// Gets the TemplateUuid extracted from the TemplateUrl
        /// </summary>
		[JsonProperty("templateUuid")]
		public string TemplateUuid
        {
            get
            {
				if (string.IsNullOrEmpty(TemplateUrl))
					return "missing template url";

				if (TemplateUrl.Contains("#"))
				{
					var result = Regex.Match(TemplateUrl, @"https?:\/\/.*\/templates\/(.*)\/#.*");
					string uuid = result.Groups[1].Value;
					return uuid;
				} else
				{
                    var result = Regex.Match(TemplateUrl, @"https?:\/\/.*\/system-templates\/(.*)\/system-template-versions\/(.*)\/.*");
                    string uuid = result.Groups[2].Value;
                    return uuid;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Rooms
        /// </summary>
        [JsonProperty("rooms")]

        public List<DeviceConfig> Rooms { get; set; }

        /// <summary>
        /// Gets or sets the Versions
        /// </summary>
        public VersionData Versions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EssentialsConfig"/> class.
        /// </summary>
        public EssentialsConfig()
            : base()
        {
            Rooms = new List<DeviceConfig>();
        }
	}

    /// <summary>
    /// Represents version data for Essentials and its packages
    /// </summary>
    public class VersionData
    {
        /// <summary>
        /// Gets or sets the Essentials version
        /// </summary>
        [JsonProperty("essentials")]
        public NugetVersion Essentials { get; set; }

        /// <summary>
        /// Gets or sets the list of Packages
        /// </summary>
        [JsonProperty("packages")]
        public List<NugetVersion> Packages { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionData"/> class.
        /// </summary>
        public VersionData()
        {
            Packages = new List<NugetVersion>();
        }
    }

    /// <summary>
    /// Represents a NugetVersion
    /// </summary>
    public class NugetVersion
    {
        /// <summary>
        /// Gets or sets the Version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the PackageId
        /// </summary>
        [JsonProperty("packageId")]
        public string PackageId { get; set; }
    }

    /// <summary>
    /// Represents a SystemTemplateConfigs
    /// </summary>
    public class SystemTemplateConfigs
	{
        /// <summary>
        /// Gets or sets the System
        /// </summary>
		public EssentialsConfig System { get; set; }

        /// <summary>
        /// Gets or sets the Template
        /// </summary>
		public EssentialsConfig Template { get; set; }
	}
}
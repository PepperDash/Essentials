using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Config.Essentials
{
	/// <summary>
	/// Loads the ConfigObject from the file
	/// </summary>
	public class EssentialsConfig : BasicConfig
	{
		[JsonProperty("system_url")]
        public string SystemUrl { get; set; }

		[JsonProperty("template_url")]
        public string TemplateUrl { get; set; }


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
					var uuid = result.Groups[1].Value;
					return uuid;
				} else
				{
                    var result = Regex.Match(SystemUrl, @"https?:\/\/.*\/systems\/(.*)\/.*");
                    var uuid = result.Groups[1].Value;
                    return uuid;
                }
            }
        }

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
					var uuid = result.Groups[1].Value;
					return uuid;
				} else
				{
                    var result = Regex.Match(TemplateUrl, @"https?:\/\/.*\/system-templates\/(.*)\/system-template-versions\/(.*)\/.*");
                    var uuid = result.Groups[2].Value;
                    return uuid;
                }
            }
        }

		[JsonProperty("rooms")]
        /// <summary>
        /// Gets or sets the Rooms
        /// </summary>
        public List<DeviceConfig> Rooms { get; set; }


        public EssentialsConfig()
            : base()
        {
            Rooms = new List<DeviceConfig>();
        }
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

		public EssentialsConfig Template { get; set; }
	}
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Config;

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
				string uuid = result.Groups[1].Value;
				return uuid;
			}
			else
			{
				var result = Regex.Match(SystemUrl, @"https?:\/\/.*\/systems\/(.*)\/.*");
				string uuid = result.Groups[1].Value;
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
				string uuid = result.Groups[1].Value;
				return uuid;
			}
			else
			{
				var result = Regex.Match(TemplateUrl, @"https?:\/\/.*\/system-templates\/(.*)\/system-template-versions\/(.*)\/.*");
				string uuid = result.Groups[2].Value;
				return uuid;
			}
		}
	}

	[JsonProperty("rooms")]
	public List<DeviceConfig> Rooms { get; set; }


	public EssentialsConfig()
			: base()
	{
		Rooms = new List<DeviceConfig>();
	}
}

/// <summary>
/// 
/// </summary>
public class SystemTemplateConfigs
{
	public EssentialsConfig System { get; set; }

	public EssentialsConfig Template { get; set; }
}
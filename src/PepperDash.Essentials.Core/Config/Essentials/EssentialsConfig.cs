

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Config;

/// <summary>
/// Loads the ConfigObject from the file
/// </summary>
public class EssentialsConfig : BasicConfig
{
    /// <summary>
    /// Gets or sets the System URL for the Essentials configuration
    /// </summary>
    [JsonProperty("system_url")]
    public string SystemUrl { get; set; }

    /// <summary>
    /// Gets or sets the Template URL for the Essentials configuration
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
            }
            else
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
            }
            else
            {
                var result = Regex.Match(TemplateUrl, @"https?:\/\/.*\/system-templates\/(.*)\/system-template-versions\/(.*)\/.*");
                string uuid = result.Groups[2].Value;
                return uuid;
            }
        }
    }

    /// <summary>
    /// Gets or sets the list of rooms (device configurations)
    /// </summary>
    [JsonProperty("rooms")]
    public List<DeviceConfig> Rooms { get; set; }

    /// <summary>
    /// Gets or sets the Versions
    /// </summary>
    [JsonProperty("versions")]
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
    public PackageVersion Essentials { get; set; }

    /// <summary>
    /// Gets or sets the list of UserInterfaces
    /// </summary>
    [JsonProperty("userInterfaces")]
    public List<PackageVersion> UserInterfaces { get; set; }

    /// <summary>
    /// Gets or sets the TouchpanelWrapperApp version
    /// This is the .ch5z app that gets loaded to a Crestron touch panel that allows it 
    /// to run an HTML5 user interface.
    /// </summary>
    [JsonProperty("touchpanelWrapperApp")]
    public PackageVersion TouchpanelWrapperApp { get; set; }

    /// <summary>
    /// Gets or sets the list of Packages
    /// </summary>
    [JsonProperty("packages")]
    public List<PackageVersion> Packages { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionData"/> class.
    /// </summary>
    public VersionData()
    {
        UserInterfaces = new List<PackageVersion>();
        TouchpanelWrapperApp = new PackageVersion();
        Packages = new List<PackageVersion>();
    }
}

/// <summary>
/// Represents a Package Version
/// </summary>
public class PackageVersion
{
    /// <summary>
    /// Gets or sets the Version
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the PackageId
    /// </summary>
    [JsonProperty("packageId", NullValueHandling = NullValueHandling.Ignore)]
    public string PackageId { get; set; }

    /// <summary>
    /// Gets or sets the RepoUrl
    /// </summary>
    [JsonProperty("repoUrl", NullValueHandling = NullValueHandling.Ignore)]
    public string RepoUrl { get; set; }

    /// <summary>
    /// Gets or sets the human-readable name
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
}

/// <summary>
/// Represents the configuration for a system and its template
/// </summary>
public class SystemTemplateConfigs
{
    /// <summary>
    /// Gets or sets the System configuration
    /// </summary>
    public EssentialsConfig System { get; set; }

    /// <summary>
    /// Gets or sets the Template
    /// </summary>
    public EssentialsConfig Template { get; set; }
}

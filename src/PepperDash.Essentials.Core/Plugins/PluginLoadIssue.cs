// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials;

/// <summary>
/// How serious a plugin load issue is.
/// </summary>
public enum PluginLoadIssueSeverity
{
    /// <summary>
    /// The plugin still loaded, but something about it may cause problems at runtime.
    /// </summary>
    Warning,

    /// <summary>
    /// The plugin, or part of it, could not be loaded.
    /// </summary>
    Error
}

/// <summary>
/// What kind of problem a plugin load issue describes.
/// </summary>
public enum PluginLoadIssueCategory
{
    /// <summary>
    /// The plugin was built against a newer Essentials than the one running.
    /// </summary>
    BuiltAgainstNewerEssentials,

    /// <summary>
    /// The plugin references a newer version of an assembly that is already present, such as the Crestron SDK.
    /// </summary>
    NewerDependencyReference,

    /// <summary>
    /// The plugin uses an API that is not available on .NET 8.
    /// </summary>
    IncompatibleApi,

    /// <summary>
    /// The plugin's factory requires a newer Essentials than the one running, so it was skipped.
    /// </summary>
    MinimumVersionNotMet,

    /// <summary>
    /// An assembly the plugin depends on could not be found.
    /// </summary>
    MissingDependency,

    /// <summary>
    /// The assembly could not be loaded.
    /// </summary>
    LoadFailure,

    /// <summary>
    /// The assembly loaded, but its types could not be read.
    /// </summary>
    TypeLoadFailure,

    /// <summary>
    /// A plugin device factory could not be created or registered.
    /// </summary>
    FactoryFailure,

    /// <summary>
    /// The plugin file could not be moved into place or extracted from its .cplz.
    /// </summary>
    PackageExtractFailure
}

/// <summary>
/// What happened to a plugin file during startup.
/// </summary>
public enum PluginLoadStatus
{
    /// <summary>
    /// The file has been found but not processed yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The assembly loaded and at least one plugin device factory was registered.
    /// </summary>
    Loaded,

    /// <summary>
    /// The assembly loaded but contains no plugin device factory, so it is a library another plugin uses.
    /// </summary>
    LoadedAsLibrary,

    /// <summary>
    /// The assembly loaded, but none of its plugin device factories were registered.
    /// </summary>
    Skipped,

    /// <summary>
    /// The assembly could not be loaded.
    /// </summary>
    Failed
}

/// <summary>
/// One problem found while loading a plugin.
/// </summary>
public sealed class PluginLoadIssue
{
    /// <summary>
    /// Creates an issue, time-stamped now.
    /// </summary>
    public PluginLoadIssue(PluginLoadIssueSeverity severity, PluginLoadIssueCategory category, string message,
        string detail = null, string triggeredBy = null)
    {
        Severity = severity;
        Category = category;
        Message = message;
        Detail = detail;
        TriggeredBy = triggeredBy ?? "Direct load";
        TimestampUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// How serious the issue is.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public PluginLoadIssueSeverity Severity { get; }

    /// <summary>
    /// What kind of problem this is.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public PluginLoadIssueCategory Category { get; }

    /// <summary>
    /// A one-line description of the problem.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Further detail, such as an exception or advice on how to fix the problem. May be null.
    /// </summary>
    public string Detail { get; }

    /// <summary>
    /// The plugin whose load caused this issue, or "Direct load" when the file itself was being loaded.
    /// </summary>
    public string TriggeredBy { get; }

    /// <summary>
    /// When the issue was recorded.
    /// </summary>
    public DateTime TimestampUtc { get; }
}

/// <summary>
/// Everything recorded about one plugin file during startup.
/// </summary>
public sealed class PluginLoadReport
{
    private readonly List<PluginLoadIssue> issues = new List<PluginLoadIssue>();

    /// <summary>
    /// Creates a report for a plugin file.
    /// </summary>
    /// <param name="fileName">The file name, e.g. <c>MyPlugin.dll</c>.</param>
    public PluginLoadReport(string fileName)
    {
        FileName = fileName;
        BuiltAgainst = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Status = PluginLoadStatus.Pending;
    }

    /// <summary>
    /// The plugin file name, e.g. <c>MyPlugin.dll</c>.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// The assembly's simple name, if it could be read.
    /// </summary>
    public string AssemblyName { get; set; }

    /// <summary>
    /// The plugin's own version, if it could be read.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// The Essentials packages the plugin was built against, keyed by package ID. Empty when the plugin
    /// was built before build-time version stamping existed.
    /// </summary>
    public Dictionary<string, string> BuiltAgainst { get; }

    /// <summary>
    /// What happened to the plugin during startup.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public PluginLoadStatus Status { get; set; }

    /// <summary>
    /// The number of plugin device factory types found in the assembly.
    /// </summary>
    [JsonIgnore]
    public int FactoryTypesFound { get; set; }

    /// <summary>
    /// True once the assembly's types could not be read.
    /// </summary>
    [JsonIgnore]
    public bool TypesFailedToLoad { get; set; }

    /// <summary>
    /// The issues recorded for this plugin, oldest first.
    /// </summary>
    public IReadOnlyList<PluginLoadIssue> Issues => issues;

    /// <summary>
    /// True when any recorded issue is an error.
    /// </summary>
    [JsonIgnore]
    public bool HasErrors => issues.Any(i => i.Severity == PluginLoadIssueSeverity.Error);

    /// <summary>
    /// Records an issue against this plugin.
    /// </summary>
    public void AddIssue(PluginLoadIssue issue)
    {
        if (issue != null)
        {
            issues.Add(issue);
        }
    }

    /// <summary>
    /// Returns a copy that is safe to hand to a reader while loading continues.
    /// </summary>
    public PluginLoadReport Clone()
    {
        var copy = new PluginLoadReport(FileName)
        {
            AssemblyName = AssemblyName,
            Version = Version,
            Status = Status,
            FactoryTypesFound = FactoryTypesFound,
            TypesFailedToLoad = TypesFailedToLoad
        };

        foreach (var pair in BuiltAgainst)
        {
            copy.BuiltAgainst[pair.Key] = pair.Value;
        }

        foreach (var issue in issues)
        {
            copy.issues.Add(issue);
        }

        return copy;
    }
}

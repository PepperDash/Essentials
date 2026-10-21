// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials;

/// <summary>
/// Compares how a plugin was built against what is running, before the plugin is loaded. Every result
/// is a warning: a mismatch is reported, never used to stop a plugin loading.
/// </summary>
public static class PluginVersionChecker
{
    /// <summary>
    /// Assembly names that ship as part of Essentials. A plugin's references to these are judged against
    /// the running Essentials version rather than against each file's assembly version.
    /// </summary>
    public static readonly IReadOnlyCollection<string> EssentialsAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "PepperDashEssentials",
        "PepperDash_Essentials_Core",
        "PepperDashCore",
        "Essentials Devices Common",
        "mobile-control-messengers",
        "epi-essentials-mobile-control"
    };

    /// <summary>
    /// Checks one plugin.
    /// </summary>
    /// <param name="info">What the plugin's metadata says, from <see cref="PluginAssemblyInspector"/>.</param>
    /// <param name="runningEssentialsVersion">The running Essentials informational version, e.g. <c>3.0.0-rc.16</c>.</param>
    /// <param name="availableAssemblies">Assemblies already present in the running program, by simple name.</param>
    /// <returns>The issues found, empty if none.</returns>
    public static List<PluginLoadIssue> Check(PluginBuildInfo info, string runningEssentialsVersion,
        IReadOnlyDictionary<string, Version> availableAssemblies)
    {
        var issues = new List<PluginLoadIssue>();

        if (info == null)
            return issues;

        EssentialsVersion.TryParse(runningEssentialsVersion, out var running);

        var essentialsIssue = info.BuiltAgainst.Count > 0
            ? CheckStampedVersion(info, running)
            : CheckReferencedEssentialsVersion(info, running);

        if (essentialsIssue != null)
            issues.Add(essentialsIssue);

        if (availableAssemblies != null)
            issues.AddRange(CheckDependencyReferences(info, availableAssemblies));

        return issues;
    }

    private static PluginLoadIssue CheckStampedVersion(PluginBuildInfo info, EssentialsVersion running)
    {
        if (running == null || !running.IsComparable)
            return null;

        // A plugin can reference more than one Essentials package directly. Report the newest one that
        // is actually newer than running. Stamps that can't be compared (e.g. a -local build) are left
        // out first, or one could win "newest" and hide a comparable stamp that is newer than running.
        EssentialsVersion newest = null;
        string newestPackage = null;

        foreach (var pair in info.BuiltAgainst)
        {
            if (!EssentialsVersion.TryParse(pair.Value, out var stamped))
                continue;

            var comparison = EssentialsVersion.CompareForCompatibility(stamped, running);
            if (comparison == null || comparison <= 0)
                continue;

            if (newest == null || stamped.CompareTo(newest) > 0)
            {
                newest = stamped;
                newestPackage = pair.Key;
            }
        }

        if (newest == null)
            return null;

        return new PluginLoadIssue(
            PluginLoadIssueSeverity.Warning,
            PluginLoadIssueCategory.BuiltAgainstNewerEssentials,
            $"Built against {newestPackage} {newest}, but Essentials {running} is running.",
            $"Plugins built against a newer Essentials can call APIs, or depend on Crestron SDK versions, that this Essentials does not have. The plugin was still loaded. Upgrade Essentials to {newest} or later, or rebuild the plugin against {running}.");
    }

    private static PluginLoadIssue CheckReferencedEssentialsVersion(PluginBuildInfo info, EssentialsVersion running)
    {
        if (running == null || !running.IsComparable)
            return null;

        var newestReference = info.References
            .Where(r => EssentialsAssemblyNames.Contains(r.Name) && r.Version != null)
            .OrderByDescending(r => r.Version)
            .FirstOrDefault();

        if (newestReference == null)
            return null;

        // Assembly versions carry no prerelease label, so only major.minor.patch can be compared.
        var referenced = (newestReference.Version.Major, newestReference.Version.Minor, Math.Max(newestReference.Version.Build, 0));
        var current = (running.Major, running.Minor, running.Patch);

        if (referenced.CompareTo(current) <= 0)
            return null;

        return new PluginLoadIssue(
            PluginLoadIssueSeverity.Warning,
            PluginLoadIssueCategory.BuiltAgainstNewerEssentials,
            $"References {newestReference.Name} {newestReference.Version}, but Essentials {running} is running.",
            $"Plugins built against a newer Essentials can call APIs that this Essentials does not have. The plugin was still loaded. This plugin was built before Essentials recorded its version in plugins, so only major.minor.patch could be compared. Upgrade Essentials, or rebuild the plugin against {running}.");
    }

    private static IEnumerable<PluginLoadIssue> CheckDependencyReferences(PluginBuildInfo info,
        IReadOnlyDictionary<string, Version> availableAssemblies)
    {
        foreach (var reference in info.References)
        {
            if (reference.Version == null || EssentialsAssemblyNames.Contains(reference.Name))
                continue;

            if (!availableAssemblies.TryGetValue(reference.Name, out var available) || available == null)
                continue;

            if (reference.Version <= available)
                continue;

            var sdkNote = IsCrestronSdkAssembly(reference.Name)
                ? " This assembly is part of the Crestron SDK, so Essentials and its plugins should be built against the same SDK version."
                : string.Empty;

            yield return new PluginLoadIssue(
                PluginLoadIssueSeverity.Warning,
                PluginLoadIssueCategory.NewerDependencyReference,
                $"References {reference.Name} {reference.Version}, but {available} is loaded.",
                $"The plugin was compiled against a newer {reference.Name} than the one available. Calls into anything added after {available} will fail at runtime with a MissingMethodException or TypeLoadException. The plugin was still loaded.{sdkNote}");
        }
    }

    /// <summary>
    /// Explains why a plugin factory failed its Essentials version check, so the report points at the
    /// right fix. A development factory that lists the builds it accepts is rejected when the running
    /// build is not in that list, even if its minimum version is met; mirrors
    /// <c>Global.IsRunningDevelopmentVersion</c>.
    /// </summary>
    /// <param name="factoryName">The factory's type name.</param>
    /// <param name="minimumVersion">The factory's <c>MinimumEssentialsFrameworkVersion</c>.</param>
    /// <param name="developmentVersions">The development factory's accepted builds, or null for a normal factory.</param>
    /// <param name="runningEssentialsVersion">The running Essentials version.</param>
    public static PluginLoadIssue DescribeFactoryRejection(string factoryName, string minimumVersion,
        IReadOnlyCollection<string> developmentVersions, string runningEssentialsVersion)
    {
        if (developmentVersions != null && !developmentVersions.Contains(runningEssentialsVersion))
        {
            var accepted = developmentVersions.Count > 0 ? string.Join(", ", developmentVersions) : "(none listed)";

            return new PluginLoadIssue(
                PluginLoadIssueSeverity.Error,
                PluginLoadIssueCategory.DevelopmentBuildNotListed,
                $"Plugin factory {factoryName} is a development build that only loads on Essentials {accepted}, but {runningEssentialsVersion} is running, so the factory was skipped.",
                "Run one of the listed Essentials builds, or deploy a release build of the plugin, which checks MinimumEssentialsFrameworkVersion instead of an exact build.");
        }

        return new PluginLoadIssue(
            PluginLoadIssueSeverity.Error,
            PluginLoadIssueCategory.MinimumVersionNotMet,
            $"Plugin factory {factoryName} requires Essentials {minimumVersion} or later, but {runningEssentialsVersion} is running. Dependency check failed, so the factory was skipped.",
            "Upgrade Essentials, or deploy a version of the plugin built for this Essentials.");
    }

    private static bool IsCrestronSdkAssembly(string name) =>
        name.StartsWith("Crestron", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("SimplSharp", StringComparison.OrdinalIgnoreCase);
}

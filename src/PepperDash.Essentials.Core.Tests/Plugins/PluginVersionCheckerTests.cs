using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PepperDash.Essentials;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Plugins;

public class PluginVersionCheckerTests
{
    private static readonly IReadOnlyDictionary<string, Version> NoAssemblies = new Dictionary<string, Version>();

    private static PluginBuildInfo Plugin(string? builtAgainst = null, params (string Name, string Version)[] references)
    {
        var info = new PluginBuildInfo { AssemblyName = "MyPlugin", AssemblyVersion = new Version(1, 0, 0, 0) };

        if (builtAgainst != null)
            info.BuiltAgainst["PepperDashEssentials"] = builtAgainst;

        foreach (var (name, version) in references)
            info.References.Add(new AssemblyReferenceInfo(name, Version.Parse(version)));

        return info;
    }

    [Fact]
    public void StampedNewerThanRunning_Warns()
    {
        var issues = PluginVersionChecker.Check(Plugin("3.0.0-rc.16"), "3.0.0-rc.11", NoAssemblies);

        issues.Should().ContainSingle();
        issues[0].Severity.Should().Be(PluginLoadIssueSeverity.Warning);
        issues[0].Category.Should().Be(PluginLoadIssueCategory.BuiltAgainstNewerEssentials);
        issues[0].Message.Should().Contain("3.0.0-rc.16").And.Contain("3.0.0-rc.11");
    }

    [Theory]
    [InlineData("3.0.0-rc.11", "3.0.0-rc.16")]
    [InlineData("3.0.0-rc.16", "3.0.0-rc.16")]
    [InlineData("3.0.0-rc.16", "3.0.0")]
    [InlineData("2.43.0", "3.0.0")]
    public void StampedSameOrOlder_NoIssue(string builtAgainst, string running)
    {
        PluginVersionChecker.Check(Plugin(builtAgainst), running, NoAssemblies).Should().BeEmpty();
    }

    [Theory]
    [InlineData("3.0.0-rc.16", "3.0.0-local")]
    [InlineData("3.0.0-feat-update-ihaswebview.1", "3.0.0-rc.16")]
    public void StampedButNotComparable_NoIssue(string builtAgainst, string running)
    {
        PluginVersionChecker.Check(Plugin(builtAgainst), running, NoAssemblies).Should().BeEmpty();
    }

    [Fact]
    public void ReportsTheNewestStampWhenBothPackagesAreStamped()
    {
        var info = Plugin("3.0.0-rc.12");
        info.BuiltAgainst["PepperDash.Essentials.Core"] = "3.0.0-rc.16";

        var issues = PluginVersionChecker.Check(info, "3.0.0-rc.11", NoAssemblies);

        issues.Should().ContainSingle().Which.Message.Should().Contain("PepperDash.Essentials.Core 3.0.0-rc.16");
    }

    [Fact]
    public void Unstamped_NewerEssentialsReference_Warns()
    {
        var issues = PluginVersionChecker.Check(
            Plugin(null, ("PepperDash_Essentials_Core", "3.1.0.0")), "3.0.0-rc.16", NoAssemblies);

        issues.Should().ContainSingle().Which.Category.Should().Be(PluginLoadIssueCategory.BuiltAgainstNewerEssentials);
    }

    [Fact]
    public void Unstamped_SameCoreVersionAsRunningPrerelease_NoIssue()
    {
        // The reference only says 3.0.0.0, so it must not rank above a running 3.0.0-rc.16
        PluginVersionChecker.Check(Plugin(null, ("PepperDash_Essentials_Core", "3.0.0.0")), "3.0.0-rc.16", NoAssemblies)
            .Should().BeEmpty();
    }

    [Fact]
    public void NewerCrestronSdkReference_Warns()
    {
        var available = new Dictionary<string, Version> { ["Crestron.SimplSharpPro.UI"] = new Version(2, 21, 128, 0) };

        var issues = PluginVersionChecker.Check(
            Plugin("3.0.0-rc.11", ("Crestron.SimplSharpPro.UI", "2.21.274.0")), "3.0.0-rc.11", available);

        var issue = issues.Should().ContainSingle().Subject;
        issue.Category.Should().Be(PluginLoadIssueCategory.NewerDependencyReference);
        issue.Message.Should().Contain("2.21.274.0").And.Contain("2.21.128.0");
        issue.Detail.Should().Contain("Crestron SDK");
    }

    [Fact]
    public void SameOrOlderDependencyReference_NoIssue()
    {
        var available = new Dictionary<string, Version>
        {
            ["Crestron.SimplSharpPro.UI"] = new Version(2, 21, 274, 0),
            ["Newtonsoft.Json"] = new Version(13, 0, 0, 0)
        };

        PluginVersionChecker.Check(
                Plugin(null, ("Crestron.SimplSharpPro.UI", "2.21.128.0"), ("Newtonsoft.Json", "13.0.0.0")), "3.0.0", available)
            .Should().BeEmpty();
    }

    [Fact]
    public void UnknownDependency_NoIssue()
    {
        PluginVersionChecker.Check(Plugin(null, ("SomeVendor.Sdk", "9.9.9.9")), "3.0.0", NoAssemblies)
            .Should().BeEmpty();
    }

    [Fact]
    public void EssentialsAssembliesAreNotDoubleReportedAsDependencies()
    {
        var available = new Dictionary<string, Version> { ["PepperDash_Essentials_Core"] = new Version(3, 0, 0, 0) };

        var issues = PluginVersionChecker.Check(
            Plugin(null, ("PepperDash_Essentials_Core", "3.1.0.0")), "3.0.0", available);

        issues.Should().ContainSingle().Which.Category.Should().Be(PluginLoadIssueCategory.BuiltAgainstNewerEssentials);
    }

    [Fact]
    public void ReportsBothEssentialsAndSdkMismatches()
    {
        var available = new Dictionary<string, Version> { ["Crestron.SimplSharpPro.UI"] = new Version(2, 21, 128, 0) };

        var issues = PluginVersionChecker.Check(
            Plugin("3.0.0-rc.16", ("Crestron.SimplSharpPro.UI", "2.21.274.0")), "3.0.0-rc.11", available);

        issues.Select(i => i.Category).Should().BeEquivalentTo(new[]
        {
            PluginLoadIssueCategory.BuiltAgainstNewerEssentials,
            PluginLoadIssueCategory.NewerDependencyReference
        });
    }
}

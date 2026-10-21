using FluentAssertions;
using PepperDash.Essentials;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Plugins;

public class PluginLoadReportFormatterTests
{
    private static PluginLoadReport Report(string fileName, PluginLoadStatus status, params PluginLoadIssue[] issues)
    {
        var report = new PluginLoadReport(fileName) { Version = "1.0.0", Status = status };

        foreach (var issue in issues)
            report.AddIssue(issue);

        return report;
    }

    private static PluginLoadIssue Warning(string message, string? detail = null) =>
        new(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.NewerDependencyReference, message, detail);

    private static PluginLoadIssue Error(string message) =>
        new(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.MinimumVersionNotMet, message);

    [Fact]
    public void NoIssues_SaysSo()
    {
        var text = PluginLoadReportFormatter.Format(new[] { Report("Clean.dll", PluginLoadStatus.Loaded) }, "3.0.0", false);

        text.Should().Contain("Running Essentials version: 3.0.0")
            .And.Contain("No plugin load issues.")
            .And.NotContain("Clean.dll");
    }

    [Fact]
    public void ListsEachIssueUnderItsPlugin()
    {
        var report = Report("Room.dll", PluginLoadStatus.Loaded, Warning("References Crestron.SimplSharpPro.UI 2.21.274.0"));
        report.BuiltAgainst["PepperDashEssentials"] = "3.0.0-rc.16";

        var text = PluginLoadReportFormatter.Format(new[] { report }, "3.0.0-rc.11", false);

        text.Should().Contain("1 issue(s) across 1 plugin file(s)")
            .And.Contain("Room.dll  v1.0.0  Status: Loaded  Built against: PepperDashEssentials 3.0.0-rc.16")
            .And.Contain("[Warning] NewerDependencyReference: References Crestron.SimplSharpPro.UI 2.21.274.0");
    }

    [Fact]
    public void ListsPluginsWithErrorsFirst()
    {
        var text = PluginLoadReportFormatter.Format(new[]
        {
            Report("A.dll", PluginLoadStatus.Loaded, Warning("a warning")),
            Report("B.dll", PluginLoadStatus.Skipped, Error("an error"))
        }, "3.0.0", false);

        text.IndexOf("B.dll").Should().BeLessThan(text.IndexOf("A.dll"));
    }

    [Fact]
    public void Verbose_IncludesDetail()
    {
        var reports = new[] { Report("Room.dll", PluginLoadStatus.Loaded, Warning("msg", "line one\r\nline two")) };

        PluginLoadReportFormatter.Format(reports, "3.0.0", false).Should().NotContain("line one")
            .And.Contain("pluginissues verbose");
        PluginLoadReportFormatter.Format(reports, "3.0.0", true).Should().Contain("line one").And.Contain("line two");
    }

    [Fact]
    public void ShowsWhichPluginRequiredAMissingDependency()
    {
        var issue = new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.PackageExtractFailure,
            "Could not move the file", triggeredBy: "Room.cplz");

        PluginLoadReportFormatter.Format(new[] { Report("Lib.dll", PluginLoadStatus.Failed, issue) }, "3.0.0", false)
            .Should().Contain("(required by Room.cplz)");
    }
}

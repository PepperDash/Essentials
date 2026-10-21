// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PepperDash.Essentials;

/// <summary>
/// Formats plugin load reports as console text.
/// </summary>
public static class PluginLoadReportFormatter
{
    private const string NewLine = "\r\n";

    /// <summary>
    /// Formats the reports that have issues, one block per plugin.
    /// </summary>
    /// <param name="reports">All plugin load reports. Reports without issues are left out.</param>
    /// <param name="runningEssentialsVersion">The running Essentials version, shown in the header.</param>
    /// <param name="verbose">When true, each issue's detail is printed under it.</param>
    public static string Format(IEnumerable<PluginLoadReport> reports, string runningEssentialsVersion, bool verbose)
    {
        var withIssues = (reports ?? Enumerable.Empty<PluginLoadReport>())
            .Where(r => r.Issues.Count > 0)
            .OrderByDescending(r => r.HasErrors)
            .ThenBy(r => r.FileName)
            .ToList();

        var text = new StringBuilder();
        text.Append("Running Essentials version: ").Append(runningEssentialsVersion ?? "unknown").Append(NewLine);

        if (withIssues.Count == 0)
        {
            text.Append("No plugin load issues.").Append(NewLine);
            return text.ToString();
        }

        var issueCount = withIssues.Sum(r => r.Issues.Count);
        text.Append($"{issueCount} issue(s) across {withIssues.Count} plugin file(s):").Append(NewLine);

        foreach (var report in withIssues)
        {
            text.Append(NewLine);
            text.Append(report.FileName);

            if (!string.IsNullOrEmpty(report.Version))
                text.Append("  v").Append(report.Version);

            text.Append("  Status: ").Append(report.Status);

            if (report.BuiltAgainst.Count > 0)
            {
                text.Append("  Built against: ")
                    .Append(string.Join(", ", report.BuiltAgainst.Select(p => $"{p.Key} {p.Value}")));
            }

            text.Append(NewLine);

            foreach (var issue in report.Issues)
            {
                text.Append("  ")
                    .Append($"[{issue.Severity}]".PadRight(10))
                    .Append(issue.Category)
                    .Append(": ")
                    .Append(issue.Message);

                if (issue.TriggeredBy != "Direct load")
                    text.Append(" (required by ").Append(issue.TriggeredBy).Append(')');

                text.Append(NewLine);

                if (verbose && !string.IsNullOrEmpty(issue.Detail))
                {
                    foreach (var line in issue.Detail.Replace("\r\n", "\n").Split('\n'))
                    {
                        text.Append("            ").Append(line).Append(NewLine);
                    }
                }
            }
        }

        if (!verbose)
            text.Append(NewLine).Append("Run 'pluginissues verbose' for details.").Append(NewLine);

        return text.ToString();
    }
}

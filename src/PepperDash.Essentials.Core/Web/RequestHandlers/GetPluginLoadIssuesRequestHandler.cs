using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Returns the plugins that recorded problems while loading: version mismatches, load failures,
/// missing dependencies and skipped factories.
/// </summary>
/// <remarks>
/// Only plugin files with at least one issue are returned. Pass <c>?all=true</c> to include every
/// plugin file, so a client can show which ones loaded cleanly.
/// </remarks>
public class GetPluginLoadIssuesRequestHandler : WebApiBaseRequestHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginLoadIssuesRequestHandler"/> class.
    /// </summary>
    /// <remarks>base(true) enables CORS support by default</remarks>
    public GetPluginLoadIssuesRequestHandler() : base(true) { }

    /// <inheritdoc />
    protected override void HandleGet(HttpCwsContext context)
    {
        try
        {
            var includeAll = string.Equals(context.Request.QueryString["all"], "true", StringComparison.OrdinalIgnoreCase);

            var reports = includeAll
                ? PluginLoader.PluginLoadReports
                : PluginLoader.PluginLoadReportsWithIssues;

            var response = JsonConvert.SerializeObject(new
            {
                EssentialsVersion = Global.AssemblyVersion,
                IssueCount = reports.Sum(r => r.Issues.Count),
                ErrorCount = reports.Sum(r => r.Issues.Count(i => i.Severity == PluginLoadIssueSeverity.Error)),
                Plugins = reports
                    .OrderByDescending(r => r.HasErrors)
                    .ThenBy(r => r.FileName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            }, Formatting.Indented);

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(response, false);
            context.Response.End();
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error getting plugin load issues");
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.End();
        }
    }
}

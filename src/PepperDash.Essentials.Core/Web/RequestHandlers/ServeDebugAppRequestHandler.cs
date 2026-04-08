using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Serves the React debug app from the processor's /HTML/debug folder.
/// The root route (debug) and all sub-paths (debug/{*filePath}) are handled here.
/// Text assets are sent as UTF-8 strings; binary assets are written to the response
/// OutputStream.  Any sub-path that does not match a real file falls back to
/// index.html so that client-side (React Router) routing continues to work.
/// </summary>
public class ServeDebugAppRequestHandler : WebApiBaseRequestHandler
{
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".html", "text/html; charset=utf-8" },
        { ".htm",  "text/html; charset=utf-8" },
        { ".js",   "application/javascript" },
        { ".mjs",  "application/javascript" },
        { ".jsx",  "application/javascript" },
        { ".css",  "text/css" },
        { ".json", "application/json" },
        { ".map",  "application/json" },
        { ".svg",  "image/svg+xml" },
        { ".ico",  "image/x-icon" },
        { ".png",  "image/png" },
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif",  "image/gif" },
        { ".woff", "font/woff" },
        { ".woff2","font/woff2" },
        { ".ttf",  "font/ttf" },
        { ".eot",  "application/vnd.ms-fontobject" },
    };

    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".js", ".mjs", ".jsx", ".css", ".json", ".map", ".svg", ".txt", ".xml"
    };

    /// <summary>
    /// Constructor. CORS is enabled so browser dev-tools requests succeed.
    /// </summary>
    public ServeDebugAppRequestHandler() : base(true) { }

    /// <summary>
    /// Handles GET requests for the debug app and its static assets.
    /// </summary>
    protected override void HandleGet(HttpCwsContext context)
    {
        // When acting as the server-level fallback handler, only handle
        // requests that are actually for the /debug path; defer everything
        // else to the base class (which returns 501 Not Implemented).
        var rawUrl = context.Request.RawUrl ?? string.Empty;
        if (rawUrl.IndexOf("/debug", StringComparison.OrdinalIgnoreCase) < 0)
        {
            base.HandleGet(context);
            return;
        }

        try
        {
            var htmlDebugPath = GetHtmlDebugPath();
            if (htmlDebugPath == null)
            {
                SendResponse(context, 500, "Internal Server Error");
                return;
            }

            var requestedPath = GetRequestedFilePath(context);

            // Paths with no file extension are SPA client-side routes — serve index.html
            string candidate;
            if (string.IsNullOrEmpty(requestedPath) || !System.IO.Path.HasExtension(requestedPath))
            {
                candidate = System.IO.Path.Combine(htmlDebugPath, "index.html");
            }
            else
            {
                var relativePart = requestedPath.Replace('/', System.IO.Path.DirectorySeparatorChar);
                candidate = System.IO.Path.Combine(htmlDebugPath, relativePart);
            }

            // Resolve to an absolute path and guard against path-traversal attacks
            var resolvedCandidate = System.IO.Path.GetFullPath(candidate);
            var resolvedBase = System.IO.Path.GetFullPath(htmlDebugPath)
                               + System.IO.Path.DirectorySeparatorChar;

            if (!resolvedCandidate.StartsWith(resolvedBase, StringComparison.OrdinalIgnoreCase))
            {
                SendResponse(context, 403, "Forbidden");
                return;
            }

            // Missing static asset → fall back to index.html (SPA deep-link support)
            if (!File.Exists(resolvedCandidate) && System.IO.Path.HasExtension(requestedPath ?? string.Empty))
            {
                resolvedCandidate = System.IO.Path.Combine(htmlDebugPath, "index.html");
                Debug.LogMessage(LogEventLevel.Debug,
                    "ServeDebugAppRequestHandler: '{requestedPath:l}' not found, falling back to index.html",
                    requestedPath);
            }

            if (!File.Exists(resolvedCandidate))
            {
                SendResponse(context, 404, "Not Found");
                return;
            }

            var ext = System.IO.Path.GetExtension(resolvedCandidate);
            var contentType = MimeTypes.TryGetValue(ext, out var mime) ? mime : "application/octet-stream";

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = contentType;

            if (TextExtensions.Contains(ext))
            {
                string content;
                using (var reader = new StreamReader(resolvedCandidate))
                    content = reader.ReadToEnd();

                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Write(content, false);
            }
            else
            {
                var bytes = System.IO.File.ReadAllBytes(resolvedCandidate);
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            }

            context.Response.End();
        }
        catch (Exception ex)
        {
            Debug.LogMessage(LogEventLevel.Error,
                "ServeDebugAppRequestHandler: Unhandled error serving '{rawUrl:l}': {ex}",
                context.Request.RawUrl, ex.Message);
            try { SendResponse(context, 500, "Internal Server Error"); } catch { /* best-effort */ }
        }
    }

    /// <summary>
    /// Resolves the absolute path of the /HTML/debug folder on the processor.
    /// </summary>
    /// <remarks>
    /// On a 4-series appliance, <c>Global.FilePathPrefix</c> is
    /// <c>{root}/user/programX/</c>, so walking up two parents gives the
    /// processor root that contains the <c>html</c> folder.
    /// On Virtual Control, <c>Global.FilePathPrefix</c> is <c>{root}/User/</c>,
    /// so only one parent hop is needed.
    /// </remarks>
    private static string GetHtmlDebugPath()
    {
        try
        {
            var separators = new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };
            var programDir = new DirectoryInfo(Global.FilePathPrefix.TrimEnd(separators));

            DirectoryInfo rootDir;
            if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Server)
            {
                // Virtual Control: {root}/User/ → one parent up = {root}
                rootDir = programDir.Parent;
            }
            else
            {
                // 4-series appliance: {root}/user/programX/ → two parents up = {root}
                rootDir = programDir.Parent?.Parent;
            }

            if (rootDir == null)
            {
                Debug.LogMessage(LogEventLevel.Error,
                    "ServeDebugAppRequestHandler: Cannot resolve HTML root from FilePathPrefix '{prefix:l}'",
                    Global.FilePathPrefix);
                return null;
            }

            return System.IO.Path.Combine(rootDir.FullName, "html", "debug");
        }
        catch (Exception ex)
        {
            Debug.LogMessage(LogEventLevel.Error,
                "ServeDebugAppRequestHandler: Error resolving HTML debug path: {ex}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Extracts the file sub-path from the request by parsing <c>RawUrl</c>.
    /// Returns an empty string when the URL ends at <c>/debug</c> (root hit).
    /// </summary>
    private static string GetRequestedFilePath(HttpCwsContext context)
    {
        var rawUrl = context.Request.RawUrl ?? string.Empty;

        // Locate the /debug segment in the URL
        const string debugToken = "/debug";
        var idx = rawUrl.IndexOf(debugToken, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return string.Empty;

        var afterDebug = rawUrl.Substring(idx + debugToken.Length);

        // Strip query string
        var qIdx = afterDebug.IndexOf('?');
        if (qIdx >= 0)
            afterDebug = afterDebug.Substring(0, qIdx);

        // Strip leading slash to get a relative file path
        return afterDebug.TrimStart('/');
    }

    private static void SendResponse(HttpCwsContext context, int statusCode, string statusDescription)
    {
        context.Response.StatusCode = statusCode;
        context.Response.StatusDescription = statusDescription;
        context.Response.End();
    }
}

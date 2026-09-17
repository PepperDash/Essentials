using System;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Shared plumbing for the secrets endpoints.
/// </summary>
/// <remarks>
/// <para>
/// Every secrets handler overrides <see cref="HandleOptions"/>. The base class advertises only
/// <c>POST, GET, OPTIONS</c> and its own OPTIONS handler returns 501, so without this a
/// cross-origin JSON request - which is what the developer tools app makes when served from the
/// Vite dev server - fails preflight before it is ever dispatched.
/// </para>
/// <para>
/// Mutations are POST rather than DELETE for the same reason: DELETE is not in the advertised
/// method list.
/// </para>
/// </remarks>
public abstract class SecretsRequestHandlerBase : WebApiBaseRequestHandler
{
    /// <summary>
    /// Initializes a new instance with CORS enabled, matching the other developer tools endpoints.
    /// </summary>
    protected SecretsRequestHandlerBase() : base(true)
    {
    }

    /// <inheritdoc />
    protected override void HandleOptions(HttpCwsContext context)
    {
        context.Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type");
        context.Response.AppendHeader("Access-Control-Max-Age", "86400");
        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";
        context.Response.End();
    }

    /// <summary>
    /// Writes a JSON response with the given status code.
    /// </summary>
    protected static void WriteResponse(HttpCwsContext context, int statusCode, object payload)
    {
        context.Response.StatusCode = statusCode;
        context.Response.StatusDescription = DescriptionFor(statusCode);
        context.Response.ContentType = "application/json";
        context.Response.ContentEncoding = Encoding.UTF8;
        context.Response.Write(JsonConvert.SerializeObject(payload, Formatting.Indented), false);
        context.Response.End();
    }

    /// <summary>
    /// Reads a query-string value, returning null when absent.
    /// </summary>
    protected static string Query(HttpCwsContext context, string name)
    {
        try
        {
            return context.Request.QueryString[name];
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Reads a boolean query flag. Absent or unparseable reads as false.
    /// </summary>
    protected static bool QueryFlag(HttpCwsContext context, string name)
        => string.Equals(Query(context, name), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Deserializes a request body, reporting failure rather than throwing.
    /// </summary>
    /// <remarks>
    /// The caught exception's message is deliberately not surfaced: a deserializer error can quote
    /// the offending JSON, which for these endpoints is a credential.
    /// </remarks>
    protected static bool TryReadBody<T>(HttpCwsContext context, out T value, out string error)
        where T : class
    {
        value = null;
        error = null;

        try
        {
            if (context.Request.ContentLength < 0)
            {
                error = "No request body.";
                return false;
            }

            var data = context.Request.GetRequestBody();
            if (string.IsNullOrEmpty(data))
            {
                error = "No request body.";
                return false;
            }

            value = JsonConvert.DeserializeObject<T>(data);
            if (value == null)
            {
                error = "The request body could not be read.";
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            error = "The request body is not valid JSON.";
            return false;
        }
    }

    private static string DescriptionFor(int statusCode) => statusCode switch
    {
        200 => "OK",
        202 => "Accepted",
        400 => "Bad Request",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        503 => "Service Unavailable",
        507 => "Insufficient Storage",
        _ => "Internal Server Error"
    };
}

/// <summary>
/// Lists the registered secret providers.
/// </summary>
public class SecretsProvidersRequestHandler : SecretsRequestHandlerBase
{
    /// <inheritdoc />
    protected override void HandleGet(HttpCwsContext context)
    {
        var (status, response) = SecretsApiExecutor.ListProviders();
        WriteResponse(context, status, response);
    }
}

/// <summary>
/// Lists the keys a provider holds. Never returns a value.
/// </summary>
public class SecretsListRequestHandler : SecretsRequestHandlerBase
{
    /// <inheritdoc />
    protected override void HandleGet(HttpCwsContext context)
    {
        var provider = Query(context, "provider");
        var (status, response) = SecretsApiExecutor.List(
            provider,
            QueryFlag(context, "includeReserved"),
            QueryFlag(context, "includeSizes"));

        WriteResponse(context, status, response);
    }
}

/// <summary>
/// Runs a single-secret command: set, update, delete, test, pruneIndex or rebuildIndex.
/// </summary>
public class SecretsCommandRequestHandler : SecretsRequestHandlerBase
{
    /// <inheritdoc />
    protected override void HandlePost(HttpCwsContext context)
    {
        try
        {
            if (!TryReadBody<SecretCommandRequest>(context, out var request, out var error))
            {
                WriteResponse(context, 400, new SecretCommandResponse
                {
                    Status = "error",
                    Error = new SecretsApiError { Code = "invalidJson", Message = error }
                });
                return;
            }

            // Action and key only - the value must never reach the log.
            Debug.LogMessage(LogEventLevel.Debug,
                "Secrets command: {action} {provider}:{key}",
                null, request.Action, request.Provider, request.Key);

            var (status, response) = SecretsApiExecutor.ExecuteCommand(request);
            WriteResponse(context, status, response);
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error handling secrets command: {Exception}");
            WriteResponse(context, 500, new SecretCommandResponse
            {
                Status = "error",
                Error = new SecretsApiError { Code = "executionError", Message = ex.Message }
            });
        }
    }
}

/// <summary>
/// Previews or applies a batch of secrets.
/// </summary>
public class SecretsBulkRequestHandler : SecretsRequestHandlerBase
{
    /// <inheritdoc />
    protected override void HandlePost(HttpCwsContext context)
    {
        try
        {
            if (!TryReadBody<BulkSecretsRequest>(context, out var request, out var error))
            {
                WriteResponse(context, 400, new BulkSecretsResponse
                {
                    Error = new SecretsApiError { Code = "invalidJson", Message = error }
                });
                return;
            }

            // Mode and provider only - the batch carries values.
            Debug.LogMessage(LogEventLevel.Debug,
                "Secrets bulk: {mode} on {provider} (overwrite {overwrite})",
                null, request.Mode, request.Provider, request.Overwrite);

            var (status, response) = SecretsApiExecutor.ExecuteBulk(request);
            WriteResponse(context, status, response);
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error handling bulk secrets: {Exception}");
            WriteResponse(context, 500, new BulkSecretsResponse
            {
                Error = new SecretsApiError { Code = "executionError", Message = ex.Message }
            });
        }
    }
}

/// <summary>
/// Returns a key-only template with blank values, ready to fill in and apply elsewhere.
/// </summary>
public class SecretsTemplateRequestHandler : SecretsRequestHandlerBase
{
    /// <inheritdoc />
    protected override void HandleGet(HttpCwsContext context)
    {
        var (status, response) = SecretsApiExecutor.BuildTemplate(
            Query(context, "provider"),
            QueryFlag(context, "includeUnmanaged"));

        WriteResponse(context, status, response);
    }
}

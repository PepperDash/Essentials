using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Executes key-addressed routing commands - making and clearing routes from the routing dev tools.
/// POST only; all logic lives in <see cref="RoutingCommandExecutor"/>.
/// </summary>
public class RoutingCommandRequestHandler : WebApiBaseRequestHandler
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>base(true) enables CORS support, matching the other routing handlers.</remarks>
    public RoutingCommandRequestHandler()
        : base(true)
    {
    }

    /// <summary>
    /// Handles POST method requests.
    /// </summary>
    /// <param name="context">The request context.</param>
    protected override void HandlePost(HttpCwsContext context)
    {
        try
        {
            if (context.Request.ContentLength < 0)
            {
                WriteResponse(context, 400, ErrorResponse("invalidJson", "No request body."));
                return;
            }

            var data = context.Request.GetRequestBody();
            if (string.IsNullOrEmpty(data))
            {
                WriteResponse(context, 400, ErrorResponse("invalidJson", "No request body."));
                return;
            }

            RoutingCommandRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<RoutingCommandRequest>(data);
            }
            catch (JsonException ex)
            {
                WriteResponse(context, 400, ErrorResponse("invalidJson", ex.Message));
                return;
            }

            Debug.LogMessage(LogEventLevel.Debug, "Routing command: {@command}", null, request);

            var (statusCode, response) = RoutingCommandExecutor.Execute(request);
            WriteResponse(context, statusCode, response);
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error handling routing command: {Exception}");
            WriteResponse(context, 500, ErrorResponse("executionError", ex.Message));
        }
    }

    /// <summary>
    /// Handles OPTIONS preflight requests.
    /// </summary>
    /// <remarks>
    /// The base class's CORS support emits Allow-Origin and Allow-Methods but not Allow-Headers, and
    /// its HandleOptions returns 501 - so a genuinely cross-origin POST with a JSON content type
    /// would fail preflight. The dev-tools app is same-origin, so nothing is broken today; this just
    /// keeps the endpoint usable from external tooling.
    /// </remarks>
    /// <param name="context">The request context.</param>
    protected override void HandleOptions(HttpCwsContext context)
    {
        context.Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type");
        context.Response.AppendHeader("Access-Control-Max-Age", "86400");
        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";
        context.Response.End();
    }

    private static RoutingCommandResponse ErrorResponse(string code, string message)
    {
        return new RoutingCommandResponse
        {
            Status = "error",
            Error = new RoutingCommandError { Code = code, Message = message }
        };
    }

    private static void WriteResponse(HttpCwsContext context, int statusCode, RoutingCommandResponse response)
    {
        context.Response.StatusCode = statusCode;
        context.Response.StatusDescription = DescriptionFor(statusCode);
        context.Response.ContentType = "application/json";
        context.Response.ContentEncoding = Encoding.UTF8;
        context.Response.Write(JsonConvert.SerializeObject(response, Formatting.Indented), false);
        context.Response.End();
    }

    private static string DescriptionFor(int statusCode) => statusCode switch
    {
        200 => "OK",
        202 => "Accepted",
        400 => "Bad Request",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        _ => "Internal Server Error"
    };
}

/// <summary>
/// A single key-addressed routing command.
/// </summary>
/// <remarks>
/// Ports are addressed by key rather than selector: <see cref="RoutingPort.Selector"/> is an
/// arbitrary driver-defined object that cannot be expressed in JSON, so the server resolves
/// key -> port -> selector itself.
/// </remarks>
public class RoutingCommandRequest
{
    /// <summary>
    /// Gets or sets the command: sinkRoute, midpointSwitch, clearSink or clearMidpointOutput.
    /// Case-insensitive.
    /// </summary>
    [JsonProperty("command")]
    public string Command { get; set; }

    /// <summary>
    /// Gets or sets the target device key. For sinkRoute/clearSink this is the destination sink, or
    /// the IRoutingSinkWithLayouts parent when addressing a multiview tile. For midpointSwitch and
    /// clearMidpointOutput it is the IRoutingMidpointWithFeedback device.
    /// </summary>
    [JsonProperty("deviceKey")]
    public string DeviceKey { get; set; }

    /// <summary>
    /// Gets or sets the input port key on the target device. May be a multiview-qualified key of the
    /// form "tile{N}:{realPortKey}". Optional for clearSink, where omitting it clears whatever route
    /// the sink currently has.
    /// </summary>
    [JsonProperty("inputPortKey", NullValueHandling = NullValueHandling.Ignore)]
    public string InputPortKey { get; set; }

    /// <summary>
    /// Gets or sets the output port key on a midpoint device. Required for midpointSwitch and
    /// clearMidpointOutput.
    /// </summary>
    [JsonProperty("outputPortKey", NullValueHandling = NullValueHandling.Ignore)]
    public string OutputPortKey { get; set; }

    /// <summary>
    /// Gets or sets the source device key for sinkRoute. Must implement IRoutingOutputs.
    /// </summary>
    [JsonProperty("sourceDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
    public string SourceDeviceKey { get; set; }

    /// <summary>
    /// Gets or sets a specific output port on the source device. Omit to let route discovery choose.
    /// </summary>
    [JsonProperty("sourcePortKey", NullValueHandling = NullValueHandling.Ignore)]
    public string SourcePortKey { get; set; }

    /// <summary>
    /// Gets or sets the signal type: Audio, Video, AudioVideo or Usb. Case-insensitive; defaults to
    /// AudioVideo when omitted.
    /// </summary>
    [JsonProperty("signalType")]
    public string SignalType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to only release the route rather than clear it -
    /// stopping usage tracking but leaving the signal flowing. clearSink only.
    /// </summary>
    [JsonProperty("releaseOnly")]
    public bool ReleaseOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to also deselect the sink's own input. clearSink only.
    /// Off by default, since clearing a route never touches the destination and not every driver
    /// tolerates a null selector.
    /// </summary>
    [JsonProperty("clearSinkInput")]
    public bool ClearSinkInput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate and compute the route but execute nothing.
    /// </summary>
    [JsonProperty("dryRun")]
    public bool DryRun { get; set; }
}

/// <summary>
/// The result of a routing command.
/// </summary>
/// <remarks>
/// Devices and ports are reported using graph-facing keys - a tile sink is reported under its
/// IRoutingSinkWithLayouts parent with a "tile{N}:" qualified port key - so a client can match them
/// against the routing graph directly.
/// </remarks>
public class RoutingCommandResponse
{
    /// <summary>
    /// Gets or sets the outcome: executed, accepted, validated or error.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>Gets or sets the echoed command name.</summary>
    [JsonProperty("command", NullValueHandling = NullValueHandling.Ignore)]
    public string Command { get; set; }

    /// <summary>Gets or sets the graph-facing device key the command was addressed to.</summary>
    [JsonProperty("deviceKey", NullValueHandling = NullValueHandling.Ignore)]
    public string DeviceKey { get; set; }

    /// <summary>
    /// Gets or sets the real device key the command executed against. Differs from
    /// <see cref="DeviceKey"/> only when a "tile{N}:" port was de-qualified to its child tile sink.
    /// </summary>
    [JsonProperty("resolvedDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
    public string ResolvedDeviceKey { get; set; }

    /// <summary>Gets or sets the real input port key on the resolved device.</summary>
    [JsonProperty("resolvedInputPortKey", NullValueHandling = NullValueHandling.Ignore)]
    public string ResolvedInputPortKey { get; set; }

    /// <summary>Gets or sets the real output port key on the resolved device.</summary>
    [JsonProperty("resolvedOutputPortKey", NullValueHandling = NullValueHandling.Ignore)]
    public string ResolvedOutputPortKey { get; set; }

    /// <summary>Gets or sets the signal type as parsed from the request.</summary>
    [JsonProperty("signalType", NullValueHandling = NullValueHandling.Ignore)]
    public string SignalType { get; set; }

    /// <summary>
    /// Gets or sets the signal type actually handed to the devices. May be wider than
    /// <see cref="SignalType"/> when a pre-mapped route descriptor is reused - those are built from
    /// the port's declared type, so an Audio-only request across AudioVideo ports executes as
    /// AudioVideo rather than breaking away.
    /// </summary>
    [JsonProperty("effectiveSignalType", NullValueHandling = NullValueHandling.Ignore)]
    public string EffectiveSignalType { get; set; }

    /// <summary>
    /// Gets or sets the switch steps that will be, or were, executed in order. sinkRoute only.
    /// </summary>
    [JsonProperty("steps", NullValueHandling = NullValueHandling.Ignore)]
    public List<RoutingCommandStepInfo> Steps { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an AudioVideo request found a path for only one of the
    /// two signal types. The half that was found is still routed.
    /// </summary>
    [JsonProperty("partial")]
    public bool Partial { get; set; }

    /// <summary>Gets or sets the failure detail. Populated only when <see cref="Status"/> is error.</summary>
    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public RoutingCommandError Error { get; set; }
}

/// <summary>
/// A single switch step in a discovered route path. Property names deliberately match the read API's
/// RouteSwitchStepInfo so clients can reuse their existing step handling.
/// </summary>
public class RoutingCommandStepInfo
{
    /// <summary>
    /// Gets or sets the signal type for this step - Audio or Video, since an AudioVideo route is
    /// discovered as two separate paths.
    /// </summary>
    [JsonProperty("signalType")]
    public string SignalType { get; set; }

    /// <summary>Gets or sets the key of the device performing this switch.</summary>
    [JsonProperty("switchingDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
    public string SwitchingDeviceKey { get; set; }

    /// <summary>Gets or sets the input port being switched to.</summary>
    [JsonProperty("inputPortKey", NullValueHandling = NullValueHandling.Ignore)]
    public string InputPortKey { get; set; }

    /// <summary>
    /// Gets or sets the output port being switched from. Null on the final step onto a sink device,
    /// which has no output port.
    /// </summary>
    [JsonProperty("outputPortKey", NullValueHandling = NullValueHandling.Ignore)]
    public string OutputPortKey { get; set; }
}

/// <summary>
/// Machine-readable failure detail.
/// </summary>
public class RoutingCommandError
{
    /// <summary>
    /// Gets or sets a stable error code: invalidJson, missingField, unknownCommand, invalidSignalType,
    /// deviceNotFound, deviceNotRoutable, tileNotFound, portNotFound, signalTypeNotSupportedByPort,
    /// noRouteFound or executionError.
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; }

    /// <summary>Gets or sets the human-readable message.</summary>
    [JsonProperty("message")]
    public string Message { get; set; }

    /// <summary>Gets or sets the offending request field, where one applies.</summary>
    [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
    public string Field { get; set; }
}

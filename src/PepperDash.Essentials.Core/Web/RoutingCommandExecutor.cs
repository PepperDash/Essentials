using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web;

/// <summary>
/// Validates and executes key-addressed routing commands on behalf of
/// <see cref="RoutingCommandRequestHandler"/>. Deliberately HTTP-free so all the routing logic can
/// be read in one place, mirroring how <see cref="RoutingGraphHelpers"/> factors logic out of
/// <see cref="GetRoutingDevicesAndTieLinesHandler"/>.
/// </summary>
/// <remarks>
/// <para>
/// Ports are addressed by key throughout. <see cref="RoutingPort.Selector"/> is an arbitrary
/// driver-defined object that cannot be expressed in JSON, so callers name ports and this class
/// resolves key -> <see cref="RoutingPort"/> -> selector.
/// </para>
/// <para>
/// Sink commands return 202 rather than 200 because <c>ReleaseAndMakeRoute</c> and <c>ClearRoute</c>
/// enqueue onto a shared single-worker routing queue, and completion is conditional: a destination
/// that is an <see cref="IWarmingCooling"/> device mid-cooldown parks in <c>Extensions.RouteRequests</c>
/// until its cooldown finishes. <c>RunRouteRequest</c> also returns void and swallows its own
/// exceptions, so there is no completion signal to wait on even in principle. What makes the 202
/// meaningful is that all of the validation below - including full path discovery - happens
/// synchronously first, so a request that provably cannot work returns 409 before anything is queued.
/// </para>
/// </remarks>
public static class RoutingCommandExecutor
{
    private const string CommandSinkRoute = "sinkRoute";
    private const string CommandMidpointSwitch = "midpointSwitch";
    private const string CommandClearSink = "clearSink";
    private const string CommandClearMidpointOutput = "clearMidpointOutput";

    /// <summary>
    /// Validates and runs a routing command.
    /// </summary>
    /// <param name="request">The deserialized request.</param>
    /// <returns>An HTTP status code and the response body to write.</returns>
    public static (int StatusCode, RoutingCommandResponse Response) Execute(RoutingCommandRequest request)
    {
        if (request == null)
            return Error(400, "invalidJson", "Request body was empty or could not be parsed.");

        if (string.IsNullOrWhiteSpace(request.Command))
            return Error(400, "missingField", "'command' is required.", "command");

        if (string.IsNullOrWhiteSpace(request.DeviceKey))
            return Error(400, "missingField", "'deviceKey' is required.", "deviceKey");

        if (!TryParseSignalType(request.SignalType, out var signalType, out var signalTypeError))
            return Error(400, "invalidSignalType", signalTypeError, "signalType");

        try
        {
            if (string.Equals(request.Command, CommandSinkRoute, StringComparison.OrdinalIgnoreCase))
                return ExecuteSinkRoute(request, signalType);

            if (string.Equals(request.Command, CommandMidpointSwitch, StringComparison.OrdinalIgnoreCase))
                return ExecuteMidpointSwitch(request, signalType);

            if (string.Equals(request.Command, CommandClearSink, StringComparison.OrdinalIgnoreCase))
                return ExecuteClearSink(request);

            if (string.Equals(request.Command, CommandClearMidpointOutput, StringComparison.OrdinalIgnoreCase))
                return ExecuteClearMidpointOutput(request, signalType);
        }
        catch (Exception ex)
        {
            Debug.LogMessage(ex, "Error executing routing command: {Exception}");
            return Error(500, "executionError", ex.Message);
        }

        return Error(400, "unknownCommand", $"Unknown command '{request.Command}'.", "command");
    }

    // ── sinkRoute ────────────────────────────────────────────────────────────

    private static (int, RoutingCommandResponse) ExecuteSinkRoute(
        RoutingCommandRequest request,
        eRoutingSignalType signalType)
    {
        if (string.IsNullOrWhiteSpace(request.SourceDeviceKey))
            return Error(400, "missingField", "'sourceDeviceKey' is required for sinkRoute.", "sourceDeviceKey");

        if (!RoutingGraphHelpers.TryResolveRoutingInputTarget(
                request.DeviceKey, request.InputPortKey,
                out var destination, out var resolvedInputPortKey,
                out var resolveErrorCode, out var resolveErrorMessage))
        {
            return Error(StatusForCode(resolveErrorCode), resolveErrorCode, resolveErrorMessage, "deviceKey");
        }

        var sourceDevice = DeviceManager.GetDeviceForKey(request.SourceDeviceKey);
        if (sourceDevice == null)
            return Error(404, "deviceNotFound", $"No device with key '{request.SourceDeviceKey}'.", "sourceDeviceKey");

        if (sourceDevice is not IRoutingOutputs source)
        {
            return Error(422, "deviceNotRoutable",
                $"Device '{request.SourceDeviceKey}' does not implement IRoutingOutputs.", "sourceDeviceKey");
        }

        // A null port means "discover one", which is what ReleaseAndMakeRoute does with an empty key.
        RoutingInputPort destinationPort = null;
        if (!string.IsNullOrEmpty(resolvedInputPortKey))
        {
            destinationPort = destination.InputPorts[resolvedInputPortKey];
            if (destinationPort == null)
            {
                return Error(422, "portNotFound",
                    $"Device '{request.DeviceKey}' has no input port '{request.InputPortKey}'.", "inputPortKey");
            }

            if (!PortSupports(destinationPort, signalType))
            {
                return Error(422, "signalTypeNotSupportedByPort",
                    $"Input port '{request.InputPortKey}' carries {destinationPort.Type}, which cannot serve a {signalType} request.",
                    "signalType");
            }
        }

        RoutingOutputPort sourcePort = null;
        if (!string.IsNullOrEmpty(request.SourcePortKey))
        {
            sourcePort = source.OutputPorts[request.SourcePortKey];
            if (sourcePort == null)
            {
                return Error(422, "portNotFound",
                    $"Device '{request.SourceDeviceKey}' has no output port '{request.SourcePortKey}'.", "sourcePortKey");
            }

            if (!PortSupports(sourcePort, signalType))
            {
                return Error(422, "signalTypeNotSupportedByPort",
                    $"Output port '{request.SourcePortKey}' carries {sourcePort.Type}, which cannot serve a {signalType} request.",
                    "signalType");
            }
        }

        // Discover the path before touching anything. GetRouteToSource is a pure graph walk - it
        // builds RouteDescriptors and reads TieLineCollection, but never mutates
        // RouteDescriptorCollection.DefaultCollection and never calls ExecuteSwitch.
        var (audioOrSingle, video) = DiscoverRoute(destination, source, signalType, destinationPort, sourcePort);

        if (audioOrSingle == null && video == null)
        {
            return Error(409, "noRouteFound",
                $"No {signalType} path exists from '{request.SourceDeviceKey}' to '{request.DeviceKey}'"
                + (string.IsNullOrEmpty(request.InputPortKey) ? "." : $" input '{request.InputPortKey}'."));
        }

        var tileChildren = RoutingGraphHelpers.BuildTileChildMap();
        var response = BuildResponse(request, destination, resolvedInputPortKey, signalType);
        response.Steps = BuildSteps(tileChildren, audioOrSingle, video);
        response.EffectiveSignalType = EffectiveSignalType(audioOrSingle, video).ToString();
        // An AudioVideo request that resolved only one half. The processor still routes that half.
        response.Partial = signalType.HasFlag(eRoutingSignalType.AudioVideo)
            && (audioOrSingle == null || video == null);

        if (request.DryRun)
        {
            response.Status = "validated";
            return (200, response);
        }

        // Pass the DE-QUALIFIED port key: ReleaseAndMakeRoute resolves it against the resolved
        // destination's own InputPorts, which for a tile is the child sink, not the parent.
        destination.ReleaseAndMakeRoute(
            source,
            signalType,
            resolvedInputPortKey ?? string.Empty,
            request.SourcePortKey ?? string.Empty);

        response.Status = "accepted";
        return (202, response);
    }

    /// <summary>
    /// Finds the route the processor will actually run, mirroring <c>RunRouteRequest</c>'s own lookup
    /// order so the reported steps match what executes: a pre-mapped descriptor from
    /// <c>Extensions.RouteDescriptors</c> if one exists, otherwise a fresh graph walk.
    /// </summary>
    private static (RouteDescriptor AudioOrSingle, RouteDescriptor Video) DiscoverRoute(
        IRoutingInputs destination,
        IRoutingOutputs source,
        eRoutingSignalType signalType,
        RoutingInputPort destinationPort,
        RoutingOutputPort sourcePort)
    {
        RouteDescriptor audioOrSingle = null;
        RouteDescriptor video = null;

        if (signalType.HasFlag(eRoutingSignalType.AudioVideo))
        {
            audioOrSingle = FindPreMapped(eRoutingSignalType.Audio, destination, source, destinationPort, sourcePort);
            video = FindPreMapped(eRoutingSignalType.Video, destination, source, destinationPort, sourcePort);
        }
        else
        {
            audioOrSingle = FindPreMapped(signalType, destination, source, destinationPort, sourcePort);
        }

        if (audioOrSingle == null && video == null)
            return destination.GetRouteToSource(source, signalType, destinationPort, sourcePort);

        return (audioOrSingle, video);
    }

    private static RouteDescriptor FindPreMapped(
        eRoutingSignalType signalType,
        IRoutingInputs destination,
        IRoutingOutputs source,
        RoutingInputPort destinationPort,
        RoutingOutputPort sourcePort)
    {
        if (!Extensions.RouteDescriptors.TryGetValue(signalType, out var collection))
            return null;

        return collection.Descriptors.FirstOrDefault(d =>
            d.Source.Key == source.Key &&
            d.Destination.Key == destination.Key &&
            (destinationPort == null || d.InputPort?.Key == destinationPort.Key) &&
            (sourcePort == null || d.OutputPort?.Key == sourcePort.Key));
    }

    // ── midpointSwitch ───────────────────────────────────────────────────────

    private static (int, RoutingCommandResponse) ExecuteMidpointSwitch(
        RoutingCommandRequest request,
        eRoutingSignalType signalType)
    {
        if (string.IsNullOrWhiteSpace(request.InputPortKey))
            return Error(400, "missingField", "'inputPortKey' is required for midpointSwitch.", "inputPortKey");

        if (string.IsNullOrWhiteSpace(request.OutputPortKey))
            return Error(400, "missingField", "'outputPortKey' is required for midpointSwitch.", "outputPortKey");

        if (!TryResolveMidpoint(request.DeviceKey, out var midpoint, out var midpointError))
            return midpointError;

        var inputPort = midpoint.InputPorts[request.InputPortKey];
        if (inputPort == null)
        {
            return Error(422, "portNotFound",
                $"Device '{request.DeviceKey}' has no input port '{request.InputPortKey}'.", "inputPortKey");
        }

        var outputPort = midpoint.OutputPorts[request.OutputPortKey];
        if (outputPort == null)
        {
            return Error(422, "portNotFound",
                $"Device '{request.DeviceKey}' has no output port '{request.OutputPortKey}'.", "outputPortKey");
        }

        if (!PortSupports(inputPort, signalType))
        {
            return Error(422, "signalTypeNotSupportedByPort",
                $"Input port '{request.InputPortKey}' carries {inputPort.Type}, which cannot serve a {signalType} request.",
                "signalType");
        }

        if (!PortSupports(outputPort, signalType))
        {
            return Error(422, "signalTypeNotSupportedByPort",
                $"Output port '{request.OutputPortKey}' carries {outputPort.Type}, which cannot serve a {signalType} request.",
                "signalType");
        }

        var response = BuildResponse(request, midpoint, request.InputPortKey, signalType);
        response.ResolvedOutputPortKey = request.OutputPortKey;
        response.EffectiveSignalType = signalType.ToString();

        if (request.DryRun)
        {
            response.Status = "validated";
            return (200, response);
        }

        // The flags enum is passed through rather than split into separate audio and video calls:
        // DM/NVX drivers switch AudioVideo natively, and splitting would issue two discrete switches
        // (and two RouteChanged events) on hardware that wanted one. Breakaway is expressed by the
        // caller sending Audio or Video, already validated against the declared port types above.
        //
        // This runs inline on the CWS thread, like DevJsonRequestHandler, which is what lets the
        // response honestly report "executed". Every in-repo driver queues or fires and forgets; if
        // a blocking implementation ever shows up, give this its own GenericQueue and return 202.
        midpoint.ExecuteSwitch(inputPort.Selector, outputPort.Selector, signalType);

        response.Status = "executed";
        return (200, response);
    }

    // ── clearSink ────────────────────────────────────────────────────────────

    private static (int, RoutingCommandResponse) ExecuteClearSink(RoutingCommandRequest request)
    {
        if (!RoutingGraphHelpers.TryResolveRoutingInputTarget(
                request.DeviceKey, request.InputPortKey,
                out var destination, out var resolvedInputPortKey,
                out var resolveErrorCode, out var resolveErrorMessage))
        {
            return Error(StatusForCode(resolveErrorCode), resolveErrorCode, resolveErrorMessage, "deviceKey");
        }

        // No signal-type check: clearing is type-agnostic, and ReleaseRouteInternal derives the type
        // from whatever descriptor it finds.
        if (!string.IsNullOrEmpty(resolvedInputPortKey) && destination.InputPorts[resolvedInputPortKey] == null)
        {
            return Error(422, "portNotFound",
                $"Device '{request.DeviceKey}' has no input port '{request.InputPortKey}'.", "inputPortKey");
        }

        var response = BuildResponse(request, destination, resolvedInputPortKey, null);

        if (request.DryRun)
        {
            response.Status = "validated";
            return (200, response);
        }

        if (string.IsNullOrEmpty(resolvedInputPortKey))
        {
            if (request.ReleaseOnly) destination.ReleaseRoute();
            else destination.ClearRoute();
        }
        else
        {
            if (request.ReleaseOnly) destination.ReleaseRoute(resolvedInputPortKey);
            else destination.ClearRoute(resolvedInputPortKey);
        }

        // RouteDescriptor.ReleaseRoutes only tears down switches whose device is an
        // IRoutingMidpointWithFeedback, so a clear never touches the destination's own input - a
        // cleared display would otherwise sit there still showing its last source.
        if (request.ClearSinkInput && destination is IRoutingSinkWithFeedback sink)
        {
            try
            {
                sink.ExecuteSwitch(null);
            }
            catch (Exception ex)
            {
                // Not every driver tolerates a null selector. The route was still released, so log
                // and report success rather than failing the whole command.
                Debug.LogMessage(LogEventLevel.Warning,
                    "Could not deselect input on '{deviceKey}' while clearing: {message}",
                    null, request.DeviceKey, ex.Message);
            }
        }

        response.Status = "accepted";
        return (202, response);
    }

    // ── clearMidpointOutput ──────────────────────────────────────────────────

    private static (int, RoutingCommandResponse) ExecuteClearMidpointOutput(
        RoutingCommandRequest request,
        eRoutingSignalType signalType)
    {
        if (string.IsNullOrWhiteSpace(request.OutputPortKey))
            return Error(400, "missingField", "'outputPortKey' is required for clearMidpointOutput.", "outputPortKey");

        if (!TryResolveMidpoint(request.DeviceKey, out var midpoint, out var midpointError))
            return midpointError;

        var outputPort = midpoint.OutputPorts[request.OutputPortKey];
        if (outputPort == null)
        {
            return Error(422, "portNotFound",
                $"Device '{request.DeviceKey}' has no output port '{request.OutputPortKey}'.", "outputPortKey");
        }

        if (!PortSupports(outputPort, signalType))
        {
            return Error(422, "signalTypeNotSupportedByPort",
                $"Output port '{request.OutputPortKey}' carries {outputPort.Type}, which cannot serve a {signalType} request.",
                "signalType");
        }

        var response = BuildResponse(request, midpoint, null, signalType);
        response.ResolvedOutputPortKey = request.OutputPortKey;
        response.EffectiveSignalType = signalType.ToString();

        if (request.DryRun)
        {
            response.Status = "validated";
            return (200, response);
        }

        midpoint.ClearRoute(outputPort.Selector, signalType);

        response.Status = "executed";
        return (200, response);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool TryResolveMidpoint(
        string deviceKey,
        out IRoutingMidpointWithFeedback midpoint,
        out (int, RoutingCommandResponse) error)
    {
        midpoint = null;
        error = default;

        var device = DeviceManager.GetDeviceForKey(deviceKey);
        if (device == null)
        {
            error = Error(404, "deviceNotFound", $"No device with key '{deviceKey}'.", "deviceKey");
            return false;
        }

        if (device is not IRoutingMidpointWithFeedback found)
        {
            error = Error(422, "deviceNotRoutable",
                $"Device '{deviceKey}' does not implement IRoutingMidpointWithFeedback.", "deviceKey");
            return false;
        }

        midpoint = found;
        return true;
    }

    /// <summary>
    /// The client-facing mirror of the port compatibility rule: a port must carry every requested
    /// flag. So an AudioVideo port satisfies an Audio request, a Video port does not satisfy an
    /// AudioVideo request, and Usb is isolated on its own bit.
    /// </summary>
    private static bool PortSupports(RoutingPort port, eRoutingSignalType requested)
        => (port.Type & requested) == requested;

    /// <summary>
    /// Parses a signal type name, defaulting to AudioVideo when absent.
    /// </summary>
    /// <remarks>
    /// <see cref="Enum.TryParse{TEnum}(string, bool, out TEnum)"/> alone accepts arbitrary numeric
    /// strings - "7" and "0" both parse happily - so the result is also checked against the defined
    /// values. This keeps the wire format to the four names the API documents.
    /// </remarks>
    private static bool TryParseSignalType(string value, out eRoutingSignalType signalType, out string error)
    {
        error = null;
        var text = string.IsNullOrWhiteSpace(value) ? nameof(eRoutingSignalType.AudioVideo) : value.Trim();

        if (!Enum.TryParse(text, true, out signalType) || !Enum.IsDefined(typeof(eRoutingSignalType), signalType))
        {
            error = $"'{value}' is not a valid signal type. Expected Audio, Video, AudioVideo or Usb.";
            return false;
        }

        return true;
    }

    /// <summary>The union of the signal types the discovered descriptors will actually switch.</summary>
    /// <remarks>
    /// This can be WIDER than the request. A pre-mapped descriptor built by
    /// <c>MapDestinationsToSources</c> takes its type from the port's declared type, so an Audio-only
    /// request across all-AudioVideo ports executes as AudioVideo. Reporting it lets a client say
    /// "requested Audio, executed AudioVideo" rather than implying breakaway that did not happen.
    /// </remarks>
    private static eRoutingSignalType EffectiveSignalType(RouteDescriptor audioOrSingle, RouteDescriptor video)
    {
        var effective = default(eRoutingSignalType);
        if (audioOrSingle != null) effective |= audioOrSingle.SignalType;
        if (video != null) effective |= video.SignalType;
        return effective;
    }

    private static List<RoutingCommandStepInfo> BuildSteps(
        Dictionary<string, RoutingGraphHelpers.TileChildInfo> tileChildren,
        params RouteDescriptor[] descriptors)
    {
        var steps = new List<RoutingCommandStepInfo>();

        foreach (var descriptor in descriptors.Where(d => d != null))
        {
            foreach (var route in descriptor.Routes)
            {
                var switchingDeviceKey = route.SwitchingDevice?.Key;
                var (graphDeviceKey, graphInputPortKey) =
                    RoutingGraphHelpers.ToGraphKeys(tileChildren, switchingDeviceKey, route.InputPort?.Key);

                steps.Add(new RoutingCommandStepInfo
                {
                    SignalType = descriptor.SignalType.ToString(),
                    SwitchingDeviceKey = graphDeviceKey,
                    InputPortKey = graphInputPortKey,
                    // Null on the final step onto a sink, which has no output port.
                    OutputPortKey = route.OutputPort?.Key
                });
            }
        }

        return steps;
    }

    private static RoutingCommandResponse BuildResponse(
        RoutingCommandRequest request,
        IKeyed resolvedDevice,
        string resolvedInputPortKey,
        eRoutingSignalType? signalType)
    {
        return new RoutingCommandResponse
        {
            Command = request.Command,
            DeviceKey = request.DeviceKey,
            ResolvedDeviceKey = resolvedDevice?.Key,
            ResolvedInputPortKey = resolvedInputPortKey,
            SignalType = signalType?.ToString()
        };
    }

    /// <summary>
    /// 404 means the key is wrong; 422 means the keys exist but the request is impossible on that
    /// device. Kept in one place so the resolver's codes map consistently.
    /// </summary>
    private static int StatusForCode(string errorCode)
        => errorCode == "deviceNotFound" ? 404 : 422;

    private static (int, RoutingCommandResponse) Error(
        int statusCode,
        string code,
        string message,
        string field = null)
    {
        return (statusCode, new RoutingCommandResponse
        {
            Status = "error",
            Error = new RoutingCommandError { Code = code, Message = message, Field = field }
        });
    }
}

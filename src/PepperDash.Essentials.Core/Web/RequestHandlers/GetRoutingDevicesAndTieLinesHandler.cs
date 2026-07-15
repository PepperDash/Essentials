using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    /// <summary>
    /// Handles HTTP requests to retrieve routing devices and tielines information
    /// </summary>
    public class GetRoutingDevicesAndTieLinesHandler : WebApiBaseRequestHandler
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRoutingDevicesAndTieLinesHandler"/> class.
        /// </summary>
        public GetRoutingDevicesAndTieLinesHandler() : base(true) { }

        /// <summary>
        /// Handles the GET request to retrieve routing devices and tielines information
        /// </summary>
        /// <param name="context"></param>
        protected override void HandleGet(HttpCwsContext context)
        {
            var devices = new List<RoutingDeviceInfo>();

            // Tile-sink children of an IRoutingSinkWithLayouts device (e.g. a multiview decoder's
            // per-window sinks) are rendered as synthesized input ports on the parent's node instead
            // of as their own separate nodes - see BuildRoutingDeviceInfo below.
            var tileChildren = RoutingGraphHelpers.BuildTileChildMap();

            // Get all devices from DeviceManager
            foreach (var device in DeviceManager.AllDevices)
            {
                if (tileChildren.ContainsKey(device.Key))
                    continue;

                var deviceInfo = new RoutingDeviceInfo
                {
                    Key = device.Key,
                    Name = (device as IKeyName)?.Name ?? device.Key
                };

                // Check if device implements IRoutingInputs
                if (device is IRoutingInputs inputDevice)
                {
                    deviceInfo.HasInputs = true;
                    deviceInfo.InputPorts = [.. inputDevice.InputPorts.Select(p => new PortInfo
                    {
                        Key = p.Key,
                        SignalType = p.Type.ToString(),
                        ConnectionType = p.ConnectionType.ToString(),
                        IsInternal = p.IsInternal
                    })];
                }

                // Check if device implements IRoutingOutputs
                if (device is IRoutingOutputs outputDevice)
                {
                    deviceInfo.HasOutputs = true;
                    deviceInfo.OutputPorts = [.. outputDevice.OutputPorts.Select(p => new PortInfo
                    {
                        Key = p.Key,
                        SignalType = p.Type.ToString(),
                        ConnectionType = p.ConnectionType.ToString(),
                        IsInternal = p.IsInternal
                    })];
                }

                // Check if device implements IRoutingMidpoint
                if (device is IRoutingMidpoint)
                {
                    deviceInfo.HasInputsAndOutputs = true;
                }

                // Devices implementing IRoutingSinkWithLayouts (e.g. a multiview decoder) don't
                // implement IRoutingInputs themselves - their tiles do. Synthesize one input port per
                // tile (qualified so multiple tiles don't collide) so the graph can render a single
                // node with one edge-target input per tile.
                if (device is IRoutingSinkWithLayouts layoutDevice)
                {
                    deviceInfo.HasInputs = true;

                    var tilePorts = layoutDevice.WindowTileSinks
                        .OrderBy(kvp => kvp.Key)
                        .SelectMany(kvp => (kvp.Value as IRoutingInputs)?.InputPorts.Select(p => new PortInfo
                        {
                            Key = RoutingGraphHelpers.QualifyTilePortKey(kvp.Key, p.Key),
                            SignalType = p.Type.ToString(),
                            ConnectionType = p.ConnectionType.ToString(),
                            IsInternal = p.IsInternal
                        }) ?? []);

                    deviceInfo.InputPorts = (deviceInfo.InputPorts ?? []).Concat(tilePorts).ToList();
                }

                // Only include devices that have routing capabilities
                if (deviceInfo.HasInputs || deviceInfo.HasOutputs)
                {
                    devices.Add(deviceInfo);
                }
            }

            // Get all tielines, remapping any that target a tile-sink child so they point at its
            // IRoutingSinkWithLayouts parent's node/qualified port instead.
            var tielines = TieLineCollection.Default.Select(tl =>
            {
                var destinationDeviceKey = tl.DestinationPort.ParentDevice.Key;
                var destinationPortKey = tl.DestinationPort.Key;

                if (tileChildren.TryGetValue(destinationDeviceKey, out var tileInfo))
                {
                    destinationDeviceKey = tileInfo.Parent.Key;
                    destinationPortKey = RoutingGraphHelpers.QualifyTilePortKey(tileInfo.TileNumber, destinationPortKey);
                }

                return new TieLineInfo
                {
                    SourceDeviceKey = tl.SourcePort.ParentDevice.Key,
                    SourcePortKey = tl.SourcePort.Key,
                    DestinationDeviceKey = destinationDeviceKey,
                    DestinationPortKey = destinationPortKey,
                    SignalType = tl.Type.ToString(),
                    IsInternal = tl.IsInternal
                };
            }).ToList();

            // Get current active routes from DefaultCollection, grouped by signal type
            var currentRoutes = RouteDescriptorCollection.DefaultCollection.Descriptors
                .GroupBy(d => d.SignalType.ToString())
                .Select(g => new CurrentRouteGroupInfo
                {
                    SignalType = g.Key,
                    Routes = [.. g.Select(d =>
                    {
                        var destinationDeviceKey = d.Destination.Key;
                        var destinationInputPortKey = d.InputPort?.Key;

                        if (tileChildren.TryGetValue(destinationDeviceKey, out var tileInfo))
                        {
                            destinationDeviceKey = tileInfo.Parent.Key;
                            if (destinationInputPortKey != null)
                                destinationInputPortKey = RoutingGraphHelpers.QualifyTilePortKey(tileInfo.TileNumber, destinationInputPortKey);
                        }

                        return new ActiveRouteInfo
                        {
                            SourceDeviceKey = d.Source.Key,
                            DestinationDeviceKey = destinationDeviceKey,
                            DestinationInputPortKey = destinationInputPortKey,
                            Steps = [.. d.Routes.Select(r =>
                            {
                                var switchingDeviceKey = r.SwitchingDevice?.Key;
                                var inputPortKey = r.InputPort?.Key;

                                if (switchingDeviceKey != null && tileChildren.TryGetValue(switchingDeviceKey, out var stepTileInfo))
                                {
                                    switchingDeviceKey = stepTileInfo.Parent.Key;
                                    if (inputPortKey != null)
                                        inputPortKey = RoutingGraphHelpers.QualifyTilePortKey(stepTileInfo.TileNumber, inputPortKey);
                                }

                                return new RouteSwitchStepInfo
                                {
                                    SwitchingDeviceKey = switchingDeviceKey,
                                    InputPortKey = inputPortKey,
                                    OutputPortKey = r.OutputPort?.Key
                                };
                            })]
                        };
                    })]
                }).ToList();

            var response = new RoutingSystemInfo
            {
                Devices = devices,
                TieLines = tielines,
                CurrentRoutes = currentRoutes,
                SinkCurrentSources = BuildSinkCurrentSources(tileChildren),
                MultiviewLayouts = RoutingGraphHelpers.BuildMultiviewLayoutSnapshot()
            };

            var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(jsonResponse, false);
            context.Response.End();
        }

        /// <summary>
        /// Builds current-source info for every sink device, read directly from each sink's own
        /// <see cref="ICurrentSources"/> bookkeeping (see <see cref="RoutingGraphHelpers.GetCurrentSourceKey"/>).
        /// Unlike <see cref="RouteDescriptorCollection"/>-based <see cref="CurrentRouteGroupInfo"/>, this
        /// also reflects routes made via a device-specific bulk API (e.g.
        /// <c>IHasDynamicMultiviewLayout.ApplyDynamicLayout</c>) that never creates a
        /// <see cref="RouteDescriptor"/> or <see cref="TieLine"/> at all, so it's what the dev tools app
        /// should use to seed initial sink-routing state on page load.
        /// </summary>
        private static List<SinkCurrentSourceInfo> BuildSinkCurrentSources(
            Dictionary<string, RoutingGraphHelpers.TileChildInfo> tileChildren)
        {
            var result = new List<SinkCurrentSourceInfo>();

            foreach (var device in DeviceManager.AllDevices.OfType<IRoutingSinkWithFeedback>())
            {
                if (device.CurrentInputPort == null)
                    continue;

                var sourceKey = RoutingGraphHelpers.GetCurrentSourceKey(device);
                if (string.IsNullOrEmpty(sourceKey))
                    continue;

                var deviceKey = device.Key;
                var inputPortKey = device.CurrentInputPort.Key;

                if (tileChildren.TryGetValue(deviceKey, out var tileInfo))
                {
                    deviceKey = tileInfo.Parent.Key;
                    inputPortKey = RoutingGraphHelpers.QualifyTilePortKey(tileInfo.TileNumber, inputPortKey);
                }

                result.Add(new SinkCurrentSourceInfo
                {
                    DeviceKey = deviceKey,
                    InputPortKey = inputPortKey,
                    SourceDeviceKey = sourceKey,
                    SignalType = device.CurrentInputPort.Type.ToString()
                });
            }

            return result;
        }
    }

    /// <summary>
    /// Represents the complete routing system information including devices and tielines
    /// </summary>
    public class RoutingSystemInfo
    {

        /// <summary>
        /// Gets or sets the list of routing devices in the system, including their ports information
        /// </summary>
        [JsonProperty("devices")]
        public List<RoutingDeviceInfo> Devices { get; set; }


        /// <summary>
        /// Gets or sets the list of tielines in the system, including source/destination device and port information
        /// </summary>
        [JsonProperty("tieLines")]
        public List<TieLineInfo> TieLines { get; set; }

        /// <summary>
        /// Gets or sets the current active routes in the system, grouped by signal type
        /// </summary>
        [JsonProperty("currentRoutes")]
        public List<CurrentRouteGroupInfo> CurrentRoutes { get; set; }

        /// <summary>
        /// Gets or sets the current source feeding each sink device, read directly from each sink's own
        /// current-source bookkeeping. Covers routes made via device-specific bulk APIs (e.g.
        /// dynamic multiview layouts) that <see cref="CurrentRoutes"/> does not.
        /// </summary>
        [JsonProperty("sinkCurrentSources")]
        public List<SinkCurrentSourceInfo> SinkCurrentSources { get; set; }

        /// <summary>
        /// Gets or sets the current multiview canvas/tile layout for every device implementing
        /// <see cref="IRoutingSinkWithLayoutState"/>, keyed by device key. Devices with no currently
        /// active layout are omitted. Lets a client render an initial visual mock-up of each
        /// multiview decoder's monitor output without waiting for a routing feedback WebSocket
        /// connection - see <see cref="RoutingGraphHelpers.BuildMultiviewLayoutSnapshot"/>.
        /// </summary>
        [JsonProperty("multiviewLayouts")]
        public Dictionary<string, MultiviewLayoutState> MultiviewLayouts { get; set; }
    }

    /// <summary>
    /// Represents the source currently feeding a single sink device's input port
    /// </summary>
    public class SinkCurrentSourceInfo
    {
        /// <summary>
        /// Gets or sets the key of the sink device (or its IRoutingSinkWithLayouts parent, if this is a tile)
        /// </summary>
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the input port currently receiving the source
        /// </summary>
        [JsonProperty("inputPortKey")]
        public string InputPortKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the device currently feeding this input
        /// </summary>
        [JsonProperty("sourceDeviceKey")]
        public string SourceDeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the signal type of the input port (e.g., AudioVideo, Audio, Video, etc.)
        /// </summary>
        [JsonProperty("signalType")]
        public string SignalType { get; set; }
    }

    /// <summary>
    /// Represents a routing device with its ports information
    /// </summary>
    public class RoutingDeviceInfo : IKeyName
    {

        /// <inheritdoc />
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <inheritdoc />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device has routing input ports
        /// </summary>
        [JsonProperty("hasInputs")]
        public bool HasInputs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device has routing output ports
        /// </summary>
        [JsonProperty("hasOutputs")]
        public bool HasOutputs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device has both routing inputs and outputs (e.g., matrix switcher)
        /// </summary>
        [JsonProperty("hasInputsAndOutputs")]
        public bool HasInputsAndOutputs { get; set; }

        /// <summary>
        /// Gets or sets the list of input ports for the device, if applicable. Null if the device does not have routing inputs.
        /// </summary>
        [JsonProperty("inputPorts", NullValueHandling = NullValueHandling.Ignore)]
        public List<PortInfo> InputPorts { get; set; }

        /// <summary>
        /// Gets or sets the list of output ports for the device, if applicable. Null if the device does not have routing outputs.
        /// </summary>
        [JsonProperty("outputPorts", NullValueHandling = NullValueHandling.Ignore)]
        public List<PortInfo> OutputPorts { get; set; }
    }

    /// <summary>
    /// Represents a routing port with its properties
    /// </summary>
    public class PortInfo : IKeyed
    {
        /// <inheritdoc />
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the signal type of the port (e.g., AudioVideo, Audio, Video, etc.)
        /// </summary>
        [JsonProperty("signalType")]
        public string SignalType { get; set; }

        /// <summary>
        /// Gets or sets the connection type of the port (e.g., Hdmi, Dvi, Vga, etc.)
        /// </summary>
        [JsonProperty("connectionType")]
        public string ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the port is internal
        /// </summary>
        [JsonProperty("isInternal")]
        public bool IsInternal { get; set; }
    }

    /// <summary>
    /// Represents a tieline connection between two ports
    /// </summary>
    public class TieLineInfo
    {
        /// <summary>
        /// Gets or sets the key of the source device for the tieline connection
        /// </summary>
        [JsonProperty("sourceDeviceKey")]
        public string SourceDeviceKey { get; set; }


        /// <summary>
        /// Gets or sets the key of the source port for the tieline connection
        /// </summary>
        [JsonProperty("sourcePortKey")]
        public string SourcePortKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the destination device for the tieline connection
        /// </summary>
        [JsonProperty("destinationDeviceKey")]
        public string DestinationDeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the destination port for the tieline connection
        /// </summary>
        [JsonProperty("destinationPortKey")]
        public string DestinationPortKey { get; set; }

        /// <summary>
        /// Gets or sets the signal type of the tieline connection (e.g., AudioVideo, Audio, Video, etc.)
        /// </summary>
        [JsonProperty("signalType")]
        public string SignalType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tieline connection is internal
        /// </summary>
        [JsonProperty("isInternal")]
        public bool IsInternal { get; set; }
    }

    /// <summary>
    /// Represents a group of active routes for a given signal type
    /// </summary>
    public class CurrentRouteGroupInfo
    {
        /// <summary>
        /// Gets or sets the signal type for the group of active routes (e.g., AudioVideo, Audio, Video, etc.)
        /// </summary>
        [JsonProperty("signalType")]
        public string SignalType { get; set; }

        /// <summary>
        /// Gets or sets the list of active routes for the given signal type
        /// </summary>
        [JsonProperty("routes")]
        public List<ActiveRouteInfo> Routes { get; set; }
    }

    /// <summary>
    /// Represents a single active route from a source to a destination
    /// </summary>
    public class ActiveRouteInfo
    {
        /// <summary>
        /// Gets or sets the key of the source device for the active route
        /// </summary>
        [JsonProperty("sourceDeviceKey")]
        public string SourceDeviceKey { get; set; }

        /// <summary> 
        /// Gets or sets the key of the destination device for the active route
        /// </summary>
        [JsonProperty("destinationDeviceKey")]
        public string DestinationDeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the destination input port for the active route, if applicable
        /// </summary>
        [JsonProperty("destinationInputPortKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DestinationInputPortKey { get; set; }

        /// <summary>
        /// Gets or sets the list of switching steps for the active route
        /// </summary>
        [JsonProperty("steps")]
        public List<RouteSwitchStepInfo> Steps { get; set; }
    }

    /// <summary>
    /// Represents a single switching step within a route
    /// </summary>
    public class RouteSwitchStepInfo
    {
        /// <summary>
        /// Gets or sets the key of the switching device for the route step
        /// </summary>
        [JsonProperty("switchingDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchingDeviceKey { get; set; }


        /// <summary>
        /// Gets or sets the key of the input port for the route step, if applicable
        /// </summary>
        [JsonProperty("inputPortKey", NullValueHandling = NullValueHandling.Ignore)]
        public string InputPortKey { get; set; }

        /// <summary>
        /// Gets or sets the key of the output port for the route step, if applicable
        /// </summary>
        [JsonProperty("outputPortKey", NullValueHandling = NullValueHandling.Ignore)]
        public string OutputPortKey { get; set; }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

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

            // Get all devices from DeviceManager
            foreach (var device in DeviceManager.AllDevices)
            {
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

                // Check if device implements IRoutingInputsOutputs
                if (device is IRoutingInputsOutputs)
                {
                    deviceInfo.HasInputsAndOutputs = true;
                }

                // Only include devices that have routing capabilities
                if (deviceInfo.HasInputs || deviceInfo.HasOutputs)
                {
                    devices.Add(deviceInfo);
                }
            }

            // Get all tielines
            var tielines = TieLineCollection.Default.Select(tl => new TieLineInfo
            {
                SourceDeviceKey = tl.SourcePort.ParentDevice.Key,
                SourcePortKey = tl.SourcePort.Key,
                DestinationDeviceKey = tl.DestinationPort.ParentDevice.Key,
                DestinationPortKey = tl.DestinationPort.Key,
                SignalType = tl.Type.ToString(),
                IsInternal = tl.IsInternal
            }).ToList();

            // Get current active routes from DefaultCollection, grouped by signal type
            var currentRoutes = RouteDescriptorCollection.DefaultCollection.Descriptors
                .GroupBy(d => d.SignalType.ToString())
                .Select(g => new CurrentRouteGroupInfo
                {
                    SignalType = g.Key,
                    Routes = [.. g.Select(d => new ActiveRouteInfo
                    {
                        SourceDeviceKey = d.Source.Key,
                        DestinationDeviceKey = d.Destination.Key,
                        DestinationInputPortKey = d.InputPort?.Key,
                        Steps = [.. d.Routes.Select(r => new RouteSwitchStepInfo
                        {
                            SwitchingDeviceKey = r.SwitchingDevice?.Key,
                            InputPortKey = r.InputPort?.Key,
                            OutputPortKey = r.OutputPort?.Key
                        })]
                    })]
                }).ToList();

            var response = new RoutingSystemInfo
            {
                Devices = devices,
                TieLines = tielines,
                CurrentRoutes = currentRoutes
            };

            var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(jsonResponse, false);
            context.Response.End();
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
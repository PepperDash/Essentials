using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using Serilog.Events;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implment IMatrixRouting
    /// </summary>
    public class IMatrixRoutingMessenger : MessengerBase
    {
        private readonly IMatrixRouting matrixDevice;

        /// <summary>
        /// Initializes a new instance of the IMatrixRoutingMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements IMatrixRouting</param>
        public IMatrixRoutingMessenger(string key, string messagePath, IMatrixRouting device) : base(key, messagePath, device as IKeyName)
        {
            matrixDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) =>
            {
                try
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "InputCount: {inputCount}, OutputCount: {outputCount}", this, matrixDevice.InputSlots.Count, matrixDevice.OutputSlots.Count);
                    var message = new MatrixStateMessage
                    {
                        Outputs = matrixDevice.OutputSlots.ToDictionary(kvp => kvp.Key, kvp => new RoutingOutput(kvp.Value)),
                        Inputs = matrixDevice.InputSlots.ToDictionary(kvp => kvp.Key, kvp => new RoutingInput(kvp.Value)),
                    };


                    PostStatusMessage(message, id);
                }
                catch (Exception e)
                {
                    Debug.LogMessage(e, "Exception Getting full status: {@exception}", this, e);
                }
            });

            AddAction("/route", (id, content) =>
            {
                var request = content.ToObject<MatrixRouteRequest>();

                matrixDevice.Route(request.InputKey, request.OutputKey, request.RouteType);
            });

            foreach (var output in matrixDevice.OutputSlots)
            {
                var key = output.Key;
                var outputSlot = output.Value;

                outputSlot.OutputSlotChanged += (sender, args) =>
                {
                    PostStatusMessage(JToken.FromObject(new
                    {
                        outputs = matrixDevice.OutputSlots.ToDictionary(kvp => kvp.Key, kvp => new RoutingOutput(kvp.Value))
                    }));
                };
            }

            foreach (var input in matrixDevice.InputSlots)
            {
                var key = input.Key;
                var inputSlot = input.Value;

                inputSlot.VideoSyncChanged += (sender, args) =>
                {
                    PostStatusMessage(JToken.FromObject(new
                    {
                        inputs = matrixDevice.InputSlots.ToDictionary(kvp => kvp.Key, kvp => new RoutingInput(kvp.Value))
                    }));
                };
            }
        }
    }

    /// <summary>
    /// State message for matrix routing information
    /// </summary>
    public class MatrixStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the outputs dictionary
        /// </summary>
        [JsonProperty("outputs")]
        public Dictionary<string, RoutingOutput> Outputs;

        /// <summary>
        /// Gets or sets the inputs dictionary
        /// </summary>
        [JsonProperty("inputs")]
        public Dictionary<string, RoutingInput> Inputs;
    }

    /// <summary>
    /// Represents a routing input slot
    /// </summary>
    public class RoutingInput
    {
        private IRoutingInputSlot _input;

        /// <summary>
        /// Gets the transmit device key
        /// </summary>
        [JsonProperty("txDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string TxDeviceKey => _input?.TxDeviceKey;

        /// <summary>
        /// Gets the slot number
        /// </summary>
        [JsonProperty("slotNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int? SlotNumber => _input?.SlotNumber;

        /// <summary>
        /// Gets the supported signal types
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("supportedSignalTypes", NullValueHandling = NullValueHandling.Ignore)]
        public eRoutingSignalType? SupportedSignalTypes => _input?.SupportedSignalTypes;

        /// <summary>
        /// Gets the name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name => _input?.Name;

        /// <summary>
        /// Gets the online status
        /// </summary>
        [JsonProperty("isOnline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOnline => _input?.IsOnline.BoolValue;

        /// <summary>
        /// Gets the video sync detected status
        /// </summary>
        [JsonProperty("videoSyncDetected", NullValueHandling = NullValueHandling.Ignore)]

        public bool? VideoSyncDetected => _input?.VideoSyncDetected;

        /// <summary>
        /// Gets the key
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key => _input?.Key;

        /// <summary>
        /// Initializes a new instance of the RoutingInput class
        /// </summary>
        /// <param name="input">The input slot</param>
        public RoutingInput(IRoutingInputSlot input)
        {
            _input = input;
        }
    }

    /// <summary>
    /// Represents a routing output slot
    /// </summary>
    public class RoutingOutput
    {
        private IRoutingOutputSlot _output;

        /// <summary>
        /// Initializes a new instance of the RoutingOutput class
        /// </summary>
        /// <param name="output">The output slot</param>
        public RoutingOutput(IRoutingOutputSlot output)
        {
            _output = output;
        }

        /// <summary>
        /// Gets the receive device key
        /// </summary>
        [JsonProperty("rxDeviceKey")]
        public string RxDeviceKey => _output.RxDeviceKey;

        /// <summary>
        /// Gets the current routes
        /// </summary>
        [JsonProperty("currentRoutes")]
        public Dictionary<string, RoutingInput> CurrentRoutes => _output.CurrentRoutes.ToDictionary(kvp => kvp.Key.ToString(), kvp => new RoutingInput(kvp.Value));

        /// <summary>
        /// Gets the slot number
        /// </summary>
        [JsonProperty("slotNumber")]
        public int SlotNumber => _output.SlotNumber;

        /// <summary>
        /// Gets the supported signal types
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("supportedSignalTypes")]
        public eRoutingSignalType SupportedSignalTypes => _output.SupportedSignalTypes;

        /// <summary>
        /// Gets the name
        /// </summary>
        [JsonProperty("name")]
        public string Name => _output.Name;

        /// <summary>
        /// Gets the key
        /// </summary>
        [JsonProperty("key")]
        public string Key => _output.Key;
    }

    /// <summary>
    /// Request for matrix routing
    /// </summary>
    public class MatrixRouteRequest
    {
        /// <summary>
        /// Gets or sets the output key
        /// </summary>
        [JsonProperty("outputKey")]
        public string OutputKey { get; set; }

        /// <summary>
        /// Gets or sets the input key
        /// </summary>
        [JsonProperty("inputKey")]
        public string InputKey { get; set; }

        /// <summary>
        /// Gets or sets the route type
        /// </summary>
        [JsonProperty("routeType")]
        public eRoutingSignalType RouteType { get; set; }
    }
}

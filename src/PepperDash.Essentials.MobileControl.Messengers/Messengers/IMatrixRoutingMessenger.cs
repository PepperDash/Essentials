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
        public IMatrixRoutingMessenger(string key, string messagePath, IMatrixRouting device) : base(key, messagePath, device as IKeyName)
        {
            matrixDevice = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/matrixStatus", (id, content) => SendFullStatus(id));

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

                inputSlot.IsOnline.OutputChange += (sender, args) =>
                {
                    PostStatusMessage(JToken.FromObject(new
                    {
                        inputs = matrixDevice.InputSlots.ToDictionary(kvp => kvp.Key, kvp => new RoutingInput(kvp.Value))
                    }));
                };
            }
        }

        private void SendFullStatus(string id = null)
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
        }
    }

    /// <summary>
    /// Represents a MatrixStateMessage
    /// </summary>
    public class MatrixStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("outputs")]
        public Dictionary<string, RoutingOutput> Outputs;

        [JsonProperty("inputs")]
        public Dictionary<string, RoutingInput> Inputs;
    }

    /// <summary>
    /// Represents a RoutingInput
    /// </summary>
    public class RoutingInput
    {
        private IRoutingInputSlot _input;

        [JsonProperty("txDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string TxDeviceKey => _input?.TxDeviceKey;

        [JsonProperty("slotNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int? SlotNumber => _input?.SlotNumber;

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("supportedSignalTypes", NullValueHandling = NullValueHandling.Ignore)]
        public eRoutingSignalType? SupportedSignalTypes => _input?.SupportedSignalTypes;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name => _input?.Name;

        [JsonProperty("isOnline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOnline => _input?.IsOnline.BoolValue;

        [JsonProperty("videoSyncDetected", NullValueHandling = NullValueHandling.Ignore)]

        public bool? VideoSyncDetected => _input?.VideoSyncDetected;

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key => _input?.Key;

        public RoutingInput(IRoutingInputSlot input)
        {
            _input = input;
        }
    }

    /// <summary>
    /// Represents a RoutingOutput
    /// </summary>
    public class RoutingOutput
    {
        private IRoutingOutputSlot _output;


        public RoutingOutput(IRoutingOutputSlot output)
        {
            _output = output;
        }

        [JsonProperty("rxDeviceKey")]
        public string RxDeviceKey => _output.RxDeviceKey;

        [JsonProperty("currentRoutes")]
        public Dictionary<string, RoutingInput> CurrentRoutes => _output.CurrentRoutes.ToDictionary(kvp => kvp.Key.ToString(), kvp => new RoutingInput(kvp.Value));

        [JsonProperty("slotNumber")]
        public int SlotNumber => _output.SlotNumber;

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("supportedSignalTypes")]
        public eRoutingSignalType SupportedSignalTypes => _output.SupportedSignalTypes;

        [JsonProperty("name")]
        public string Name => _output.Name;

        [JsonProperty("key")]
        public string Key => _output.Key;
    }

    /// <summary>
    /// Represents a MatrixRouteRequest
    /// </summary>
    public class MatrixRouteRequest
    {
        [JsonProperty("outputKey")]
        /// <summary>
        /// Gets or sets the OutputKey
        /// </summary>
        public string OutputKey { get; set; }

        [JsonProperty("inputKey")]
        /// <summary>
        /// Gets or sets the InputKey
        /// </summary>
        public string InputKey { get; set; }

        [JsonProperty("routeType")]
        /// <summary>
        /// Gets or sets the RouteType
        /// </summary>
        public eRoutingSignalType RouteType { get; set; }
    }
}

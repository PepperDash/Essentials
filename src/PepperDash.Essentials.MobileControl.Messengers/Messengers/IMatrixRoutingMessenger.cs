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
        /// Initializes a new instance of the <see cref="IMatrixRoutingMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
                public IMatrixRoutingMessenger(string key, string messagePath, IMatrixRouting device) : base(key, messagePath, device as IKeyName)
        {
            matrixDevice = device;
        }

        /// <inheritdoc />
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
        /// <summary>
        /// Gets or sets the Outputs
        /// </summary>
        [JsonProperty("outputs")]
        public Dictionary<string, RoutingOutput> Outputs;

        /// <summary>
        /// Gets or sets the Inputs
        /// </summary>
        [JsonProperty("inputs")]
        public Dictionary<string, RoutingInput> Inputs;
    }

    /// <summary>
    /// Represents a RoutingInput
    /// </summary>
    public class RoutingInput : IKeyName
    {
        private IRoutingInputSlot _input;

        /// <summary>
        /// Gets the TxDeviceKey of the input slot
        /// </summary>
        [JsonProperty("txDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string TxDeviceKey => _input?.TxDeviceKey;

        /// <summary>
        /// Gets the SlotNumber of the input slot
         ///
        /// </summary>
        [JsonProperty("slotNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int? SlotNumber => _input?.SlotNumber;

        /// <summary>
        /// Gets the SupportedSignalTypes of the input slot
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("supportedSignalTypes", NullValueHandling = NullValueHandling.Ignore)]
        public eRoutingSignalType? SupportedSignalTypes => _input?.SupportedSignalTypes;

        /// <summary>
        /// Gets the Name of the input slot
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name => _input?.Name;

        /// <summary>
        /// Gets the IsOnline of the input slot
         ///
        /// </summary>
        [JsonProperty("isOnline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOnline => _input?.IsOnline.BoolValue;


        /// <summary>
        /// Gets the VideoSyncDetected of the input slot
        /// </summary>
        [JsonProperty("videoSyncDetected", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VideoSyncDetected => _input?.VideoSyncDetected;

        /// <summary>
        /// Gets the Key of the input slot
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key => _input?.Key;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingInput"/> class.
        /// </summary>
        /// <param name="input"></param>
        public RoutingInput(IRoutingInputSlot input)
        {
            _input = input;
        }
    }

    /// <summary>
    /// Represents a RoutingOutput
    /// </summary>
    public class RoutingOutput : IKeyName
    {
        private IRoutingOutputSlot _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingOutput"/> class.
        /// </summary>
        /// <param name="output"></param>
        public RoutingOutput(IRoutingOutputSlot output)
        {
            _output = output;
        }

        /// <summary>
        /// Gets the RxDeviceKey of the output slot
        /// </summary>
        [JsonProperty("rxDeviceKey")]
        public string RxDeviceKey => _output.RxDeviceKey;

        /// <summary>
        /// Gets the CurrentRoutes of the output slot
        /// </summary>
        [JsonProperty("currentRoutes")]
        public Dictionary<string, RoutingInput> CurrentRoutes => _output.CurrentRoutes.ToDictionary(kvp => kvp.Key.ToString(), kvp => new RoutingInput(kvp.Value));

        /// <summary>
        /// Gets the SlotNumber of the output slot
        /// </summary>
        [JsonProperty("slotNumber")]
        public int SlotNumber => _output.SlotNumber;

        /// <summary>
        /// Gets the SupportedSignalTypes of the output slot
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("supportedSignalTypes")]
        public eRoutingSignalType SupportedSignalTypes => _output.SupportedSignalTypes;


        /// <summary>
        /// Gets the Name of the output slot
        /// </summary>
        [JsonProperty("name")]
        public string Name => _output.Name;


        /// <summary>
        /// Gets the Key of the output slot
        /// </summary>
        [JsonProperty("key")]
        public string Key => _output.Key;
    }

    /// <summary>
    /// Represents a MatrixRouteRequest
    /// </summary>
    public class MatrixRouteRequest
    {
        /// <summary>
        /// Gets or sets the OutputKey
        /// </summary>
        [JsonProperty("outputKey")]
        public string OutputKey { get; set; }

        /// <summary>
        /// Gets or sets the InputKey
        /// </summary>
        [JsonProperty("inputKey")]
        public string InputKey { get; set; }

        /// <summary>
        /// Gets or sets the RouteType
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [JsonProperty("routeType")]
        public eRoutingSignalType RouteType { get; set; }
    }
}
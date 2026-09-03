using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IHasNamedRoutingSlots"/> — sends named
    /// input/output slots with per-signal-type current-route feedback, which the bare
    /// <see cref="IRoutingMidpointWithFeedbackMessenger"/> (registered for every
    /// <see cref="IRoutingMidpointWithFeedback"/> device, including this one) cannot provide.
    /// </summary>
    public class IHasNamedRoutingSlotsMessenger : MessengerBase
    {
        private readonly IHasNamedRoutingSlots _device;

        public IHasNamedRoutingSlotsMessenger(string key, string messagePath, IHasNamedRoutingSlots device)
            : base(key, messagePath, device as IKeyName)
        {
            _device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            foreach (var input in _device.InputSlots.Values)
            {
                if (input is not IRoutingInputSlotInfo status) continue;

                status.VideoSyncChanged += (sender, args) => SendFullStatus();

                if (status.IsOnline != null)
                    status.IsOnline.OutputChange += (sender, args) => SendFullStatus();
            }

            foreach (var output in _device.OutputSlots.Values)
            {
                output.OutputSlotChanged += (sender, args) => SendFullStatus();

                if (output is IRoutingOutputSlotStatus outputStatus && outputStatus.IsOnline != null)
                    outputStatus.IsOnline.OutputChange += (sender, args) => SendFullStatus();
            }
        }

        private static RoutingSlotMessage BuildInputMessage(IRoutingSlotInfo slot)
        {
            var message = new RoutingSlotMessage
            {
                Key = slot.Key,
                Name = slot.Name,
                SlotNumber = slot.SlotNumber,
                SupportedSignalTypes = slot.SupportedSignalTypes.ToString()
            };

            if (slot is IRoutingInputSlotInfo status)
            {
                message.TxDeviceKey = status.TxDeviceKey;
                message.IsOnline = status.IsOnline?.BoolValue;
                message.VideoSyncDetected = status.VideoSyncDetected;
            }

            return message;
        }

        private static RoutingSlotMessage BuildOutputMessage(
            IRoutingOutputSlotInfo slot,
            IReadOnlyDictionary<string, RoutingSlotMessage> inputs)
        {
            var message = new RoutingSlotMessage
            {
                Key = slot.Key,
                Name = slot.Name,
                SlotNumber = slot.SlotNumber,
                SupportedSignalTypes = slot.SupportedSignalTypes.ToString(),
                CurrentRouteInputKeys = slot.CurrentRouteInputKeys
                    .ToDictionary(r => r.Key.ToString(), r => r.Value),
                CurrentRoutes = slot.CurrentRouteInputKeys.ToDictionary(
                    r => r.Key.ToString(),
                    r => inputs.TryGetValue(r.Value, out var input)
                        ? input
                        : new RoutingSlotMessage { Key = r.Value })
            };

            if (slot is IRoutingOutputSlotStatus status)
            {
                message.RxDeviceKey = status.RxDeviceKey;
                message.IsOnline = status.IsOnline?.BoolValue;
            }

            return message;
        }

        private void SendFullStatus(string id = null)
        {
            var inputs = _device.InputSlots.ToDictionary(kvp => kvp.Key, kvp => BuildInputMessage(kvp.Value));

            var outputs = _device.OutputSlots.ToDictionary(kvp => kvp.Key, kvp => BuildOutputMessage(kvp.Value, inputs));

            var content = JToken.FromObject(new
            {
                inputs,
                outputs
            });

            PostStatusMessage(content, MessagePath, id);
        }
    }

    public class RoutingSlotMessage
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slotNumber")]
        public int SlotNumber { get; set; }

        [JsonProperty("supportedSignalTypes")]
        public string SupportedSignalTypes { get; set; }

        [JsonProperty("txDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string TxDeviceKey { get; set; }

        [JsonProperty("rxDeviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string RxDeviceKey { get; set; }

        [JsonProperty("isOnline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOnline { get; set; }

        [JsonProperty("videoSyncDetected", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VideoSyncDetected { get; set; }

        [JsonProperty("currentRouteInputKeys", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> CurrentRouteInputKeys { get; set; }

        [JsonProperty("currentRoutes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, RoutingSlotMessage> CurrentRoutes { get; set; }
    }
}

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

            foreach (var output in _device.OutputSlots.Values)
            {
                output.OutputSlotChanged += (sender, args) => SendFullStatus();
            }
        }

        private static RoutingSlotMessage BuildSlotMessage(IRoutingSlotInfo slot) => new RoutingSlotMessage
        {
            Key = slot.Key,
            Name = slot.Name,
            SlotNumber = slot.SlotNumber,
            SupportedSignalTypes = slot.SupportedSignalTypes.ToString()
        };

        private void SendFullStatus(string id = null)
        {
            var inputSlots = _device.InputSlots.ToDictionary(kvp => kvp.Key, kvp => BuildSlotMessage(kvp.Value));

            var outputSlots = _device.OutputSlots.ToDictionary(kvp => kvp.Key, kvp =>
            {
                var message = BuildSlotMessage(kvp.Value);
                message.CurrentRouteInputKeys = kvp.Value.CurrentRouteInputKeys
                    .ToDictionary(r => r.Key.ToString(), r => r.Value);
                return message;
            });

            var content = JToken.FromObject(new
            {
                inputSlots,
                outputSlots
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

        [JsonProperty("currentRouteInputKeys", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> CurrentRouteInputKeys { get; set; }
    }
}

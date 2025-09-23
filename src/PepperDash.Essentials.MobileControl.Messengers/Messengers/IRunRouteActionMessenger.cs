using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a RunRouteActionMessenger
    /// </summary>
    public class RunRouteActionMessenger : MessengerBase
    {
        /// <summary>
        /// Gets or sets the RoutingDevice
        /// </summary>
        public IRunRouteAction RoutingDevice { get; private set; }

        public RunRouteActionMessenger(string key, IRunRouteAction routingDevice, string messagePath)
            : base(key, messagePath, routingDevice as IKeyName)
        {
            RoutingDevice = routingDevice ?? throw new ArgumentNullException("routingDevice");


            if (RoutingDevice is IRoutingSink routingSink)
            {
                routingSink.CurrentSourceChange += RoutingSink_CurrentSourceChange;
            }
        }

        private void RoutingSink_CurrentSourceChange(SourceListItem info, ChangeType type)
        {
            SendRoutingFullMessageObject();
        }

        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendRoutingFullMessageObject(id));

            AddAction("/routingStatus", (id, content) => SendRoutingFullMessageObject(id));

            AddAction("/source", (id, content) =>
                {
                    var c = content.ToObject<SourceSelectMessageContent>();
                    // assume no sourceListKey
                    var sourceListKey = string.Empty;

                    if (!string.IsNullOrEmpty(c.SourceListKey))
                    {
                        // Check for source list in content of message
                        sourceListKey = c.SourceListKey;
                    }

                    RoutingDevice.RunRouteAction(c.SourceListItemKey, sourceListKey);
                });

            if (RoutingDevice is IRoutingSink sinkDevice)
            {
                sinkDevice.CurrentSourceChange += (o, a) => SendRoutingFullMessageObject();
            }
        }

        /// <summary>
        /// Helper method to update full status of the routing device
        /// </summary>
        private void SendRoutingFullMessageObject(string id = null)
        {
            if (RoutingDevice is IRoutingSink sinkDevice)
            {
                var sourceKey = sinkDevice.CurrentSourceInfoKey;

                if (string.IsNullOrEmpty(sourceKey))
                    sourceKey = "none";

                PostStatusMessage(new RoutingStateMessage
                {
                    SelectedSourceKey = sourceKey
                });
            }
        }
    }

    /// <summary>
    /// Represents a RoutingStateMessage
    /// </summary>
    public class RoutingStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the SelectedSourceKey
        /// </summary>
        [JsonProperty("selectedSourceKey")]
        public string SelectedSourceKey { get; set; }
    }
}
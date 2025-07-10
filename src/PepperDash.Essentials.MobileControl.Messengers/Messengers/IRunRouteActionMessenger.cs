using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for routing actions
    /// </summary>
    public class RunRouteActionMessenger : MessengerBase
    {
        /// <summary>
        /// Device being bridged
        /// </summary>
        public IRunRouteAction RoutingDevice { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunRouteActionMessenger"/> class.
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="routingDevice">Device that implements IRunRouteAction</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <exception cref="ArgumentNullException">Thrown when routingDevice is null</exception>
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

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendRoutingFullMessageObject(id));

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
                }, id);
            }
        }
    }

    /// <summary>
    /// Message class for routing state
    /// </summary>
    public class RoutingStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the selected source key
        /// </summary>
        [JsonProperty("selectedSourceKey")]
        public string SelectedSourceKey { get; set; }
    }
}
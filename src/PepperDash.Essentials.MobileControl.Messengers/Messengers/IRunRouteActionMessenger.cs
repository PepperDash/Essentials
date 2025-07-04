using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using System;


namespace PepperDash.Essentials.AppServer.Messengers;

public class RunRouteActionMessenger : MessengerBase
{
    /// <summary>
    /// Device being bridged
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
        AddAction("/fullStatus", (id, content) => SendRoutingFullMessageObject());

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
    private void SendRoutingFullMessageObject()
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

public class RoutingStateMessage : DeviceStateMessageBase
{
    [JsonProperty("selectedSourceKey")]
    public string SelectedSourceKey { get; set; }
}
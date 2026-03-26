using System;
using PepperDash.Core;
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

        /// <summary>
        /// Initializes a new instance of the RunRouteActionMessenger class
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="routingDevice">The routing device.</param>
        /// <param name="messagePath">The message path.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RunRouteActionMessenger(string key, IRunRouteAction routingDevice, string messagePath)
            : base(key, messagePath, routingDevice as IKeyName)
        {
            RoutingDevice = routingDevice ?? throw new ArgumentNullException("routingDevice");
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
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

        }
    }

}
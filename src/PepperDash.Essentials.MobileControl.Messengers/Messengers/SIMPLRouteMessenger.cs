using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;


namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for routing operations in SIMPL-based systems.
    /// Handles source selection and routing status feedback.
    /// </summary>
    public class SIMPLRouteMessenger : MessengerBase
    {
        private readonly BasicTriList _eisc;

        private readonly uint _joinStart;

        /// <summary>
        /// Defines the string join mappings for SIMPL routing operations.
        /// </summary>
        public class StringJoin
        {
            /// <summary>
            /// Join number for current source information (1).
            /// </summary>
            public const uint CurrentSource = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SIMPLRouteMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="eisc">The basic tri-list for SIMPL communication.</param>
        /// <param name="messagePath">The message path for routing messages.</param>
        /// <param name="joinStart">The starting join number for SIMPL signal mapping.</param>
        public SIMPLRouteMessenger(string key, BasicTriList eisc, string messagePath, uint joinStart)
            : base(key, messagePath)
        {
            _eisc = eisc;
            _joinStart = joinStart - 1;

            _eisc.SetStringSigAction(_joinStart + StringJoin.CurrentSource, SendRoutingFullMessageObject);
        }

        /// <summary>
        /// Registers actions for handling routing operations and status reporting.
        /// Includes full status requests and source selection actions.
        /// </summary>
        protected override void RegisterActions()
        {
            AddAction("/fullStatus",
                (id, content) => SendRoutingFullMessageObject(_eisc.GetString(_joinStart + StringJoin.CurrentSource)));

            AddAction("/source", (id, content) =>
            {
                var c = content.ToObject<SourceSelectMessageContent>();

                _eisc.SetString(_joinStart + StringJoin.CurrentSource, c.SourceListItemKey);
            });
        }

        /// <summary>
        /// Unregisters this messenger from the mobile control app server.
        /// Removes all registered actions and clears SIMPL signal actions.
        /// </summary>
        /// <param name="appServerController">The mobile control app server controller.</param>
        public void CustomUnregisterWithAppServer(IMobileControl appServerController)
        {
            appServerController.RemoveAction(MessagePath + "/fullStatus");
            appServerController.RemoveAction(MessagePath + "/source");

            _eisc.SetStringSigAction(_joinStart + StringJoin.CurrentSource, null);
        }

        /// <summary>
        /// Helper method to update full status of the routing device
        /// </summary>
        private void SendRoutingFullMessageObject(string sourceKey)
        {
            if (string.IsNullOrEmpty(sourceKey))
                sourceKey = "none";

            PostStatusMessage(JToken.FromObject(new
            {
                selectedSourceKey = sourceKey
            })
            );
        }
    }
}
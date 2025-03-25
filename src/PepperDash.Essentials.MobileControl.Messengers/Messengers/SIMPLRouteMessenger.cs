using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;


namespace PepperDash.Essentials.AppServer.Messengers
{

    public class SIMPLRouteMessenger : MessengerBase
    {
        private readonly BasicTriList _eisc;

        private readonly uint _joinStart;

        public class StringJoin
        {
            /// <summary>
            /// 1
            /// </summary>
            public const uint CurrentSource = 1;
        }

        public SIMPLRouteMessenger(string key, BasicTriList eisc, string messagePath, uint joinStart)
            : base(key, messagePath)
        {
            _eisc = eisc;
            _joinStart = joinStart - 1;

            _eisc.SetStringSigAction(_joinStart + StringJoin.CurrentSource, SendRoutingFullMessageObject);
        }

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            AddAction("/fullStatus",
                (id, content) => SendRoutingFullMessageObject(_eisc.GetString(_joinStart + StringJoin.CurrentSource)));

            AddAction("/source", (id, content) =>
            {
                var c = content.ToObject<SourceSelectMessageContent>();

                _eisc.SetString(_joinStart + StringJoin.CurrentSource, c.SourceListItemKey);
            });
        }

#if SERIES4
        public void CustomUnregsiterWithAppServer(IMobileControl appServerController)
#else
        public void CustomUnregsiterWithAppServer(MobileControlSystemController appServerController)
#endif
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
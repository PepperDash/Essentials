using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer.Messengers
{
    public class SIMPLRouteMessenger : MessengerBase
    {
        BasicTriList EISC;

        uint JoinStart;

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
            EISC = eisc;
            JoinStart = joinStart - 1;

            EISC.SetStringSigAction(JoinStart + StringJoin.CurrentSource, (s) => SendRoutingFullMessageObject(s));
        }

        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            appServerController.AddAction(MessagePath + "/fullStatus", new Action(() =>
                {
                    SendRoutingFullMessageObject(EISC.GetString(JoinStart + StringJoin.CurrentSource));
                }));

            appServerController.AddAction(MessagePath +"/source", new Action<SourceSelectMessageContent>(c =>
                {
                    EISC.SetString(JoinStart + StringJoin.CurrentSource, c.SourceListItem);
                }));

        }

        public void CustomUnregsiterWithAppServer(MobileControlSystemController appServerController)
        {
            appServerController.RemoveAction(MessagePath + "/fullStatus");
            appServerController.RemoveAction(MessagePath + "/source");

            EISC.SetStringSigAction(JoinStart + StringJoin.CurrentSource, null);
        }

        /// <summary>
        /// Helper method to update full status of the routing device
        /// </summary>
        void SendRoutingFullMessageObject(string sourceKey)
        {
            if (string.IsNullOrEmpty(sourceKey))
                sourceKey = "none";

            PostStatusMessage(new
            {
                selectedSourceKey = sourceKey
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IRunRouteActionMessenger : MessengerBase
    {
        /// <summary>
        /// Device being bridged
        /// </summary>
        public IRunRouteAction RoutingDevice {get; private set;}

        public IRunRouteActionMessenger(string key, IRunRouteAction routingDevice, string messagePath)
            : base(key, messagePath)
        {
            if (routingDevice == null)
                throw new ArgumentNullException("routingDevice");

            RoutingDevice = routingDevice;

            var routingSink = RoutingDevice as IRoutingSinkNoSwitching;

            if (routingSink != null)
            {
                routingSink.CurrentSourceChange += new SourceInfoChangeHandler(routingSink_CurrentSourceChange);
            }
        }

        void routingSink_CurrentSourceChange(SourceListItem info, ChangeType type)
        {
            SendRoutingFullMessageObject();
        }

        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            appServerController.AddAction(MessagePath + "/fullStatus", new Action(SendRoutingFullMessageObject));

            appServerController.AddAction(MessagePath + "/source", new Action<SourceSelectMessageContent>(c =>
                    {
                        RoutingDevice.RunRouteAction(c.SourceListItem, c.SourceListKey);
                    }));

            var sinkDevice = RoutingDevice as IRoutingSinkNoSwitching;
            if(sinkDevice != null)
            {
                sinkDevice.CurrentSourceChange += new SourceInfoChangeHandler((o, a) =>
                {
                    SendRoutingFullMessageObject();
                });
            }
        }

        /// <summary>
        /// Helper method to update full status of the routing device
        /// </summary>
        void SendRoutingFullMessageObject()
        {
            var sinkDevice = RoutingDevice as IRoutingSinkNoSwitching;

            if(sinkDevice != null) 
            {
                var sourceKey = sinkDevice.CurrentSourceInfoKey;

                if (string.IsNullOrEmpty(sourceKey))
                    sourceKey = "none";

                PostStatusMessage(new
                {
                    selectedSourceKey = sourceKey
                });
            }
        }
    }
}
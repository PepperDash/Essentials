using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class GenericMessenger : MessengerBase
    {
        public GenericMessenger(string key, EssentialsDevice device, string messagePath) : base(key, messagePath, device)
        {
        }

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());
        }

        private void SendFullStatus()
        {
            var state = new DeviceStateMessageBase();

            PostStatusMessage(state);
        }
    }
}

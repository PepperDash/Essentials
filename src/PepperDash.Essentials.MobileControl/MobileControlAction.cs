using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;

namespace PepperDash.Essentials
{
    public class MobileControlAction : IMobileControlAction
    {
        public IMobileControlMessenger Messenger { get; private set; }

        public Action<string, string, JToken> Action { get; private set; }

        public MobileControlAction(IMobileControlMessenger messenger, Action<string, string, JToken> handler)
        {
            Messenger = messenger;
            Action = handler;
        }
    }
}

using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a MobileControlAction
    /// </summary>
    public class MobileControlAction : IMobileControlAction
    {
        /// <summary>
        /// Gets or sets the Messenger
        /// </summary>
        public IMobileControlMessenger Messenger { get; private set; }

        public Action<string, string, JToken> Action { get; private set; }

        public MobileControlAction(IMobileControlMessenger messenger, Action<string, string, JToken> handler)
        {
            Messenger = messenger;
            Action = handler;
        }
    }
}

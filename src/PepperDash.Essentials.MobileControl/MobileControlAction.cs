using System;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a MobileControlAction
    /// </summary>
    public class MobileControlAction : IMobileControlAction
    {
        /// <summary>
        /// Gets the Messenger
        /// </summary>
        public IMobileControlMessenger Messenger { get; private set; }

        /// <summary>
        /// Action to execute when this path is matched
        /// </summary>
        public Action<string, string, JToken> Action { get; private set; }

        /// <summary>
        /// Initialize an instance of the <see cref="MobileControlAction"/> class
        /// </summary>
        /// <param name="messenger">Messenger associated with this action</param>
        /// <param name="handler">Action to take when this path is matched</param>
        public MobileControlAction(IMobileControlMessenger messenger, Action<string, string, JToken> handler)
        {
            Messenger = messenger;
            Action = handler;
        }
    }
}

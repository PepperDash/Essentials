using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Generic messenger for basic device communication
    /// </summary>
    public class GenericMessenger : MessengerBase
    {
        /// <summary>
        /// Initializes a new instance of the GenericMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="device">Device to communicate with</param>
        /// <param name="messagePath">Path for message routing</param>
        public GenericMessenger(string key, EssentialsDevice device, string messagePath) : base(key, messagePath, device)
        {
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
        }

        private void SendFullStatus(string id = null)
        {
            var state = new DeviceStateMessageBase();

            PostStatusMessage(state, id);
        }
    }
}

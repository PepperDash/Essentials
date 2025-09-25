using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a GenericMessenger
    /// </summary>
    public class GenericMessenger : MessengerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMessenger"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="device">The device.</param>
        /// <param name="messagePath">The message path.</param>
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

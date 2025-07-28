using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a GenericMessenger
    /// </summary>
    public class GenericMessenger : MessengerBase
    {
        public GenericMessenger(string key, EssentialsDevice device, string messagePath) : base(key, messagePath, device)
        {
        }

        protected override void RegisterActions()
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

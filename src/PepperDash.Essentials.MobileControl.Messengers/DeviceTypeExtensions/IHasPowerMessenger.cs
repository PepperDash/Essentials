using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Messenger that provides mobile control interface for devices with power control functionality
    /// </summary>
    public class IHasPowerMessenger : MessengerBase
    {
        /// <summary>
        /// The power control device this messenger is associated with
        /// </summary>
        private readonly IHasPowerControl powerDevice;

        /// <summary>
        /// Initializes a new instance of the IHasPowerMessenger class
        /// </summary>
        /// <param name="key">The unique key for this messenger</param>
        /// <param name="messagePath">The message path for routing power control messages</param>
        /// <param name="device">The device that implements power control functionality</param>
        public IHasPowerMessenger(string key, string messagePath, IHasPowerControl device) : base(key, messagePath, device as IKeyName)
        {
            powerDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/powerOn", (id, content) => powerDevice?.PowerOn());
            AddAction("/powerOff", (id, content) => powerDevice?.PowerOff());
            AddAction("/powerToggle", (id, content) => powerDevice?.PowerToggle());
        }
    }
}
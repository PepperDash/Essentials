using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a IHasPowerMessenger
    /// </summary>
    public class IHasPowerMessenger : MessengerBase
    {
        private readonly IHasPowerControl powerDevice;

        /// <summary>
        /// Create an instance of the <see cref="IHasPowerMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
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
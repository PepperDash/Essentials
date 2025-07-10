using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Messenger for devices that implement IDvr interface
    /// </summary>
    public class IDvrMessenger : MessengerBase
    {
        private readonly IDvr dvrDevice;

        /// <summary>
        /// Initializes a new instance of the IDvrMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements IDvr</param>
        public IDvrMessenger(string key, string messagePath, IDvr device) : base(key, messagePath, device as IKeyName)
        {
            dvrDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/dvrlist", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dvrDevice?.DvrList(b)));
            AddAction("/record", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dvrDevice?.Record(b)));
        }

    }
}
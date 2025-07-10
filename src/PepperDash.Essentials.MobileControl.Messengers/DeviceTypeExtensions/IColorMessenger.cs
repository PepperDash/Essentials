using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Messenger for devices that implement IColor interface
    /// </summary>
    public class IColorMessenger : MessengerBase
    {
        private readonly IColor colorDevice;

        /// <summary>
        /// Initializes a new instance of the IColorMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements IColor</param>
        public IColorMessenger(string key, string messagePath, IColor device) : base(key, messagePath, device as IKeyName)
        {
            colorDevice = device as IColor;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/red", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => colorDevice?.Red(b)));
            AddAction("/green", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => colorDevice?.Green(b)));
            AddAction("/yellow", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => colorDevice?.Yellow(b)));
            AddAction("/blue", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => colorDevice?.Blue(b)));
        }
    }
}
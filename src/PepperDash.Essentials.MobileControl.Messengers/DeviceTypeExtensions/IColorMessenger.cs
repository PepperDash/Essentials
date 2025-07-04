using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a IColorMessenger
    /// </summary>
    public class IColorMessenger : MessengerBase
    {
        private readonly IColor colorDevice;

        /// <summary>
        /// Create an instance of the <see cref="IColorMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
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
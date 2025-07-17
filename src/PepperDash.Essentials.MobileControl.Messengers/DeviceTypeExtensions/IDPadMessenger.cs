using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Messenger that provides mobile control interface for devices with directional pad functionality
    /// </summary>
    public class IDPadMessenger : MessengerBase
    {
        /// <summary>
        /// The directional pad device this messenger is associated with
        /// </summary>
        private readonly IDPad dpadDevice;

        /// <summary>
        /// Initializes a new instance of the IDPadMessenger class
        /// </summary>
        /// <param name="key">The unique key for this messenger</param>
        /// <param name="messagePath">The message path for routing directional pad messages</param>
        /// <param name="device">The device that implements directional pad functionality</param>
        public IDPadMessenger(string key, string messagePath, IDPad device) : base(key, messagePath, device as IKeyName)
        {
            dpadDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/up", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dpadDevice?.Up(b)));
            AddAction("/down", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dpadDevice?.Down(b)));
            AddAction("/left", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dpadDevice?.Left(b)));
            AddAction("/right", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dpadDevice?.Right(b)));
            AddAction("/select", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dpadDevice?.Select(b)));
            AddAction("/menu", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dpadDevice?.Menu(b)));
            AddAction("/exit", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dpadDevice?.Exit(b)));
        }
    }
}
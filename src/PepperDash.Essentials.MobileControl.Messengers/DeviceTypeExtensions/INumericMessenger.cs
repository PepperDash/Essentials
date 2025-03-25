using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
#if SERIES4
#endif
namespace PepperDash.Essentials.Room.MobileControl
{
    public class INumericKeypadMessenger:MessengerBase
    {
        private readonly INumericKeypad keypadDevice;
        public INumericKeypadMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            keypadDevice = device as INumericKeypad;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();            

            AddAction("/num0", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit0(b)));
            AddAction("/num1", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit1(b)));
            AddAction("/num2", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit2(b)));
            AddAction("/num3", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit3(b)));
            AddAction("/num4", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit4(b)));
            AddAction("/num5", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit5(b)));
            AddAction("/num6", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit6(b)));
            AddAction("/num7", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit7(b)));
            AddAction("/num8", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit8(b)));
            AddAction("/num9", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.Digit9(b)));
            AddAction("/numDash", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.KeypadAccessoryButton1(b)));
            AddAction("/numEnter", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => keypadDevice?.KeypadAccessoryButton2(b)));
            // Deal with the Accessory functions on the numpad later
        }
    }
}
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
#if SERIES4
#endif
namespace PepperDash.Essentials.Room.MobileControl
{
    public class IDPadMessenger:MessengerBase
    {
        private readonly IDPad dpadDevice;
        public IDPadMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            dpadDevice = device as IDPad;
        }


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
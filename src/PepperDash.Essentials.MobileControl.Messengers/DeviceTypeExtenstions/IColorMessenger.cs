using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Room.MobileControl
{
    public class IColorMessenger:MessengerBase
    {
        private readonly IColor colorDevice;
        public IColorMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            colorDevice = device as IColor;
        }

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
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Room.MobileControl
{
    public class IHasPowerMessenger:MessengerBase
    {
        private readonly IHasPowerControl powerDevice;
        public IHasPowerMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            powerDevice = device as IHasPowerControl;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/powerOn", (id, content) => powerDevice?.PowerOn());
            AddAction("/powerOff", (id, content) => powerDevice?.PowerOff());
            AddAction("/powerToggle", (id, content) => powerDevice?.PowerToggle());
        }
    }
}
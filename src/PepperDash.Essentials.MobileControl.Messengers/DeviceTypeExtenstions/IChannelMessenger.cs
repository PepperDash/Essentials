using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
#if SERIES4
#endif
namespace PepperDash.Essentials.Room.MobileControl
{
    public class IChannelMessenger:MessengerBase
    {
        private readonly IChannel channelDevice;

        public IChannelMessenger(string key, string messagePath, Device device) : base(key, messagePath, device)
        {
            channelDevice = device as IChannel;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();      

            AddAction("/chanUp", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.ChannelUp(b)));

            AddAction("/chanDown", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.ChannelDown(b)));
            AddAction("/lastChan", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.LastChannel(b)));
            AddAction("/guide", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.Guide(b)));
            AddAction("/info", (id, content) => PressAndHoldHandler.HandlePressAndHold  (DeviceKey, content, (b) => channelDevice?.Info(b)));
            AddAction("/exit", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.Exit(b)));
        }
    }
}
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a IChannelMessenger
    /// </summary>
    public class IChannelMessenger : MessengerBase
    {
        private readonly IChannel channelDevice;

        public IChannelMessenger(string key, string messagePath, IChannel device) : base(key, messagePath, device as IKeyName)
        {
            channelDevice = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/chanUp", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.ChannelUp(b)));

            AddAction("/chanDown", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.ChannelDown(b)));
            AddAction("/lastChan", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.LastChannel(b)));
            AddAction("/guide", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.Guide(b)));
            AddAction("/info", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.Info(b)));
            AddAction("/exit", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => channelDevice?.Exit(b)));
        }
    }
}
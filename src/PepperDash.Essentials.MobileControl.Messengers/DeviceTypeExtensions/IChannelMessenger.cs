using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Messenger that provides mobile control interface for devices with channel control functionality
    /// </summary>
    public class IChannelMessenger : MessengerBase
    {
        /// <summary>
        /// The channel control device this messenger is associated with
        /// </summary>
        private readonly IChannel channelDevice;

        /// <summary>
        /// Initializes a new instance of the IChannelMessenger class
        /// </summary>
        /// <param name="key">The unique key for this messenger</param>
        /// <param name="messagePath">The message path for routing channel control messages</param>
        /// <param name="device">The device that implements channel control functionality</param>
        public IChannelMessenger(string key, string messagePath, IChannel device) : base(key, messagePath, device as IKeyName)
        {
            channelDevice = device;
        }

        /// <inheritdoc />
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
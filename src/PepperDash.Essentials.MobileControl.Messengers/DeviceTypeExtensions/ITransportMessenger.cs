using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Messenger that provides mobile control interface for devices with transport control functionality
    /// </summary>
    public class ITransportMessenger : MessengerBase
    {
        /// <summary>
        /// The transport control device this messenger is associated with
        /// </summary>
        private readonly ITransport transportDevice;

        /// <summary>
        /// Initializes a new instance of the ITransportMessenger class
        /// </summary>
        /// <param name="key">The unique key for this messenger</param>
        /// <param name="messagePath">The message path for routing transport control messages</param>
        /// <param name="device">The device that implements transport control functionality</param>
        public ITransportMessenger(string key, string messagePath, ITransport device) : base(key, messagePath, device as IKeyName)
        {
            transportDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/play", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Play(b)));
            AddAction("/pause", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Pause(b)));
            AddAction("/stop", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Stop(b)));
            AddAction("/prevTrack", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.ChapPlus(b)));
            AddAction("/nextTrack", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.ChapMinus(b)));
            AddAction("/rewind", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Rewind(b)));
            AddAction("/ffwd", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.FFwd(b)));
            AddAction("/record", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Record(b)));
        }

    }
}
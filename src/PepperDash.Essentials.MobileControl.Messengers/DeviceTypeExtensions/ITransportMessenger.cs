
﻿using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a ITransportMessenger
    /// </summary>
    public class ITransportMessenger : MessengerBase
    {
        private readonly ITransport transportDevice;

        /// <summary>
        /// Create an instance of the <see cref="ITransportMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
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


﻿using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a IDvrMessenger
    /// </summary>
    public class IDvrMessenger : MessengerBase
    {
        private readonly IDvr dvrDevice;

        /// <summary>
        /// Create an instance of the <see cref="IDvrMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        public IDvrMessenger(string key, string messagePath, IDvr device) : base(key, messagePath, device as IKeyName)
        {
            dvrDevice = device;
        }


        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/dvrlist", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dvrDevice?.DvrList(b)));
            AddAction("/record", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dvrDevice?.Record(b)));
        }

    }
}

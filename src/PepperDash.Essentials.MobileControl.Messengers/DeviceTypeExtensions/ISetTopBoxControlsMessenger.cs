
﻿using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a ISetTopBoxControlsMessenger
    /// </summary>
    public class ISetTopBoxControlsMessenger : MessengerBase
    {
        private readonly ISetTopBoxControls stbDevice;

        /// <summary>
        /// Create an instance of the <see cref="ISetTopBoxControlsMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        public ISetTopBoxControlsMessenger(string key, string messagePath, ISetTopBoxControls device) : base(key, messagePath, device as IKeyName)
        {
            stbDevice = device;
        }


        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();
            AddAction("/fullStatus", (id, content) => SendISetTopBoxControlsFullMessageObject());
            AddAction("/dvrList", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => stbDevice?.DvrList(b)));
            AddAction("/replay", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => stbDevice?.Replay(b)));
        }
        /// <summary>
        /// Helper method to build call status for vtc
        /// </summary>
        /// <returns></returns>
        private void SendISetTopBoxControlsFullMessageObject()
        {

            PostStatusMessage(new SetTopBoxControlsState());


        }
    }

    /// <summary>
    /// Represents the state of a ISetTopBoxControls device to be sent to mobile clients
    /// </summary>
    public class SetTopBoxControlsState : DeviceStateMessageBase
    {

    }
}

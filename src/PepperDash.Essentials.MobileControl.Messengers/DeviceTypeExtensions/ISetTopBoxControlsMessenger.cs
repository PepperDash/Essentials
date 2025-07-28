using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a ISetTopBoxControlsMessenger
    /// </summary>
    public class ISetTopBoxControlsMessenger : MessengerBase
    {
        private readonly ISetTopBoxControls stbDevice;
        public ISetTopBoxControlsMessenger(string key, string messagePath, ISetTopBoxControls device) : base(key, messagePath, device as IKeyName)
        {
            stbDevice = device;
        }

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

    public class SetTopBoxControlsState : DeviceStateMessageBase
    {

    }
}
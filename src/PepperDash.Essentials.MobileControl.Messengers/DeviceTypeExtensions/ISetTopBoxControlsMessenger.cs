using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Messenger for devices that implement ISetTopBoxControls interface
    /// </summary>
    public class ISetTopBoxControlsMessenger : MessengerBase
    {
        private readonly ISetTopBoxControls stbDevice;

        /// <summary>
        /// Initializes a new instance of the ISetTopBoxControlsMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements ISetTopBoxControls</param>
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
    /// State message for set top box controls
    /// </summary>
    public class SetTopBoxControlsState : DeviceStateMessageBase
    {

    }
}
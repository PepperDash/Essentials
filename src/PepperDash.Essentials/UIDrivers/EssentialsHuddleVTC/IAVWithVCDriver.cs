using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
    /// For hanging off various common VC things that child drivers might need from a parent AV driver
    /// </summary>
    public interface IAVWithVCDriver : IAVDriver
    {
        IEssentialsHuddleVtc1Room CurrentRoom { get; }

        PepperDash.Essentials.Core.Touchpanels.Keyboards.HabaneroKeyboardController Keyboard { get; }
        /// <summary>
        /// Exposes the ability to switch into call mode
        /// </summary>
        void ActivityCallButtonPressed();
        /// <summary>
        /// Allows the codec to trigger the main UI to clear up if call is coming in.
        /// </summary>
        void PrepareForCodecIncomingCall();

        uint CallListOrMeetingInfoPopoverVisibilityJoin { get; }

        SubpageReferenceList MeetingOrContactMethodModalSrl { get; }
    }
}
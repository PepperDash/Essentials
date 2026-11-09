using System.Linq;
using Crestron.SimplSharpPro.UC;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Mobile Control controller for a Crestron UC-ENGINE-FLEX-TEAMS (Microsoft Teams Rooms). The engine shows the
    /// Mobile Control app in its windowed custom UI frame, so it is the panel, and it reports the Teams meeting and
    /// conference audio state through the standard interfaces.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="IHasInCallFeedback"/>: Teams Rooms is in a meeting. The MessengerFactoryRegistry has no
    /// messenger for this interface, so the state does not reach Mobile Control clients by itself; a room plugin reads
    /// <see cref="InCallFeedback"/> straight from the device.</item>
    /// <item><see cref="IPrivacy"/>: the conference microphone mute (its messenger is added automatically).</item>
    /// <item><see cref="IBasicVolumeWithFeedback"/>: the conference speaker volume and mute (its messenger is added
    /// automatically). The level is passed through as the engine reports it; its range is not documented. If Mobile
    /// Control faders assume 0 to 65535 and the engine reports something else, scale it here or implement
    /// <c>IBasicVolumeWithFeedbackAdvanced</c> with a <c>MaxVolumeLevel</c> once the range is known.</item>
    /// </list>
    /// The engine's own app shows the Mobile Control app in a project frame over Teams. Showing and hiding that
    /// frame is the engine's meeting-room extender (<c>ShowProject</c> and <c>HideProject</c>), which is how the app
    /// switches back to Teams: <see cref="OpenApp"/> (switch to Teams) hides the project and <see cref="HideOpenApp"/>
    /// and <see cref="CloseOpenApp"/> show it again. The app-control messenger is registered so Mobile Control clients
    /// can send them.
    /// </remarks>
    public class MobileControlUcEngineController : MobileControlTouchpanelController, IHasInCallFeedback, IPrivacy, IBasicVolumeWithFeedback
    {
        private readonly UcEngineFlexTeams engine;

        private UcEngineFlexTeamsSystem4ReservedSigs System4 => engine.ExtenderUcEngineTeamsSystem4ReservedSigs;

        private UcEngineAudioRsvdSigs Audio => engine.ExtenderUcEngineTeamsAudioReservedSigs;

        // The project frame show/hide signals live on the meeting-room extender.
        private CcsUcCodec300LyncReservedSigs Project => engine.ExtenderMeetingRoomReservedSigs;

        /// <summary>
        /// True while Teams Rooms is in a meeting
        /// </summary>
        public BoolFeedback InCallFeedback { get; private set; }

        /// <summary>
        /// True while the conference microphone is muted
        /// </summary>
        public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }

        /// <summary>
        /// True while the conference speaker is muted
        /// </summary>
        public BoolFeedback MuteFeedback { get; private set; }

        /// <summary>
        /// Conference speaker volume, as the engine reports it
        /// </summary>
        public IntFeedback VolumeLevelFeedback { get; private set; }

        private FeedbackCollection<Feedback> TeamsFeedbacks { get; set; }

        /// <summary>
        /// Initializes a new instance of the MobileControlUcEngineController class.
        /// </summary>
        /// <param name="key">The unique key identifier for this controller.</param>
        /// <param name="name">The friendly name for this controller.</param>
        /// <param name="engine">The UC Engine, registered by IP-ID like a panel.</param>
        /// <param name="config">The configuration properties for this controller.</param>
        public MobileControlUcEngineController(string key, string name, UcEngineFlexTeams engine, MobileControlTouchpanelProperties config)
            : base(key, name, engine, config)
        {
            this.engine = engine;

            InCallFeedback = new BoolFeedback($"{Key}-inCall", () => System4.MtrApplicationInMeetingFeedback.BoolValue);
            PrivacyModeIsOnFeedback = new BoolFeedback($"{Key}-privacyModeIsOn", () => Audio.ConferenceMicMuteOnFeedback.BoolValue);
            MuteFeedback = new BoolFeedback($"{Key}-speakerMute", () => Audio.ConferenceSpeakerMuteOnFeedback.BoolValue);
            VolumeLevelFeedback = new IntFeedback($"{Key}-speakerVolume", () => Audio.ConferenceSpeakerVolumeFeedback.UShortValue);

            TeamsFeedbacks = new FeedbackCollection<Feedback>
            {
                InCallFeedback, PrivacyModeIsOnFeedback, MuteFeedback, VolumeLevelFeedback
            };

            // Also list them in the base Feedbacks, so anything that enumerates IHasFeedback.Feedbacks (the Dev Tools
            // Devices page) shows them.
            Feedbacks.AddRange(TeamsFeedbacks);

            // A feedback only raises OutputChange when its value changed, so any extender change refreshes them all.
            System4.DeviceExtenderSigChange += (e, a) =>
            {
                this.LogVerbose("Teams System4 extender args: {event}:{sig}:{type}:{boolValue}:{ushortValue}:{stringValue}", a.Event, a.Sig, a.Sig.Type, a.Sig.BoolValue, a.Sig.UShortValue, a.Sig.StringValue);
                UpdateTeamsFeedbacks();
            };

            Audio.DeviceExtenderSigChange += (e, a) =>
            {
                this.LogVerbose("Teams Audio extender args: {event}:{sig}:{type}:{boolValue}:{ushortValue}:{stringValue}", a.Event, a.Sig, a.Sig.Type, a.Sig.BoolValue, a.Sig.UShortValue, a.Sig.StringValue);
                UpdateTeamsFeedbacks();
            };

            engine.OnlineStatusChange += (d, a) => UpdateTeamsFeedbacks();

            // Extenders must be marked used before the engine registers (TouchpanelBase registers in pre-activation).
            System4.Use();
            Audio.Use();
            Project.Use();
        }

        private void UpdateTeamsFeedbacks()
        {
            foreach (var feedback in TeamsFeedbacks)
            {
                feedback.FireUpdate();
            }
        }

        /// <summary>
        /// Registers the app-control messenger (the base class only does that for TSW panels), then the standard ones.
        /// </summary>
        protected override bool CustomActivate()
        {
            var mc = DeviceManager.AllDevices.OfType<IMobileControl>().FirstOrDefault();

            mc?.AddDeviceMessenger(new ITswAppControlMessenger($"appControlMessenger-{Key}", $"/device/{Key}", this));

            return base.CustomActivate();
        }

        /// <summary>
        /// Switch to Teams: hides the Mobile Control project frame so the Teams Rooms app is showing.
        /// </summary>
        public override void OpenApp()
        {
            this.LogInformation("Switch to Teams requested; hiding the project frame");
            Project.HideProject();
        }

        /// <summary>
        /// Back to the Mobile Control app: shows the project frame again.
        /// </summary>
        public override void HideOpenApp()
        {
            this.LogInformation("Back to the Mobile Control app requested; showing the project frame");
            Project.ShowProject();
        }

        /// <summary>
        /// Back to the Mobile Control app: shows the project frame again.
        /// </summary>
        public override void CloseOpenApp()
        {
            this.LogInformation("Back to the Mobile Control app requested; showing the project frame");
            Project.ShowProject();
        }

        /// <summary>
        /// Mutes the conference microphone
        /// </summary>
        public void PrivacyModeOn() => Audio.ConferenceMicMuteOn();

        /// <summary>
        /// Unmutes the conference microphone
        /// </summary>
        public void PrivacyModeOff() => Audio.ConferenceMicMuteOff();

        /// <summary>
        /// Toggles the conference microphone mute
        /// </summary>
        public void PrivacyModeToggle()
        {
            if (PrivacyModeIsOnFeedback.BoolValue) PrivacyModeOff();
            else PrivacyModeOn();
        }

        /// <summary>
        /// Ramps the conference speaker volume up while held
        /// </summary>
        public void VolumeUp(bool pressRelease) => Audio.ConferenceSpeakerVolumeUp.BoolValue = pressRelease;

        /// <summary>
        /// Ramps the conference speaker volume down while held
        /// </summary>
        public void VolumeDown(bool pressRelease) => Audio.ConferenceSpeakerVolumeDown.BoolValue = pressRelease;

        /// <summary>
        /// Sets the conference speaker volume
        /// </summary>
        public void SetVolume(ushort level) => Audio.ConferenceSpeakerVolume.UShortValue = level;

        /// <summary>
        /// Mutes the conference speaker
        /// </summary>
        public void MuteOn() => Audio.ConferenceSpeakerMuteOn();

        /// <summary>
        /// Unmutes the conference speaker
        /// </summary>
        public void MuteOff() => Audio.ConferenceSpeakerMuteOff();

        /// <summary>
        /// Toggles the conference speaker mute
        /// </summary>
        public void MuteToggle()
        {
            if (MuteFeedback.BoolValue) MuteOff();
            else MuteOn();
        }
    }
}

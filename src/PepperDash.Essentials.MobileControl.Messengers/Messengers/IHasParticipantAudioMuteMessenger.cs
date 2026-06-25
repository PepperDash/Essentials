using System;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Mobile Control messenger for <see cref="IHasParticipantAudioMute"/>:
    /// mute-all and per-participant audio/video mute toggles. Action-only (no status of its own).
    /// </summary>
    public class IHasParticipantAudioMuteMessenger : MessengerBase
    {
        private readonly IHasParticipantAudioMute _codec;

        public IHasParticipantAudioMuteMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _codec = device as IHasParticipantAudioMute ?? throw new ArgumentNullException(nameof(device));
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/muteAllParticipants", (id, content) => _codec.MuteAudioForAllParticipants());
            AddAction("/toggleParticipantAudioMute", (id, content) =>
            {
                var i = content?.ToObject<MobileControlSimpleContent<int>>();
                if (i != null) _codec.ToggleAudioForParticipant(i.Value);
            });
            AddAction("/toggleParticipantVideoMute", (id, content) =>
            {
                var i = content?.ToObject<MobileControlSimpleContent<int>>();
                if (i != null) _codec.ToggleVideoForParticipant(i.Value);
            });
        }
    }
}

using System;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Mobile Control messenger for <see cref="IHasParticipantVideoMute"/>:
    /// per-participant video mute/unmute/toggle. Action-only (no status of its own).
    /// </summary>
    public class IHasParticipantVideoMuteMessenger : MessengerBase
    {
        private readonly IHasParticipantVideoMute _codec;

        public IHasParticipantVideoMuteMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _codec = device as IHasParticipantVideoMute ?? throw new ArgumentNullException(nameof(device));
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/muteVideoForParticipant", (id, content) =>
            {
                var i = content?.ToObject<MobileControlSimpleContent<int>>();
                if (i != null) _codec.MuteVideoForParticipant(i.Value);
            });
            AddAction("/unmuteVideoForParticipant", (id, content) =>
            {
                var i = content?.ToObject<MobileControlSimpleContent<int>>();
                if (i != null) _codec.UnmuteVideoForParticipant(i.Value);
            });
            AddAction("/toggleParticipantVideoMute", (id, content) =>
            {
                var i = content?.ToObject<MobileControlSimpleContent<int>>();
                if (i != null) _codec.ToggleVideoForParticipant(i.Value);
            });
        }
    }
}

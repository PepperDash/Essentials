using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    public interface IHasMeetingRecording
    {
        BoolFeedback MeetingIsRecordingFeedback { get; }

        void StartRecording();
        void StopRecording();
        void ToggleRecording();
    }

    public interface IHasMeetingRecordingWithPrompt : IHasMeetingRecording
    {
        BoolFeedback RecordConsentPromptIsVisible { get; }
    }
}
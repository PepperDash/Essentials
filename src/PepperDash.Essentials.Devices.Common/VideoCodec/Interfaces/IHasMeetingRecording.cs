using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Feedbacks;

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

        /// <summary>
        /// Used to agree or disagree to the meeting being recorded when prompted
        /// </summary>
        /// <param name="agree"></param>
        void RecordingPromptAcknowledgement(bool agree);
    }
}
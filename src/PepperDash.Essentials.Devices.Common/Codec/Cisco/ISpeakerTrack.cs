using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Codec.Cisco
{
    /// <summary>
    /// Describes the available tracking modes for a Cisco codec
    /// </summary>
    public interface ISpeakerTrack : IKeyed
    {
        bool SpeakerTrackAvailability { get; }

        BoolFeedback SpeakerTrackAvailableFeedback { get; }

        bool SpeakerTrackStatus { get; }

        void SpeakerTrackOff();
        void SpeakerTrackOn();
    }
}

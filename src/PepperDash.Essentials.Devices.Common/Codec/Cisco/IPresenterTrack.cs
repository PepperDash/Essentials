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
    /// Describes the Presenter Track controls for a Cisco codec.
    /// </summary>
    public interface IPresenterTrack : IKeyed
    {
        bool PresenterTrackAvailability { get; }

        BoolFeedback PresenterTrackAvailableFeedback { get; }

        BoolFeedback PresenterTrackStatusOffFeedback { get; }
        BoolFeedback PresenterTrackStatusFollowFeedback { get; }
        BoolFeedback PresenterTrackStatusBackgroundFeedback { get; }
        BoolFeedback PresenterTrackStatusPersistentFeedback { get; }

        bool PresenterTrackStatus { get; }

        void PresenterTrackOff();
        void PresenterTrackFollow();
        void PresenterTrackBackground();
        void PresenterTrackPersistent();
    }
}

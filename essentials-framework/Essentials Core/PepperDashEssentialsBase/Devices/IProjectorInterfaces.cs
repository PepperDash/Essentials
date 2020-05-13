using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    public interface IHasVideoMute
    {
        BoolFeedback VideoMuteIsOn { get; }

        void VideoMuteOn(bool pressRelease);
        void VideoMuteOff(bool pressRelease);
        void VideoMuteToggle(bool pressRelease);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IBasicVideoMute
    /// </summary>
    public interface IBasicVideoMute
    {
        void VideoMuteToggle();
    }

    /// <summary>
    /// Defines the contract for IBasicVideoMuteWithFeedback
    /// </summary>
    public interface IBasicVideoMuteWithFeedback : IBasicVideoMute
    {
        BoolFeedback VideoMuteIsOn { get; }

        void VideoMuteOn();
        void VideoMuteOff();
 
    }
}
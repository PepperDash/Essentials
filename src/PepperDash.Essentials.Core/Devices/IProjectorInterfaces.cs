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
        /// <summary>
        /// Toggles the video mute
        /// </summary>
        void VideoMuteToggle();
    }

    /// <summary>
    /// Defines the contract for IBasicVideoMuteWithFeedback
    /// </summary>
    public interface IBasicVideoMuteWithFeedback : IBasicVideoMute
    {
        /// <summary>
        /// Gets the VideoMuteIsOn feedback
        /// </summary>
        BoolFeedback VideoMuteIsOn { get; }

        /// <summary>
        /// Sets the video mute on
        /// </summary>
        void VideoMuteOn();

        /// <summary>
        /// Sets the video mute off
        /// </summary>
        void VideoMuteOff();
 
    }
}
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Cameras
{


    /// <summary>
    /// To be implmented on codecs that can disable their camera(s) to blank the near end video
    /// </summary>
    public interface IHasCameraOff : IHasCameraControls
    {
        /// <summary>
        /// Feedback that indicates whether the camera is off
        /// </summary>
        BoolFeedback CameraIsOffFeedback { get; }

        /// <summary>
        /// Turns the camera off, blanking the near end video
        /// </summary>
        void CameraOff();
    }
}

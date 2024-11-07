using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Defines the requred elements for selfview control
    /// </summary>
    public interface IHasCodecSelfView
    {
        BoolFeedback SelfviewIsOnFeedback { get; }

        bool ShowSelfViewByDefault { get; }

        void SelfViewModeOn();

        void SelfViewModeOff();

        void SelfViewModeToggle();
    }
}
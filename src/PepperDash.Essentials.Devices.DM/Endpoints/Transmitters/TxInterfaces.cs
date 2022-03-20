using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Defines a device capable of setting the Free Run state of a VGA input and reporting feedback
    /// </summary>
    public interface IHasFreeRun
    {
        BoolFeedback FreeRunEnabledFeedback { get; }

        void SetFreeRunEnabled(bool enable);
    }

    /// <summary>
    /// Defines a device capable of adjusting VGA settings
    /// </summary>
    public interface IVgaBrightnessContrastControls
    {
        IntFeedback VgaBrightnessFeedback { get; }
        IntFeedback VgaContrastFeedback { get; }

        void SetVgaBrightness(ushort level);
        void SetVgaContrast(ushort level);
    }
}
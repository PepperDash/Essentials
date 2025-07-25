﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for ITemperatureSensor
    /// </summary>
    public interface ITemperatureSensor
    {
        /// <summary>
        ///  The values will range from -400 to +1760 (for -40° to +176° F) or -400 to +800
        ///    (for -40° to +80° C)in tenths of a degree.
        /// </summary>
        IntFeedback TemperatureFeedback { get; }


        BoolFeedback TemperatureInCFeedback { get; }

        void SetTemperatureFormat(bool setToC);
    }
}

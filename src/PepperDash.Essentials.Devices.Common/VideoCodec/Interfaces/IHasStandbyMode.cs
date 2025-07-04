using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec;

/// <summary>
/// Describes a device that has Standby Mode capability
/// </summary>
public interface IHasStandbyMode
{
    BoolFeedback StandbyIsOnFeedback { get; }

    void StandbyActivate();

    void StandbyDeactivate();
}

/// <summary>
/// Describes a device that has Half Waek Mode capability
/// </summary>
public interface IHasHalfWakeMode : IHasStandbyMode
{
    BoolFeedback HalfWakeModeIsOnFeedback { get; }

    BoolFeedback EnteringStandbyModeFeedback { get; }

    void HalfwakeActivate();
}
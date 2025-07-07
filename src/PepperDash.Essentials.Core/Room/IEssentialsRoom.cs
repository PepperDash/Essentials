using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Core.Devices;

using PepperDash.Core;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Describes the basic functionality of an EssentialsRoom
/// </summary>
public interface IEssentialsRoom : IKeyName, IReconfigurableDevice, IRunDefaultPresentRoute, IEnvironmentalControls
{
    BoolFeedback OnFeedback { get; }        

    BoolFeedback IsWarmingUpFeedback { get; }
    BoolFeedback IsCoolingDownFeedback { get; }        

    bool IsMobileControlEnabled { get; }
    IMobileControlRoomMessenger MobileControlRoomBridge { get; }

    string SourceListKey { get; }

    string DestinationListKey { get; }

    string AudioControlPointListKey { get; }

    string CameraListKey { get; }

    SecondsCountdownTimer ShutdownPromptTimer { get; }
    int ShutdownPromptSeconds { get; }
    int ShutdownVacancySeconds { get; }
    eShutdownType ShutdownType { get; }      

    string LogoUrlLightBkgnd { get; }
    string LogoUrlDarkBkgnd { get; }

    void StartShutdown(eShutdownType type);        

    void Shutdown();        

    void PowerOnToDefaultOrLastSource();               
}
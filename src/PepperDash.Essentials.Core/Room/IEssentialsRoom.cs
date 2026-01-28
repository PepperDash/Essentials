using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Core.Devices;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes the basic functionality of an EssentialsRoom
    /// </summary>
    public interface IEssentialsRoom : IKeyName, IReconfigurableDevice, IRunDefaultPresentRoute, IEnvironmentalControls
    {
        /// <summary>
        /// Gets the PowerFeedback
        /// </summary>
        BoolFeedback OnFeedback { get; }        

        /// <summary>
        /// Gets the IsOccupiedFeedback
        /// </summary>
        BoolFeedback IsWarmingUpFeedback { get; }

        /// <summary>
        /// Gets the IsCoolingDownFeedback
        /// </summary>
        BoolFeedback IsCoolingDownFeedback { get; }        

        /// <summary>
        /// Gets a value indicating whether mobile control is enabled for this room
        /// </summary>
        bool IsMobileControlEnabled { get; }

        /// <summary>
        /// Gets the MobileControlRoomBridge
        /// </summary>
        IMobileControlRoomMessenger MobileControlRoomBridge { get; }

        /// <summary>
        /// Gets the SourceListKey
        /// </summary>
        string SourceListKey { get; }

        /// <summary>
        /// Gets the DestinationListKey
        /// </summary>
        string DestinationListKey { get; }

        /// <summary>
        /// Gets the AudioControlPointListKey
        /// </summary>
        string AudioControlPointListKey { get; }

        /// <summary>
        /// Gets the CameraListKey
        /// </summary>
        string CameraListKey { get; }

        /// <summary>
        /// Gets the ShutdownPromptTimer
        /// </summary>
        SecondsCountdownTimer ShutdownPromptTimer { get; }

        /// <summary>
        /// Gets the ShutdownVacancyTimer
        /// </summary>
        int ShutdownPromptSeconds { get; }

        /// <summary>
        /// Gets the ShutdownVacancySeconds
        /// </summary>
        int ShutdownVacancySeconds { get; }

        /// <summary>
        /// Gets the ShutdownType
        /// </summary>
        eShutdownType ShutdownType { get; }      

        /// <summary>
        /// Gets the LogoUrlLightBkgnd
        /// </summary>
        string LogoUrlLightBkgnd { get; }

        /// <summary>
        /// Gets the LogoUrlDarkBkgnd
        /// </summary>
        string LogoUrlDarkBkgnd { get; }

        /// <summary>
        /// Starts the shutdown process
        /// </summary>
        /// <param name="type">type of shutdown event</param>
        void StartShutdown(eShutdownType type);        

        /// <summary>
        /// Shuts down the room
        /// </summary>
        void Shutdown();        

        /// <summary>
        /// Powers on the room to either the default source or the last source used
        /// </summary>
        void PowerOnToDefaultOrLastSource();               
    }

}
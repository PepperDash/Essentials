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
        BoolFeedback OnFeedback { get; }

        event EventHandler<EventArgs> RoomOccupancyIsSet;

        BoolFeedback IsWarmingUpFeedback { get; }
        BoolFeedback IsCoolingDownFeedback { get; }

        IOccupancyStatusProvider RoomOccupancy { get; }
        bool OccupancyStatusProviderIsRemote { get; }

        bool IsMobileControlEnabled { get; }
        IMobileControlRoomBridge MobileControlRoomBridge { get; }

        string SourceListKey { get; }

        SecondsCountdownTimer ShutdownPromptTimer { get; }
        int ShutdownPromptSeconds { get; }
        int ShutdownVacancySeconds { get; }
        eShutdownType ShutdownType { get; }

        EssentialsRoomEmergencyBase Emergency { get; }

        Core.Privacy.MicrophonePrivacyController MicrophonePrivacy { get; }

        string LogoUrlLightBkgnd { get; }
        string LogoUrlDarkBkgnd { get; }

        eVacancyMode VacancyMode { get; }

        bool ZeroVolumeWhenSwtichingVolumeDevices { get; }

        void StartShutdown(eShutdownType type);
        void StartRoomVacancyTimer(eVacancyMode mode);

        void Shutdown();

        void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes);

        void PowerOnToDefaultOrLastSource();

        void SetDefaultLevels();

        void RoomVacatedForTimeoutPeriod(object o);
    }

}
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.Timers;

namespace PepperDash.Essentials.Core.Rooms
{
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

}
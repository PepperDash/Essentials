using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms with in call feedback
    /// </summary>
    public interface IHasInCallFeedback
    {
        BoolFeedback InCallFeedback { get; }
    }

    /// <summary>
    /// For rooms with a single display
    /// </summary>
    public interface IHasDefaultDisplay
    {
        IRoutingSink DefaultDisplay { get; }
    }

    /// <summary>
    /// For rooms with multiple displays
    /// </summary>
    public interface IHasMultipleDisplays
    {
        Dictionary<eSourceListItemDestinationTypes, IRoutingSink> Displays { get; }
    }

    /// <summary>
    /// For rooms with routing
    /// </summary>
    public interface IRunRouteAction
    {
        void RunRouteAction(string routeKey, string sourceListKey);

        void RunRouteAction(string routeKey, string sourceListKey, Action successCallback);        
    }

    /// <summary>
    /// Simplified routing direct from source to destination
    /// </summary>
    public interface IRunDirectRouteAction
    {
        void RunDirectRoute(string sourceKey, string destinationKey, eRoutingSignalType type = eRoutingSignalType.AudioVideo);
    }
    
    /// <summary>
    /// Describes a room with matrix routing
    /// </summary>
    public interface IHasMatrixRouting
    {
        string MatrixRoutingDeviceKey { get; }

        List<string> EndpointKeys { get; }
    }

    /// <summary>
    /// Describes a room with routing endpoints
    /// </summary>
    public interface IHasRoutingEndpoints
    {
        List<string> EndpointKeys { get; }
    }

    /// <summary>
    /// Describes a room with a shutdown prompt timer
    /// </summary>
    public interface IShutdownPromptTimer
    {
        SecondsCountdownTimer ShutdownPromptTimer { get; }

        void SetShutdownPromptSeconds(int seconds);
    }

    /// <summary>
    /// For rooms that default presentation only routing
    /// </summary>
    public interface IRunDefaultPresentRoute
    {
        bool RunDefaultPresentRoute();
    }

    /// <summary>
    /// For rooms that have default presentation and calling routes
    /// </summary>
    public interface IRunDefaultCallRoute : IRunDefaultPresentRoute
    {
        bool RunDefaultCallRoute();
    }

    /// <summary>
    /// Describes environmental controls available on a room such as lighting, shades, temperature, etc.
    /// </summary>
    public interface IEnvironmentalControls
    {
        List<EssentialsDevice> EnvironmentalControlDevices { get; }

        bool HasEnvironmentalControlDevices { get; }
    }

    public interface IRoomOccupancy:IKeyed
    {
        IOccupancyStatusProvider RoomOccupancy { get; }
        bool OccupancyStatusProviderIsRemote { get; }

        void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes);

        void RoomVacatedForTimeoutPeriod(object o);

        void StartRoomVacancyTimer(eVacancyMode mode);

        eVacancyMode VacancyMode { get; }

        event EventHandler<EventArgs> RoomOccupancyIsSet;
    }

    public interface IEmergency
    {
        EssentialsRoomEmergencyBase Emergency { get; }
    }

    public interface IMicrophonePrivacy
    {
        Core.Privacy.MicrophonePrivacyController MicrophonePrivacy { get; }
    }

}
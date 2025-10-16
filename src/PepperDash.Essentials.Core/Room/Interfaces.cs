using System;
using System.Collections.Generic;

using PepperDash.Core;


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
    [Obsolete("Will be removed in a future version")]
    public interface IHasMultipleDisplays
    {
        Dictionary<string, IRoutingSink> Displays { get; }
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
    /// Defines the contract for IRunDirectRouteAction
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
    /// Defines the contract for IHasRoutingEndpoints
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

        void StartShutdown(eShutdownType type);
    }

    /// <summary>
    /// Defines the contract for ITechPassword
    /// </summary>
    public interface ITechPassword
    {
        event EventHandler<TechPasswordEventArgs> TechPasswordValidateResult;

        event EventHandler<EventArgs> TechPasswordChanged;

        int TechPasswordLength { get; }

        void ValidateTechPassword(string password);

        void SetTechPassword(string oldPassword, string newPassword);
    }

    /// <summary>
    /// Represents a TechPasswordEventArgs
    /// </summary>
    public class TechPasswordEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the IsValid
        /// </summary>
        public bool IsValid { get; private set; }

        public TechPasswordEventArgs(bool isValid)
        {
            IsValid = isValid;
        }
    }

    /// <summary>
    /// Defines the contract for IRunDefaultPresentRoute
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

    public interface IRoomOccupancy : IKeyed
    {
        IOccupancyStatusProvider RoomOccupancy { get; }
        bool OccupancyStatusProviderIsRemote { get; }

        void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes);

        void RoomVacatedForTimeoutPeriod(object o);

        void StartRoomVacancyTimer(eVacancyMode mode);

        eVacancyMode VacancyMode { get; }

        event EventHandler<EventArgs> RoomOccupancyIsSet;
    }

    /// <summary>
    /// Defines the contract for IEmergency
    /// </summary>
    public interface IEmergency
    {
        EssentialsRoomEmergencyBase Emergency { get; }
    }

    /// <summary>
    /// Defines the contract for IMicrophonePrivacy
    /// </summary>
    public interface IMicrophonePrivacy
    {
        Core.Privacy.MicrophonePrivacyController MicrophonePrivacy { get; }
    }

    /// <summary>
    /// Defines the contract for IHasAccessoryDevices
    /// </summary>
    public interface IHasAccessoryDevices : IKeyName
    {
        List<string> AccessoryDeviceKeys { get; }
    }

    /// <summary>
    /// Defines the contract for IHasCiscoNavigatorTouchpanel
    /// </summary>
    public interface IHasCiscoNavigatorTouchpanel
    {
        string CiscoNavigatorTouchpanelKey { get; }
    }
}
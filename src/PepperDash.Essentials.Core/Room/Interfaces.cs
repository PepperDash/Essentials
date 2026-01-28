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
        /// <summary>
        /// Gets the InCallFeedback
        /// </summary>
        BoolFeedback InCallFeedback { get; }
    }

    /// <summary>
    /// For rooms with a single display
    /// </summary>
    public interface IHasDefaultDisplay
    {
        /// <summary>
        /// Gets the DefaultDisplay
        /// </summary>
        IRoutingSink DefaultDisplay { get; }
    }

    /// <summary>
    /// For rooms with multiple displays
    /// </summary>
    [Obsolete("Will be removed in a future version")]
    public interface IHasMultipleDisplays
    {
        /// <summary>
        /// Gets the Displays dictionary
        /// </summary>
        Dictionary<eSourceListItemDestinationTypes, IRoutingSink> Displays { get; }
    }

    /// <summary>
    /// For rooms with routing
    /// </summary>
    public interface IRunRouteAction
    {
        /// <summary>
        /// Runs a route action
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="sourceListKey"></param>
        void RunRouteAction(string routeKey, string sourceListKey);

        /// <summary>
        /// Runs a route action with a success callback
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="sourceListKey"></param>
        /// <param name="successCallback"></param>
        void RunRouteAction(string routeKey, string sourceListKey, Action successCallback);
    }

    /// <summary>
    /// Defines the contract for IRunDirectRouteAction
    /// </summary>
    public interface IRunDirectRouteAction
    {
        /// <summary>
        /// Runs a direct route
        /// </summary>
        /// <param name="sourceKey"></param>
        /// <param name="destinationKey"></param>
        /// <param name="type"></param>
        void RunDirectRoute(string sourceKey, string destinationKey, eRoutingSignalType type = eRoutingSignalType.AudioVideo);
    }

    /// <summary>
    /// Describes a room with matrix routing
    /// </summary>
    public interface IHasMatrixRouting
    {
        /// <summary>
        /// Gets the MatrixRoutingDeviceKey
        /// </summary>
        string MatrixRoutingDeviceKey { get; }

        /// <summary>
        /// Gets the EndpointKeys
        /// </summary>
        List<string> EndpointKeys { get; }
    }

    /// <summary>
    /// Defines the contract for IHasRoutingEndpoints
    /// </summary>
    public interface IHasRoutingEndpoints
    {
        /// <summary>
        /// Gets the EndpointKeys
        /// </summary>
        List<string> EndpointKeys { get; }
    }

    /// <summary>
    /// Describes a room with a shutdown prompt timer
    /// </summary>
    public interface IShutdownPromptTimer
    {
        /// <summary>
        /// Gets the ShutdownPromptTimer
        /// </summary>
        SecondsCountdownTimer ShutdownPromptTimer { get; }

        /// <summary>
        /// Gets the ShutdownPromptSeconds
        /// </summary>
        /// <param name="seconds">number of seconds to set</param>
        void SetShutdownPromptSeconds(int seconds);

        /// <summary>
        /// Starts the shutdown process
        /// </summary>
        /// <param name="type">type of shutdown event</param>
        void StartShutdown(eShutdownType type);
    }

    /// <summary>
    /// Defines the contract for ITechPassword
    /// </summary>
    public interface ITechPassword
    {
        /// <summary>
        /// Event fired when tech password validation result is available
        /// </summary>
        event EventHandler<TechPasswordEventArgs> TechPasswordValidateResult;

        /// <summary>
        /// Event fired when tech password is changed
        /// </summary>
        event EventHandler<EventArgs> TechPasswordChanged;

        /// <summary>
        /// Gets the TechPasswordLength
        /// </summary>
        int TechPasswordLength { get; }

        /// <summary>
        /// Validates the tech password
        /// </summary>
        /// <param name="password">The tech password to validate</param>
        void ValidateTechPassword(string password);

        /// <summary>
        /// Sets the tech password
        /// </summary>
        /// <param name="oldPassword">The current tech password</param>
        /// <param name="newPassword">The new tech password to set</param>
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

        /// <summary>
        /// Constructor for TechPasswordEventArgs
        /// </summary>
        /// <param name="isValid"></param>
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
        /// <summary>
        /// Runs the default present route
        /// </summary>
        /// <returns></returns>
        bool RunDefaultPresentRoute();
    }

    /// <summary>
    /// For rooms that have default presentation and calling routes
    /// </summary>
    public interface IRunDefaultCallRoute : IRunDefaultPresentRoute
    {
        /// <summary>
        /// Runs the default call route
        /// </summary>
        /// <returns></returns>
        bool RunDefaultCallRoute();
    }

    /// <summary>
    /// Describes environmental controls available on a room such as lighting, shades, temperature, etc.
    /// </summary>
    public interface IEnvironmentalControls
    {
        /// <summary>
        /// Gets the EnvironmentalControlDevices
        /// </summary>
        List<EssentialsDevice> EnvironmentalControlDevices { get; }

        /// <summary>
        /// Gets a value indicating whether the room has environmental control devices
        /// </summary>
        bool HasEnvironmentalControlDevices { get; }
    }

    /// <summary>
    /// Defines the contract for IRoomOccupancy
    /// </summary>
    public interface IRoomOccupancy : IKeyed
    {
        /// <summary>
        /// Gets the RoomOccupancy
        /// </summary>
        IOccupancyStatusProvider RoomOccupancy { get; }

        /// <summary>
        /// Gets a value indicating whether the OccupancyStatusProviderIsRemote
        /// </summary>
        bool OccupancyStatusProviderIsRemote { get; }

        /// <summary>
        /// Sets the room occupancy
        /// </summary>
        /// <param name="statusProvider"></param>
        /// <param name="timeoutMinutes"></param>
        void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes);

        /// <summary>
        /// Called when the room has been vacated for the timeout period
        /// </summary>
        /// <param name="o"></param>
        void RoomVacatedForTimeoutPeriod(object o);

        /// <summary>
        /// Starts the room vacancy timer
        /// </summary>
        /// <param name="mode">vacancy mode</param>
        void StartRoomVacancyTimer(eVacancyMode mode);

        /// <summary>
        /// Gets the VacancyMode
        /// </summary>
        eVacancyMode VacancyMode { get; }

        /// <summary>
        /// Event fired when room occupancy is set
        /// </summary>
        event EventHandler<EventArgs> RoomOccupancyIsSet;
    }

    /// <summary>
    /// Defines the contract for IEmergency
    /// </summary>
    public interface IEmergency
    {
        /// <summary>
        /// Gets the Emergency
        /// </summary>
        EssentialsRoomEmergencyBase Emergency { get; }
    }

    /// <summary>
    /// Defines the contract for IMicrophonePrivacy
    /// </summary>
    public interface IMicrophonePrivacy
    {
        /// <summary>
        /// Gets the MicrophonePrivacy
        /// </summary>
        Core.Privacy.MicrophonePrivacyController MicrophonePrivacy { get; }
    }

    /// <summary>
    /// Defines the contract for IHasAccessoryDevices
    /// </summary>
    public interface IHasAccessoryDevices : IKeyName
    {
        /// <summary>
        /// Gets the AccessoryDeviceKeys
        /// </summary>
        List<string> AccessoryDeviceKeys { get; }
    }

    /// <summary>
    /// Defines the contract for IHasCiscoNavigatorTouchpanel
    /// </summary>
    public interface IHasCiscoNavigatorTouchpanel
    {
        /// <summary>
        /// Gets the CiscoNavigatorTouchpanelKey
        /// </summary>
        string CiscoNavigatorTouchpanelKey { get; }
    }
}
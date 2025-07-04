using System;
using System.Collections.Generic;

using PepperDash.Core;


namespace PepperDash.Essentials.Core;

/// <summary>
/// For rooms with in call feedback
/// </summary>
public interface IHasInCallFeedback
{
    /// <summary>
    /// Feedback indicating whether the room is currently in a call
    /// </summary>
    BoolFeedback InCallFeedback { get; }
}

/// <summary>
/// For rooms with a single display
/// </summary>
public interface IHasDefaultDisplay
{
    /// <summary>
    /// The default display for the room, used for presentation routing and other default routes
    /// </summary>
    IRoutingSink DefaultDisplay { get; }
}

/// <summary>
/// For rooms with multiple displays
/// </summary>
[Obsolete("This interface is being deprecated in favor of using destination lists for routing to multiple displays.")]
public interface IHasMultipleDisplays
{
    /// <summary>
    /// A dictionary of displays in the room with the key being the type of display (presentation, calling, etc.) and the value being the display device
    /// </summary>
    Dictionary<eSourceListItemDestinationTypes, IRoutingSink> Displays { get; }
}

/// <summary>
/// For rooms with routing
/// </summary>
public interface IRunRouteAction
{
    /// <summary>
    /// Runs a route action for a given route and source list key. 
    /// The source list key is used to determine the source for the route from the source list in the room's routing controller. 
    /// This allows for dynamic routing based on what source is selected in the UI for a given route.
    /// </summary>
    /// <param name="routeKey"></param>
    /// <param name="sourceListKey"></param>
    void RunRouteAction(string routeKey, string sourceListKey);

    /// <summary>
    /// Runs a route action for a given route and source list key with a callback for success. 
    /// The source list key is used to determine the source for the route from the source list in the room's routing controller. 
    /// This allows for dynamic routing based on what source is selected in the UI for a given route.
    /// </summary>
    /// <param name="routeKey"></param>
    /// <param name="sourceListKey"></param>
    /// <param name="successCallback"></param>
    void RunRouteAction(string routeKey, string sourceListKey, Action successCallback);        
}

/// <summary>
/// Simplified routing direct from source to destination
/// </summary>
public interface IRunDirectRouteAction
{
    /// <summary>
    /// Runs a direct route from a source to a destination with an optional signal type for routing.
    /// </summary>
    /// <param name="sourceKey"></param>
    /// <param name="destinationKey"></param>
    /// <param name="type"></param>
    void RunDirectRoute(string sourceKey, string destinationKey, eRoutingSignalType type = eRoutingSignalType.AudioVideo);
}

/// <summary>
/// Describes a room with matrix routing
/// </summary>
public interface IHasMatrixRouting : IHasRoutingEndpoints
{
    /// <summary>
    /// The key of the matrix routing device in the room
    /// </summary>
    string MatrixRoutingDeviceKey { get; }
}

/// <summary>
/// Describes a room with routing endpoints
/// </summary>
public interface IHasRoutingEndpoints
{   
    /// <summary>
    /// The keys of the endpoints in the room used for routing
    /// </summary>
    List<string> EndpointKeys { get; }
}

/// <summary>
/// Describes a room with a shutdown prompt timer
/// </summary>
public interface IShutdownPromptTimer
{
    /// <summary>
    /// The shutdown prompt timer for the room
    /// </summary>
    SecondsCountdownTimer ShutdownPromptTimer { get; }

    /// <summary>
    /// Sets the number of seconds for the shutdown prompt timer
    /// </summary>
    /// <param name="seconds"></param>
    void SetShutdownPromptSeconds(int seconds);

    /// <summary>
    /// Starts the shutdown process for the room with a given shutdown type
    /// </summary>
    /// <param name="type"></param>
    void StartShutdown(eShutdownType type);
}

/// <summary>
/// Describes a room with a tech password
/// </summary>
public interface ITechPassword
{
    /// <summary>
    /// Event that fires with the result of validating a tech password
    /// </summary>
    event EventHandler<TechPasswordEventArgs> TechPasswordValidateResult;

    /// <summary>
    /// Event that fires when the tech password is changed
    /// </summary>
    event EventHandler<EventArgs> TechPasswordChanged;

    /// <summary>
    /// The length of the tech password
    /// </summary>
    int TechPasswordLength { get; }

    /// <summary>
    /// Validates a given tech password against the current tech password for the room. Fires the TechPasswordValidateResult event with the result of the validation.
    /// </summary>
    /// <param name="password"></param>
    void ValidateTechPassword(string password);

    /// <summary>
    /// Sets a new tech password for the room. Fires the TechPasswordChanged event when the password is successfully changed.
    /// </summary>
    /// <param name="oldPassword"></param>
    /// <param name="newPassword"></param>
    void SetTechPassword(string oldPassword, string newPassword);
}

/// <summary>
/// Event args for tech password validation results
/// </summary>
public class TechPasswordEventArgs : EventArgs
{
    /// <summary>
    /// Indicates whether the tech password validation was successful or not
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="isValid"></param>
    public TechPasswordEventArgs(bool isValid)
    {
        IsValid = isValid;
    }
}

/// <summary>
/// For rooms that default presentation only routing
/// </summary>
public interface IRunDefaultPresentRoute
{
    /// <summary>
    /// Runs the default presentation route for the room. This is typically used for a "Present" button in the UI that routes a selected source to the default presentation display without needing to specify the source or destination for the route.
    /// </summary>
    /// <returns>True if the route was successfully run, false otherwise.</returns>
    bool RunDefaultPresentRoute();
}

/// <summary>
/// For rooms that have default presentation and calling routes
/// </summary>
public interface IRunDefaultCallRoute : IRunDefaultPresentRoute
{
    /// <summary>
    /// Runs the default call route for the room. This is typically used for a "Call" button in the UI that routes a selected source to the default call display without needing to specify the source or destination for the route.
    /// </summary>
    /// <returns>True if the route was successfully run, false otherwise.</returns>
    bool RunDefaultCallRoute();
}

/// <summary>
/// Describes environmental controls available on a room such as lighting, shades, temperature, etc.
/// </summary>
public interface IEnvironmentalControls
{
    /// <summary>
    /// A list of devices in the room that can be used for environmental control such as lighting, shades, temperature, etc.
    /// </summary>
    List<EssentialsDevice> EnvironmentalControlDevices { get; }

    /// <summary>
    /// Indicates whether the room has any devices that can be used for environmental control such as lighting, shades, temperature, etc.
    /// </summary>
    bool HasEnvironmentalControlDevices { get; }
}

/// <summary>
/// Describes a room with occupancy status
/// </summary>
public interface IRoomOccupancy:IKeyed
{
    /// <summary>
    /// The occupancy status provider for the room, which provides the current occupancy status of the room based on a sensor or other input. 
    /// This is used for features such as automatic shutdown when the room is vacant, adjusting environmental controls based on occupancy, and other occupancy-based automation.
    /// </summary>
    IOccupancyStatusProvider RoomOccupancy { get; }

    /// <summary>
    /// Indicates whether the occupancy status provider for the room is remote, meaning it is not directly connected to the control system and may require additional setup or configuration to work properly. 
    /// This can be used to determine whether certain features that rely on occupancy status should be enabled or disabled for the room.
    /// </summary>
    bool OccupancyStatusProviderIsRemote { get; }

    /// <summary>
    /// Sets the occupancy status provider for the room with a given timeout period for determining when the room is vacant. T
    /// his is used to configure the occupancy-based features of the room based on the specific occupancy sensor or input being used and the desired timeout period for determining vacancy.
    /// </summary>
    /// <param name="statusProvider"></param>
    /// <param name="timeoutMinutes"></param>
    void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes);

    /// <summary>
    /// Event handler for when the room is vacated for the timeout period, which can be used to trigger actions such as shutting down the room, adjusting environmental controls, or other automation based on vacancy.
    /// </summary>
    /// <param name="o"></param>
    void RoomVacatedForTimeoutPeriod(object o);

    /// <summary>
    /// Starts the timer for determining when the room is vacant based on the specified vacancy mode, which can be used to trigger actions such as shutting down the room, adjusting environmental controls, or other automation based on vacancy.
    /// </summary>
    /// <param name="mode"></param>
    void StartRoomVacancyTimer(eVacancyMode mode);

    /// <summary>
    /// Gets the current vacancy mode for the room, which indicates the current state of the room in terms of occupancy and can be used to determine whether certain features or automation should be enabled or disabled based on vacancy.
    /// </summary>
    eVacancyMode VacancyMode { get; }

    /// <summary>
    /// Event that fires when the occupancy status for the room is set, which can be used to trigger actions such as updating the UI, adjusting environmental controls, or other automation based on occupancy status changes.
    /// </summary>
    event EventHandler<EventArgs> RoomOccupancyIsSet;
}

/// <summary>
/// Describes a room with emergency features
/// </summary>
public interface IEmergency
{
    /// <summary>
    /// The emergency base for the room, which provides access to emergency features such as triggering an emergency alert, contacting emergency services, or other emergency-related functionality.
    /// </summary>
    EssentialsRoomEmergencyBase Emergency { get; }
}

/// <summary>
/// Describes a room with a microphone privacy controller
/// </summary>
public interface IMicrophonePrivacy
{
    /// <summary>
    /// The microphone privacy controller for the room, which provides access to features such as muting or unmuting microphones for privacy purposes.
    /// </summary>
    Core.Privacy.MicrophonePrivacyController MicrophonePrivacy { get; }
}

/// <summary>
/// Describes a room with a camera privacy controller
/// </summary>
public interface IHasAccessoryDevices : IKeyName
{
    /// <summary>
    /// A list of keys for accessory devices in the room such as cameras, microphones, or other devices that are not part of the main control system but can be integrated for additional functionality.
    /// </summary>
    List<string> AccessoryDeviceKeys { get; }
}

/// <summary>
/// Describes a room wih a Cisco Navigator touchpanel
/// </summary>
public interface IHasCiscoNavigatorTouchpanel
{
    /// <summary>
    /// The key for the Cisco Navigator touchpanel in the room, which can be used to route calls or content to the touchpanel or for other integration purposes.
    /// </summary>
    string CiscoNavigatorTouchpanelKey { get; }
}
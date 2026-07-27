using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes the functionality for an EssentailsRoomCombiner device
    /// </summary>
    public interface IEssentialsRoomCombiner : IKeyed
    {
        /// <summary>
        /// Indicates that the room combination scenario has changed
        /// </summary>
        event EventHandler<EventArgs> RoomCombinationScenarioChanged;


        /// <summary>
        /// The current room combination scenario
        /// </summary>
        [JsonProperty("currentScenario")]
        IRoomCombinationScenario CurrentScenario { get; }


        /// <summary>
        /// When true, indicates the current mode is auto mode
        /// </summary>
        [JsonIgnore]
        BoolFeedback IsInAutoModeFeedback {get;}

        /// <summary>
        /// Gets a value indicating whether the automatic mode is disabled.
        /// </summary>
        [JsonProperty("disableAutoMode")]
        bool DisableAutoMode { get; }
        /// <summary>
        /// Gets a value indicating whether the system is operating in automatic mode.
        /// </summary>
        [JsonProperty("isInAutoMode")]
        bool IsInAutoMode { get; }

        /// <summary>
        /// Gets the collection of rooms associated with the current object.
        /// </summary>
        [JsonProperty("rooms")]
        List<IKeyName> Rooms { get; }

        /// <summary>
        /// Sets auto mode
        /// </summary>
        void SetAutoMode();

        /// <summary>
        /// Sets manual mode
        /// </summary>
        void SetManualMode();

        /// <summary>
        /// Toggles the current mode between auto and manual
        /// </summary>
        void ToggleMode();

        /// <summary>
        /// The available room combinatino scenarios
        /// </summary>
        [JsonProperty("roomCombinationScenarios")]
        List<IRoomCombinationScenario> RoomCombinationScenarios { get; }

        /// <summary>
        /// The partition
        /// </summary>
        [JsonProperty("partitions")]
        List<IPartitionController> Partitions { get; }

        /// <summary>
        /// Toggles the state of a manual partition sensor
        /// </summary>
        /// <param name="partitionKey"></param>
        void TogglePartitionState(string partitionKey);

        /// <summary>
        /// Sets the room combination scenario (if in manual mode)
        /// </summary>
        /// <param name="scenarioKey"></param>
        void SetRoomCombinationScenario(string scenarioKey);
    }

    /// <summary>
    /// Optional extension for room combiners that provide operation lifecycle status.
    /// </summary>
    public interface IEssentialsRoomCombinerWithOperationStatus : IEssentialsRoomCombiner
    {
        /// <summary>
        /// Indicates that the room combination operation status has changed.
        /// </summary>
        event EventHandler<EventArgs> CombinationOperationStatusChanged;

        /// <summary>
        /// Gets the current room combination operation status.
        /// </summary>
        [JsonProperty("combinationOperation")]
        CombinationOperationStatus CombinationOperation { get; }
    }

    /// <summary>
    /// Defines lifecycle states for a room combination operation.
    /// </summary>
    public enum CombinationOperationState
    {
        Idle,
        InProgress,
        Completed,
        Failed,
        TimedOut
    }

    /// <summary>
    /// Represents room combination operation status details.
    /// </summary>
    public class CombinationOperationStatus
    {
        [JsonProperty("operationId", NullValueHandling = NullValueHandling.Ignore)]
        public string OperationId { get; set; }

        [JsonProperty("scenarioKey", NullValueHandling = NullValueHandling.Ignore)]
        public string ScenarioKey { get; set; }

        [JsonProperty("startedUtc", NullValueHandling = NullValueHandling.Ignore)]
        public string StartedUtc { get; set; }

        [JsonProperty("state")]
        public CombinationOperationState State { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }

    /// <summary>
    /// Represents a scenario for combining rooms, including activation, deactivation, and associated state.
    /// </summary>
    /// <remarks>This interface defines the behavior for managing room combination scenarios, including
    /// activation and deactivation, tracking the active state, and managing related partition states and UI mappings.
    /// Implementations of this interface are expected to handle the logic for room combinations based on the provided
    /// partition states and UI mappings.</remarks>
    /// <summary>
    /// Defines the contract for IRoomCombinationScenario
    /// </summary>
    public interface IRoomCombinationScenario : IKeyName
    {
        /// <summary>
        /// When true, indicates that this room combination scenario is active
        /// </summary>
        [JsonIgnore]
        BoolFeedback IsActiveFeedback { get; }

        /// <summary>
        /// Gets a value indicating whether the entity is active.
        /// </summary>
        [JsonProperty("isActive")]
        bool IsActive { get; }

        /// <summary>
        /// Gets a value indicating whether this scenario should be hidden in the UI.
        /// </summary>
        [JsonProperty("hideInUi")]
        bool HideInUi { get; }

        /// <summary>
        /// Activates this room combination scenario
        /// </summary>
        Task Activate();

        /// <summary>
        /// Deactivates this room combination scenario
        /// </summary>
        Task Deactivate();

        /// <summary>
        /// The state of the partitions that would activate this scenario
        /// </summary>
        [JsonProperty("partitionStates")]
        List<PartitionState> PartitionStates { get; }

        /// <summary>
        /// The mapping of UIs by key to rooms by key
        /// </summary>
        [JsonProperty("uiMap")]
        Dictionary<string, string> UiMap { get; set; }
    }

}
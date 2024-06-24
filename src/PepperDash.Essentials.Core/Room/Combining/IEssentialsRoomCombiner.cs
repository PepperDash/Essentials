using System;
using System.Collections.Generic;
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

        [JsonProperty("isInAutoMode")]
        bool IsInAutoMode { get; }

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

    public interface IRoomCombinationScenario : IKeyName
    {
        /// <summary>
        /// When true, indicates that this room combination scenario is active
        /// </summary>
        [JsonIgnore]
        BoolFeedback IsActiveFeedback { get; }

        [JsonProperty("isActive")]
        bool IsActive { get; }

        /// <summary>
        /// Activates this room combination scenario
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivates this room combination scenario
        /// </summary>
        void Deactivate();

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
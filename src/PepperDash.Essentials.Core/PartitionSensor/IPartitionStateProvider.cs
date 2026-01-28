using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IPartitionStateProvider
    /// </summary>
    public interface IPartitionStateProvider : IKeyName
    {
        /// <summary>
        /// Feedback indicating whether the partition is present
        /// </summary>
        [JsonIgnore]
        BoolFeedback PartitionPresentFeedback { get; }

        /// <summary>
        /// Indicates whether the partition is present
        /// </summary>
        [JsonProperty("partitionPresent")]
        bool PartitionPresent { get; }
    }

    /// <summary>
    /// Defines the contract for IPartitionController
    /// </summary>
    public interface IPartitionController : IPartitionStateProvider
    {
        /// <summary>
        /// List of adjacent room keys
        /// </summary>
        [JsonProperty("adjacentRoomKeys")]
        List<string> AdjacentRoomKeys { get; }

        /// <summary>
        /// Indicates whether the controller is in Auto mode or Manual mode
        /// </summary>
        [JsonProperty("isInAutoMode")]
        bool IsInAutoMode { get; }

        /// <summary>
        /// Sets the PartitionPresent state
        /// </summary>
        void SetPartitionStatePresent();

        /// <summary>
        /// Sets the PartitionPresent state to not present
        /// </summary>
        void SetPartitionStateNotPresent();

        /// <summary>
        /// Toggles the PartitionPresent state
        /// </summary>
        void ToggglePartitionState();

        /// <summary>
        /// Sets the controller to Manual mode
        /// </summary>
        void SetManualMode();

        /// <summary>
        /// Sets the controller to Auto mode
        /// </summary>
        void SetAutoMode();
    }
}
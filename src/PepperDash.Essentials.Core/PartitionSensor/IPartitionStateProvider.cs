using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.PartitionSensor
{
    /// <summary>
    /// Defines the contract for IPartitionStateProvider
    /// </summary>
    public interface IPartitionStateProvider : IKeyName
    {
        [JsonIgnore]
        BoolFeedback PartitionPresentFeedback { get; }

        [JsonProperty("partitionPresent")]
        bool PartitionPresent { get; }
    }

    /// <summary>
    /// Defines the contract for IPartitionController
    /// </summary>
    public interface IPartitionController : IPartitionStateProvider
    {
        [JsonProperty("adjacentRoomKeys")]
        List<string> AdjacentRoomKeys { get; }

        [JsonProperty("isInAutoMode")]
        bool IsInAutoMode { get; }

        void SetPartitionStatePresent();

        void SetPartitionStateNotPresent();

        void ToggglePartitionState();

        void SetManualMode();

        void SetAutoMode();
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.PartitionSensor
{
    /// <summary>
    /// Describes the functionality of a device that senses and provides partition state
    /// </summary>
    public interface IPartitionStateProvider : IKeyName
    {
        [JsonIgnore]
        BoolFeedback PartitionPresentFeedback { get; }

        [JsonProperty("partitionPresent")]
        bool PartitionPresent { get; }
    }

    /// <summary>
    /// Describes the functionality of a device that can provide partition state either manually via user input or optionally via a sensor state
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
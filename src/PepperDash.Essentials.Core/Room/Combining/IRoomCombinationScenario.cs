using System.Collections.Generic;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public interface IRoomCombinationScenario : IKeyName
    {
        /// <summary>
        /// When true, indicates that this room combination scenario is active
        /// </summary>
        BoolFeedback IsActiveFeedback { get; }

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
        List<PartitionState> PartitionStates { get; }

        /// <summary>
        /// The mapping of UIs by key to rooms by key
        /// </summary>
        Dictionary<string, string> UiMap { get; set; }
    }
}
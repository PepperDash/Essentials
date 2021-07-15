using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes the functionality for an EssentailsRoomCombiner device
    /// </summary>
    public interface IEssentialsRoomCombiner
    {
        // TODO: Update the EventArgs class as needed to specify scenario change
        event EventHandler<EventArgs> RoomCombinationScenarioChanged;

        BoolFeedback IsInAutoModeFeedback {get;}

        void SetAutoMode();

        void SetManualMode();

        void ToggleMode();

        List<IRoomCombinationScenario> Scenarios { get; }

        List<IPartitionStateProvider> Partitions { get; }

        void TogglePartitionState(string partitionKey);
    }

    public interface IRoomCombinationScenario : IKeyName
    {
        BoolFeedback IsActive { get; }

        void Activate();
    }

}
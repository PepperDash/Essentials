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
        /// <summary>
        /// Indicates that the room combination scenario has changed
        /// </summary>
        event EventHandler<EventArgs> RoomCombinationScenarioChanged;

        /// <summary>
        /// The current room combination scenario
        /// </summary>
        IRoomCombinationScenario CurrentScenario { get; }

        /// <summary>
        /// When true, indicates the current mode is auto mode
        /// </summary>
        BoolFeedback IsInAutoModeFeedback {get;}

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
        List<IRoomCombinationScenario> RoomCombinationScenarios { get; }

        /// <summary>
        /// The partition
        /// </summary>
        List<IPartitionStateProvider> PartitionStateProviders { get; }

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
        BoolFeedback IsActiveFeedback { get; }

        /// <summary>
        /// Activates this room combination scenario
        /// </summary>
        void Activate();
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a room combination scenario
    /// </summary>
    public class RoomCombinationScenario: IRoomCombinationScenario
    {
        private RoomCombinationScenarioConfig _config;

        public string Key { get; set; }

        public string Name { get; set; }

        public List<PartitionState> PartitionStates { get; private set; }

        public Dictionary<string, string> EnabledRoomMap { get; set; }

        private bool _isActive;

        public BoolFeedback IsActiveFeedback { get; private set; }

        List<DeviceActionWrapper> activationActions;

        List<DeviceActionWrapper> deactivationActions;

        public RoomCombinationScenario(RoomCombinationScenarioConfig config)
        {
            Key = config.Key;

            Name = config.Name;

            PartitionStates = config.PartitionStates;

            EnabledRoomMap = config.RoomMap;

            activationActions = config.ActivationActions;

            deactivationActions = config.DeactivationActions;

            _config = config;

            IsActiveFeedback = new BoolFeedback(() => _isActive);
        }

        public void Activate()
        {
            if (activationActions != null)
            {
                foreach (var action in activationActions)
                {
                    DeviceJsonApi.DoDeviceAction(action);
                }
            }

            _isActive = true;
            IsActiveFeedback.FireUpdate();
        }

        public void Deactivate()
        {
            if (deactivationActions != null)
            {
                foreach (var action in deactivationActions)
                {
                    DeviceJsonApi.DoDeviceAction(action);
                }
            }

            _isActive = false;
            IsActiveFeedback.FireUpdate();
        }

    }

}
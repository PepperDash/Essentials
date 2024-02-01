

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

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

        public Dictionary<string, string> UiMap { get; set; }

        private bool _isActive;

        public BoolFeedback IsActiveFeedback { get; private set; }

        private List<DeviceActionWrapper> activationActions;

        private List<DeviceActionWrapper> deactivationActions;

        public RoomCombinationScenario(RoomCombinationScenarioConfig config)
        {
            Key = config.Key;

            Name = config.Name;

            PartitionStates = config.PartitionStates;

            UiMap = config.UiMap;

            activationActions = config.ActivationActions;

            deactivationActions = config.DeactivationActions;

            _config = config;

            IsActiveFeedback = new BoolFeedback(() => _isActive);
        }

        public void Activate()
        {
            Debug.Console(1, "Activating Scenario: '{0}' with {1} action(s) defined", Name, activationActions.Count);   

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
            Debug.Console(1, "Deactivating Scenario: '{0}' with {1} action(s) defined", Name, deactivationActions.Count);

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
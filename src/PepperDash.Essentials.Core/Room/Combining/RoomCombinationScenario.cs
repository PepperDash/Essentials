

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

using Newtonsoft.Json;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a room combination scenario
    /// </summary>
    public class RoomCombinationScenario: IRoomCombinationScenario, IKeyName
    {
        private RoomCombinationScenarioConfig _config;

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("partitionStates")]
        public List<PartitionState> PartitionStates { get; private set; }

        [JsonProperty("uiMap")]
        public Dictionary<string, string> UiMap { get; set; }

        private bool _isActive;

        [JsonProperty("isActive")]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if(value == _isActive)
                {
                    return;
                }

                _isActive = value;
                IsActiveFeedback.FireUpdate();
            }
        }

        [JsonIgnore]
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
            Debug.LogMessage(LogEventLevel.Debug, "Activating Scenario: '{0}' with {1} action(s) defined", Name, activationActions.Count);   

            if (activationActions != null)
            {
                foreach (var action in activationActions)
                {
                    DeviceJsonApi.DoDeviceAction(action);
                }
            }

            IsActive = true;
        }

        public void Deactivate()
        {
            Debug.LogMessage(LogEventLevel.Debug, "Deactivating Scenario: '{0}' with {1} action(s) defined", Name, deactivationActions.Count);

            if (deactivationActions != null)
            {
                foreach (var action in deactivationActions)
                {
                    DeviceJsonApi.DoDeviceAction(action);
                }
            }

            IsActive = false;
        }

    }

}
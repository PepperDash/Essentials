using Newtonsoft.Json;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Rooms.Combining
{
    /// <summary>
    /// Represents a room combination scenario
    /// </summary>
    public class RoomCombinationScenario : IRoomCombinationScenario, IKeyName
    {
        private RoomCombinationScenarioConfig _config;

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("partitionStates")]
        /// <summary>
        /// Gets or sets the PartitionStates
        /// </summary>
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
                if (value == _isActive)
                {
                    return;
                }

                _isActive = value;
                IsActiveFeedback.FireUpdate();
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Gets or sets the IsActiveFeedback
        /// </summary>
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

        public async Task Activate()
        {
            this.LogInformation("Activating Scenario {name} with {activationActionCount} action(s) defined", Name, activationActions.Count);

            var tasks = new List<Task>();

            if (activationActions != null)
            {
                foreach (var action in activationActions)
                {
                    this.LogInformation("Running Activation action {@action}", action);
                    await DeviceJsonApi.DoDeviceActionAsync(action);
                }
            }

            IsActive = true;
        }

        public async Task Deactivate()
        {
            this.LogInformation("Deactivating Scenario {name} with {deactivationActionCount} action(s) defined", Name, deactivationActions.Count);

            var tasks = new List<Task>();

            if (deactivationActions != null)
            {
                foreach (var action in deactivationActions)
                {
                    this.LogInformation("Running deactivation action {actionDeviceKey}:{actionMethod}", action.DeviceKey, action.MethodName);
                    await DeviceJsonApi.DoDeviceActionAsync(action);
                }
            }

            IsActive = false;
        }

    }

}
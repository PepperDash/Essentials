using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using Serilog.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a room combination scenario
    /// </summary>
    public class RoomCombinationScenario : IRoomCombinationScenario, IKeyName
    {
        private RoomCombinationScenarioConfig _config;

        /// <summary>
        /// Gets or sets the key associated with the object.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets a value indicating whether to hide this scenario in the UI.
        /// </summary>
        ///        
        [JsonProperty("hideInUi")]

        public bool HideInUi
        {
            get { return _config.HideInUi; }
        }

        /// <summary>
        /// Gets or sets the PartitionStates
        /// </summary>
        ///
        [JsonProperty("partitionStates")]

        public List<PartitionState> PartitionStates { get; private set; }

        /// <summary>
        /// Determines which UI devices get mapped to which room in this scenario.  The Key should be the key of the UI device and the Value should be the key of the room to map to
        /// </summary>
        [JsonProperty("uiMap")]
        public Dictionary<string, string> UiMap { get; set; }

        private bool _isActive;

        /// <summary>
        /// Gets or sets IsActive
        /// </summary>
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

        /// <summary>
        /// Gets or sets the IsActiveFeedback
        /// </summary>
        [JsonIgnore]
        public BoolFeedback IsActiveFeedback { get; private set; }

        private List<DeviceActionWrapper> activationActions;

        private List<DeviceActionWrapper> deactivationActions;

        /// <summary>
        /// Constructor for RoomCombinationScenario
        /// </summary>
        /// <param name="config">config of the room combine scenario</param>
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

        /// <summary>
        /// Activates the scenario
        /// </summary>
        /// <returns></returns>
        public async Task Activate()
        {
            this.LogInformation("Activating Scenario {name} with {activationActionCount} action(s) defined", Name, activationActions.Count);

            List<Task> tasks = new List<Task>();

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

        /// <summary>
        /// Deactivates the scenario
        /// </summary>
        /// <returns></returns>
        public async Task Deactivate()
        {
            this.LogInformation("Deactivating Scenario {name} with {deactivationActionCount} action(s) defined", Name, deactivationActions.Count);

            List<Task> tasks = new List<Task>();

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
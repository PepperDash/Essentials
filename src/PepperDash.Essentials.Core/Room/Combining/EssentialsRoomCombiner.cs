using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.PartitionSensor;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Room.Combining
{
    public class EssentialsRoomCombiner : EssentialsDevice, IEssentialsRoomCombiner
    {
        private EssentialsRoomCombinerPropertiesConfig _propertiesConfig;

        private IRoomCombinationScenario _currentScenario;

        private List<IEssentialsRoom> _rooms;

        public List<IKeyName> Rooms
        {
            get
            {
                return _rooms.Cast<IKeyName>().ToList();
            }
        }

        private bool _isInAutoMode;

        public bool IsInAutoMode
        {
            get
            {
                return _isInAutoMode;
            }
            set
            {
                if (value == _isInAutoMode)
                {
                    return;
                }

                _isInAutoMode = value;
                IsInAutoModeFeedback.FireUpdate();
            }
        }

        private CTimer _scenarioChangeDebounceTimer;

        private int _scenarioChangeDebounceTimeSeconds = 10; // default to 10s

        private Mutex _scenarioChange = new Mutex();

        public EssentialsRoomCombiner(string key, EssentialsRoomCombinerPropertiesConfig props)
            : base(key)
        {
            _propertiesConfig = props;

            Partitions = new List<IPartitionController>();
            RoomCombinationScenarios = new List<IRoomCombinationScenario>();

            if (_propertiesConfig.ScenarioChangeDebounceTimeSeconds > 0)
            {
                _scenarioChangeDebounceTimeSeconds = _propertiesConfig.ScenarioChangeDebounceTimeSeconds;
            }

            IsInAutoModeFeedback = new BoolFeedback(() => _isInAutoMode);

            // default to auto mode
            IsInAutoMode = true;

            if (_propertiesConfig.defaultToManualMode)
            {
                IsInAutoMode = false;
            }

            IsInAutoModeFeedback.FireUpdate();

            CreateScenarios();

            AddPostActivationAction(() =>
            {
                SetupPartitionStateProviders();

                SetRooms();
            });


            // Subscribe to the AllDevicesInitialized event
            // We need to wait until all devices are initialized in case
            // any actions are dependent on 3rd party devices already being
            // connected and initialized
            DeviceManager.AllDevicesInitialized += (o, a) =>
            {
                if (IsInAutoMode)
                {
                    DetermineRoomCombinationScenario();
                }
                else
                {
                    SetRoomCombinationScenario(_propertiesConfig.defaultScenarioKey);
                }
            };
        }

        private void CreateScenarios()
        {
            foreach (var scenarioConfig in _propertiesConfig.Scenarios)
            {
                var scenario = new RoomCombinationScenario(scenarioConfig);
                RoomCombinationScenarios.Add(scenario);
            }
        }

        private void SetRooms()
        {
            _rooms = new List<IEssentialsRoom>();

            foreach (var roomKey in _propertiesConfig.RoomKeys)
            {
                var room = DeviceManager.GetDeviceForKey(roomKey);

                if (DeviceManager.GetDeviceForKey(roomKey) is IEssentialsRoom essentialsRoom)
                {
                    _rooms.Add(essentialsRoom);
                }
            }

            var rooms = DeviceManager.AllDevices.OfType<IEssentialsRoom>().Cast<Device>();

            foreach (var room in rooms)
            {
                room.Deactivate();
            }
        }

        private void SetupPartitionStateProviders()
        {
            foreach (var pConfig in _propertiesConfig.Partitions)
            {
                var sensor = DeviceManager.GetDeviceForKey(pConfig.DeviceKey) as IPartitionStateProvider;

                var partition = new EssentialsPartitionController(pConfig.Key, pConfig.Name, sensor, _propertiesConfig.defaultToManualMode, pConfig.AdjacentRoomKeys);

                partition.PartitionPresentFeedback.OutputChange += PartitionPresentFeedback_OutputChange;

                Partitions.Add(partition);
            }
        }

        private void PartitionPresentFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            StartDebounceTimer();
        }

        private void StartDebounceTimer()
        {
            // default to 500ms for manual mode
            var time = 500;

            // if in auto mode, debounce the scenario change
            if (IsInAutoMode) time = _scenarioChangeDebounceTimeSeconds * 1000;

            if (_scenarioChangeDebounceTimer == null)
            {
                _scenarioChangeDebounceTimer = new CTimer((o) => DetermineRoomCombinationScenario(), time);
            }
            else
            {
                _scenarioChangeDebounceTimer.Reset(time);
            }
        }

        /// <summary>
        /// Determines the current room combination scenario based on the state of the partition sensors
        /// </summary>
        private void DetermineRoomCombinationScenario()
        {
            if (_scenarioChangeDebounceTimer != null)
            {
                _scenarioChangeDebounceTimer.Dispose();
                _scenarioChangeDebounceTimer = null;
            }

            this.LogInformation("Determining Combination Scenario");

            var currentScenario = RoomCombinationScenarios.FirstOrDefault((s) =>
            {
                this.LogDebug("Checking scenario {scenarioKey}", s.Key);
                // iterate the partition states
                foreach (var partitionState in s.PartitionStates)
                {
                    this.LogDebug("checking PartitionState {partitionStateKey}", partitionState.PartitionKey);
                    // get the partition by key
                    var partition = Partitions.FirstOrDefault((p) => p.Key.Equals(partitionState.PartitionKey));

                    this.LogDebug("Expected State: {partitionPresent} Actual State: {partitionState}", partitionState.PartitionPresent, partition.PartitionPresentFeedback.BoolValue);

                    if (partition != null && partitionState.PartitionPresent != partition.PartitionPresentFeedback.BoolValue)
                    {
                        // the partition can't be found or the state doesn't match
                        return false;
                    }
                }
                // if it hasn't returned false by now we have the matching scenario
                return true;
            });

            if (currentScenario != null)
            {
                this.LogInformation("Found combination Scenario {scenarioKey}", currentScenario.Key);
                ChangeScenario(currentScenario);
            }
        }

        private async Task ChangeScenario(IRoomCombinationScenario newScenario)
        {
            

                if (newScenario == _currentScenario)
                {
                    return;
                }

                // Deactivate the old scenario first
                if (_currentScenario != null)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Deactivating scenario {currentScenario}", this, _currentScenario.Name);
                    await _currentScenario.Deactivate();
                }

                _currentScenario = newScenario;

                // Activate the new scenario
                if (_currentScenario != null)
                {
                    Debug.LogMessage(LogEventLevel.Debug, $"Current Scenario: {_currentScenario.Name}", this);
                    await _currentScenario.Activate();
                }

                RoomCombinationScenarioChanged?.Invoke(this, new EventArgs());

            
        }

        #region IEssentialsRoomCombiner Members

        public event EventHandler<EventArgs> RoomCombinationScenarioChanged;

        public IRoomCombinationScenario CurrentScenario
        {
            get
            {
                return _currentScenario;
            }
        }

        public BoolFeedback IsInAutoModeFeedback { get; private set; }

        public void SetAutoMode()
        {
            IsInAutoMode = true;

            foreach (var partition in Partitions)
            {
                partition.SetAutoMode();
            }

            DetermineRoomCombinationScenario();
        }

        public void SetManualMode()
        {
            IsInAutoMode = false;

            foreach (var partition in Partitions)
            {
                partition.SetManualMode();
            }
        }

        public void ToggleMode()
        {
            if (IsInAutoMode)
            {
                SetManualMode();
            }
            else
            {
                SetAutoMode();
            }
        }

        public List<IRoomCombinationScenario> RoomCombinationScenarios { get; private set; }

        public List<IPartitionController> Partitions { get; private set; }

        public void TogglePartitionState(string partitionKey)
        {
            var partition = Partitions.FirstOrDefault((p) => p.Key.Equals(partitionKey));

            if (partition != null)
            {
                partition.ToggglePartitionState();
            }
        }

        public void SetRoomCombinationScenario(string scenarioKey)
        {
            if (IsInAutoMode)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Cannot set room combination scenario when in auto mode.  Set to auto mode first.");
                return;
            }

            // Get the scenario
            var scenario = RoomCombinationScenarios.FirstOrDefault((s) => s.Key.Equals(scenarioKey));


            // Set the parition states from the scenario manually
            if (scenario != null)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Manually setting scenario to '{0}'", scenario.Key);
                foreach (var partitionState in scenario.PartitionStates)
                {
                    var partition = Partitions.FirstOrDefault((p) => p.Key.Equals(partitionState.PartitionKey));

                    if (partition != null)
                    {
                        if (partitionState.PartitionPresent)
                        {
                            Debug.LogMessage(LogEventLevel.Information, this, "Manually setting state to Present for: '{0}'", partition.Key);
                            partition.SetPartitionStatePresent();
                        }
                        else
                        {
                            Debug.LogMessage(LogEventLevel.Information, this, "Manually setting state to Not Present for: '{0}'", partition.Key);
                            partition.SetPartitionStateNotPresent();
                        }
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Debug, this, "Unable to find partition with key: '{0}'", partitionState.PartitionKey);
                    }
                }
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Unable to find scenario with key: '{0}'", scenarioKey);
            }
        }

        #endregion
    }

    public class EssentialsRoomCombinerFactory : EssentialsDeviceFactory<EssentialsRoomCombiner>
    {
        public EssentialsRoomCombinerFactory()
        {
            TypeNames = new List<string> { "essentialsroomcombiner" };
        }

        public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new EssentialsRoomCombiner Device");

            var props = dc.Properties.ToObject<EssentialsRoomCombinerPropertiesConfig>();

            return new EssentialsRoomCombiner(dc.Key, props);
        }
    }
}
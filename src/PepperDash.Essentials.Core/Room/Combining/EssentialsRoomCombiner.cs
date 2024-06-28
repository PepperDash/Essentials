using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;

using PepperDash.Core;
using Serilog.Events;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
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
                if(value == _isInAutoMode)
                {
                    return;
                }

                _isInAutoMode = value;
                IsInAutoModeFeedback.FireUpdate();
            }
        }

        private CTimer _scenarioChangeDebounceTimer;

        private int _scenarioChangeDebounceTimeSeconds = 10; // default to 10s

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

                if (IsInAutoMode)
                {
                    DetermineRoomCombinationScenario();
                }
                else
                {
                    SetRoomCombinationScenario(_propertiesConfig.defaultScenarioKey);
                }
            });
        }

        void CreateScenarios()
        {
            foreach (var scenarioConfig in _propertiesConfig.Scenarios)
            {
                var scenario = new RoomCombinationScenario(scenarioConfig);
                RoomCombinationScenarios.Add(scenario);
            }
        }

        void SetRooms()
        {
            _rooms = new List<IEssentialsRoom>();

            foreach (var roomKey in _propertiesConfig.RoomKeys)
            {
                var room = DeviceManager.GetDeviceForKey(roomKey) as IEssentialsRoom;
                if (room != null)
                {
                    _rooms.Add(room);
                }
            }
        }

        void SetupPartitionStateProviders()
        {
            foreach (var pConfig in _propertiesConfig.Partitions)
            {
                var sensor = DeviceManager.GetDeviceForKey(pConfig.DeviceKey) as IPartitionStateProvider;

                var partition = new EssentialsPartitionController(pConfig.Key, pConfig.Name, sensor, _propertiesConfig.defaultToManualMode, pConfig.AdjacentRoomKeys);

                partition.PartitionPresentFeedback.OutputChange += PartitionPresentFeedback_OutputChange;

                Partitions.Add(partition);
            }
        }

        void PartitionPresentFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            if (!IsInAutoMode) return;

            StartDebounceTimer();
        }

        void StartDebounceTimer()
        {
            // default to 500ms for manual mode
            var time = 500;

            // if in auto mode, debounce the scenario change
            if(IsInAutoMode) time = _scenarioChangeDebounceTimeSeconds * 1000;

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
        void DetermineRoomCombinationScenario()
        {
            if (_scenarioChangeDebounceTimer != null)
            {
                _scenarioChangeDebounceTimer.Dispose();
                _scenarioChangeDebounceTimer = null;
            }

            var currentScenario = RoomCombinationScenarios.FirstOrDefault((s) =>
            {
                // iterate the partition states
                foreach (var partitionState in s.PartitionStates)
                {
                    // get the partition by key
                    var partition = Partitions.FirstOrDefault((p) => p.Key.Equals(partitionState.PartitionKey));

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
                CurrentScenario = currentScenario;
            }
        }

        #region IEssentialsRoomCombiner Members

        public event EventHandler<EventArgs> RoomCombinationScenarioChanged;

        public IRoomCombinationScenario CurrentScenario
        {
            get
            {
                return _currentScenario;
            }
            private set
            {
                if (value != _currentScenario)
                {
                    // Deactivate the old scenario first
                    if (_currentScenario != null)
                    {
                        _currentScenario.Deactivate();
                    }

                    _currentScenario = value;

                    // Activate the new scenario
                    if (_currentScenario != null)
                    {
                        _currentScenario.Activate();

                        Debug.LogMessage(LogEventLevel.Debug, $"Current Scenario: {_currentScenario.Name}", this);
                    }

                    var handler = RoomCombinationScenarioChanged;
                    if (handler != null)
                    {
                        handler(this, new EventArgs());
                    }
                }
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
            if(IsInAutoMode)
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
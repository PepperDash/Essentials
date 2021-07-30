using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public class EssentialsRoomCombiner : EssentialsDevice, IEssentialsRoomCombiner
    {
        private EssentialsRoomCombinerPropertiesConfig _propertiesConfig;

        private IRoomCombinationScenario _currentScenario;

        private List<IEssentialsRoom> _rooms;

        private bool isInAutoMode;

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

            IsInAutoModeFeedback = new BoolFeedback(() => isInAutoMode);

            // default to auto mode
            isInAutoMode = true;

            if (_propertiesConfig.defaultToManualMode)
            {
                isInAutoMode = false;
            }

            IsInAutoModeFeedback.FireUpdate();

            CreateScenarios();

            AddPostActivationAction(() =>
            {
                SetupPartitionStateProviders();

                SetRooms();

                if (isInAutoMode)
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
            StartDebounceTimer();
        }

        void StartDebounceTimer()
        {
            var time = _scenarioChangeDebounceTimeSeconds * 1000;

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
                    _currentScenario = value;
                    Debug.Console(1, this, "Current Scenario: {0}", _currentScenario.Name);
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
            isInAutoMode = true;
            IsInAutoModeFeedback.FireUpdate();
        }

        public void SetManualMode()
        {
            isInAutoMode = false;
            IsInAutoModeFeedback.FireUpdate();
        }

        public void ToggleMode()
        {
            isInAutoMode = !isInAutoMode;
            IsInAutoModeFeedback.FireUpdate();
        }

        public List<IRoomCombinationScenario> RoomCombinationScenarios { get; private set; }

        public List<IPartitionController> Partitions { get; private set; }

        public void TogglePartitionState(string partitionKey)
        {
            var partition = Partitions.FirstOrDefault((p) => p.Key.Equals(partitionKey)) as IPartitionController;

            if (partition != null)
            {
                partition.ToggglePartitionState();
            }
        }

        public void SetRoomCombinationScenario(string scenarioKey)
        {
            if (isInAutoMode)
            {
                Debug.Console(0, this, "Cannot set room combination scenario when in auto mode.  Set to auto mode first.");
                return;
            }

            // Get the scenario
            var scenario = RoomCombinationScenarios.FirstOrDefault((s) => s.Key.Equals(scenarioKey));


            // Set the parition states from the scenario manually
            if (scenario != null)
            {
                Debug.Console(0, this, "Manually setting scenario to '{0}'", scenario.Key);
                foreach (var partitionState in scenario.PartitionStates)
                {
                    var partition = Partitions.FirstOrDefault((p) => p.Key.Equals(partitionState.PartitionKey));

                    if (partition != null)
                    {
                        if (partitionState.PartitionPresent)
                        {
                            Debug.Console(0, this, "Manually setting state to Present for: '{0}'", partition.Key);
                            partition.SetPartitionStatePresent();
                        }
                        else
                        {
                            Debug.Console(0, this, "Manually setting state to Not Present for: '{0}'", partition.Key);
                            partition.SetPartitionStateNotPresent();
                        }
                    }
                    else
                    {
                        Debug.Console(1, this, "Unable to find partition with key: '{0}'", partitionState.PartitionKey);
                    }
                }
            }
            else
            {
                Debug.Console(1, this, "Unable to find scenario with key: '{0}'", scenarioKey);
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
            Debug.Console(1, "Factory Attempting to create new EssentialsRoomCombiner Device");

            var props = dc.Properties.ToObject<EssentialsRoomCombinerPropertiesConfig>();

            return new EssentialsRoomCombiner(dc.Key, props);
        }
    }
}
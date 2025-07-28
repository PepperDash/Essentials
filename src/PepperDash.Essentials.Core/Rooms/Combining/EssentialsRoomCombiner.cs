using Crestron.SimplSharp;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.PartitionSensor;
using PepperDash.Essentials.Core.Room.Combining;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Rooms.Combining
{
    /// <summary>
    /// Represents a device that manages room combinations by controlling partitions and scenarios.
    /// </summary>
    /// <remarks>The <see cref="EssentialsRoomCombiner"/> allows for dynamic configuration of room
    /// combinations  based on partition states and predefined scenarios. It supports both automatic and manual modes 
    /// for managing room combinations. In automatic mode, the device determines the current room  combination scenario
    /// based on partition sensor states. In manual mode, scenarios can be set  explicitly by the user.</remarks>
    public class EssentialsRoomCombiner : EssentialsDevice, IEssentialsRoomCombiner
    {
        private EssentialsRoomCombinerPropertiesConfig _propertiesConfig;

        private IRoomCombinationScenario _currentScenario;

        private List<IEssentialsRoom> _rooms;

        /// <summary>
        /// Gets a list of rooms represented as key-name pairs.
        /// </summary>
        public List<IKeyName> Rooms
        {
            get
            {
                return _rooms.Cast<IKeyName>().ToList();
            }
        }

        private bool _isInAutoMode;

        /// <summary>
        /// Gets or sets a value indicating whether the system is operating in automatic mode.
        /// </summary>
        /// <remarks>Changing this property triggers an update event via
        /// <c>IsInAutoModeFeedback.FireUpdate()</c>. Ensure that any event listeners are properly configured to handle
        /// this update.</remarks>
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

        /// <summary>
        /// Gets a value indicating whether automatic mode is disabled.
        /// </summary>
        public bool DisableAutoMode
        {
            get
            {
                return _propertiesConfig.DisableAutoMode;
            }
        }

        private CTimer _scenarioChangeDebounceTimer;

        private int _scenarioChangeDebounceTimeSeconds = 10; // default to 10s

        private Mutex _scenarioChange = new Mutex();

        /// <summary>
        /// Initializes a new instance of the <see cref="EssentialsRoomCombiner"/> class, which manages room combination
        /// scenarios and partition states.
        /// </summary>
        /// <remarks>The <see cref="EssentialsRoomCombiner"/> class is designed to handle dynamic room
        /// combination scenarios based on partition states. It supports both automatic and manual modes for managing
        /// room combinations. By default, the instance starts in automatic mode unless the <paramref name="props"/>
        /// specifies otherwise.  After activation, the room combiner initializes partition state providers and sets up
        /// the initial room configuration. Additionally, it subscribes to the <see
        /// cref="DeviceManager.AllDevicesInitialized"/> event to ensure proper initialization of dependent devices
        /// before determining or setting the room combination scenario.</remarks>
        /// <param name="key">The unique identifier for the room combiner instance.</param>
        /// <param name="props">The configuration properties for the room combiner, including default settings and debounce times.</param>
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

        /// <summary>
        /// Occurs when the room combination scenario changes.
        /// </summary>
        /// <remarks>This event is triggered whenever the configuration or state of the room combination
        /// changes. Subscribers can use this event to update their logic or UI based on the new scenario.</remarks>
        public event EventHandler<EventArgs> RoomCombinationScenarioChanged;

        /// <summary>
        /// Gets the current room combination scenario.
        /// </summary>
        public IRoomCombinationScenario CurrentScenario
        {
            get
            {
                return _currentScenario;
            }
        }

        /// <summary>
        /// Gets or sets the IsInAutoModeFeedback
        /// </summary>
        public BoolFeedback IsInAutoModeFeedback { get; private set; }

        /// <summary>
        /// Enables auto mode for the room combiner and its partitions, allowing automatic room combination scenarios to
        /// be determined.
        /// </summary>
        /// <remarks>Auto mode allows the room combiner to automatically adjust its configuration based on
        /// the state of its partitions.  If auto mode is disabled in the configuration, this method logs a warning and
        /// does not enable auto mode.</remarks>
        public void SetAutoMode()
        {
            if(_propertiesConfig.DisableAutoMode)
            {
                this.LogWarning("Auto mode is disabled for this room combiner. Cannot set to auto mode.");
                return;
            }
            IsInAutoMode = true;

            foreach (var partition in Partitions)
            {
                partition.SetAutoMode();
            }

            DetermineRoomCombinationScenario();
        }

        /// <summary>
        /// Switches the system to manual mode, disabling automatic operations.
        /// </summary>
        /// <remarks>This method sets the system to manual mode by updating the mode state and propagates 
        /// the change to all partitions. Once in manual mode, automatic operations are disabled  for the system and its
        /// partitions.</remarks>
        /// <summary>
        /// SetManualMode method
        /// </summary>
        public void SetManualMode()
        {
            IsInAutoMode = false;

            foreach (var partition in Partitions)
            {
                partition.SetManualMode();
            }
        }

        /// <summary>
        /// Toggles the current mode between automatic and manual.
        /// </summary>
        /// <remarks>If the current mode is automatic, this method switches to manual mode.  If the
        /// current mode is manual, it switches to automatic mode.</remarks>
        /// <summary>
        /// ToggleMode method
        /// </summary>
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

        /// <summary>
        /// Gets or sets the RoomCombinationScenarios
        /// </summary>
        public List<IRoomCombinationScenario> RoomCombinationScenarios { get; private set; }

        /// <summary>
        /// Gets the collection of partition controllers managed by this instance.
        /// </summary>
        public List<IPartitionController> Partitions { get; private set; }

        /// <summary>
        /// Toggles the state of the partition identified by the specified partition key.
        /// </summary>
        /// <remarks>If no partition with the specified key exists, the method performs no
        /// action.</remarks>
        /// <param name="partitionKey">The key of the partition whose state is to be toggled. This value cannot be null or empty.</param>
        public void TogglePartitionState(string partitionKey)
        {
            var partition = Partitions.FirstOrDefault((p) => p.Key.Equals(partitionKey));

            if (partition != null)
            {
                partition.ToggglePartitionState();
            }
        }

        /// <summary>
        /// Sets the room combination scenario based on the specified scenario key.
        /// </summary>
        /// <remarks>This method manually adjusts the partition states according to the specified
        /// scenario. If the application is in auto mode,  the operation will not proceed, and a log message will be
        /// generated indicating that the mode must be set to manual first.  If the specified scenario key does not
        /// match any existing scenario, a debug log message will be generated. For each partition state in the
        /// scenario, the corresponding partition will be updated to either "Present" or "Not Present"  based on the
        /// scenario's configuration. If a partition key in the scenario cannot be found, a debug log message will be
        /// generated.</remarks>
        /// <param name="scenarioKey">The key identifying the room combination scenario to apply. This must match the key of an existing scenario.</param>
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

    /// <summary>
    /// Provides a factory for creating instances of <see cref="EssentialsRoomCombiner"/> devices.
    /// </summary>
    /// <remarks>This factory is responsible for constructing <see cref="EssentialsRoomCombiner"/> devices
    /// based on the provided configuration. It supports the type name "essentialsroomcombiner" for device
    /// creation.</remarks>
    /// <summary>
    /// Represents a EssentialsRoomCombinerFactory
    /// </summary>
    public class EssentialsRoomCombinerFactory : EssentialsDeviceFactory<EssentialsRoomCombiner>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EssentialsRoomCombinerFactory"/> class.
        /// </summary>
        /// <remarks>This factory is used to create instances of room combiners with the specified type
        /// names. By default, the factory includes the type name "essentialsroomcombiner".</remarks>
        public EssentialsRoomCombinerFactory()
        {
            TypeNames = new List<string> { "essentialsroomcombiner" };
        }

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="EssentialsRoomCombiner"/> device.
        /// </summary>
        /// <remarks>This method uses the provided device configuration to extract the properties and
        /// create an  <see cref="EssentialsRoomCombiner"/> device. Ensure that the configuration contains valid 
        /// properties for the device to be created successfully.</remarks>
        /// <param name="dc">The device configuration containing the key and properties required to build the device.</param>
        /// <returns>A new instance of <see cref="EssentialsRoomCombiner"/> initialized with the specified configuration.</returns>
        public override EssentialsDevice BuildDevice(Core.Config.DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new EssentialsRoomCombiner Device");

            var props = dc.Properties.ToObject<EssentialsRoomCombinerPropertiesConfig>();

            return new EssentialsRoomCombiner(dc.Key, props);
        }
    }
}
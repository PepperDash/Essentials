using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    public class EssentialsRoomCombiner : EssentialsDevice, IEssentialsRoomCombiner
    {
        private EssentialsRoomCombinerPropertiesConfig _propertiesConfig;

        private IRoomCombinationScenario _currentScenario;

        private List<IEssentialsRoom> _rooms;

        private bool isInAutoMode;

        public EssentialsRoomCombiner(string key, EssentialsRoomCombinerPropertiesConfig props)
            : base(key)
        {
            _propertiesConfig = props;

            IsInAutoModeFeedback = new BoolFeedback(() => isInAutoMode);

            // default to auto mode
            isInAutoMode = true;

            if (_propertiesConfig.defaultToManualMode)
            {
                isInAutoMode = false;
            }

            IsInAutoModeFeedback.FireUpdate();

            CreateScenarios();

            SetupPartitionStateProviders();

            SetRooms();
        }

        void CreateScenarios()
        {
            RoomCombinationScenarios = new List<IRoomCombinationScenario>();

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
            DetermineRoomCombinationScenario();
        }


        /// <summary>
        /// Determines the current room combination scenario based on the state of the partition sensors
        /// </summary>
        void DetermineRoomCombinationScenario()
        {
            //RoomCombinationScenarios.FirstOrDefault((s) =>
            //{
            //    foreach (var partitionState in s.PartitionStates)
            //    {
            //        var partition = Partitions.FirstOrDefault(
            //    }
            //});
        }

        #region IEssentialsRoomCombiner Members

        public event EventHandler<EventArgs> RoomCombinationScenarioChanged;

        public IRoomCombinationScenario CurrentScenario
        {
            get
            {
                return _currentScenario;
            }
            set
            {
                if (value != _currentScenario)
                {
                    _currentScenario = value;
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

        public List<IPartitionStateProvider> Partitions { get; private set; }

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

        }

        #endregion
    }
}
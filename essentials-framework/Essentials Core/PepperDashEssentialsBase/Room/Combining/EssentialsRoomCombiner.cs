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
            foreach (var scenarioConfig in _propertiesConfig.Scenarios)
            {
                var scenario = new RoomCombinationScenario(scenarioConfig);
            }
        }

        void SetRooms()
        {
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

        public List<IPartitionStateProvider> PartitionStateProviders { get; private set; }

        public void TogglePartitionState(string partitionKey)
        {
            var partition = PartitionStateProviders.FirstOrDefault((p) => p.Key.Equals(partitionKey)) as IManualPartitionSensor;

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
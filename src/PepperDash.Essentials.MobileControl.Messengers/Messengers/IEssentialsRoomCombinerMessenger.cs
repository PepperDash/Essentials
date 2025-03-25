using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IEssentialsRoomCombinerMessenger : MessengerBase
    {
        private readonly IEssentialsRoomCombiner _roomCombiner;

        public IEssentialsRoomCombinerMessenger(string key, string messagePath, IEssentialsRoomCombiner roomCombiner)
            : base(key, messagePath, roomCombiner as Device)
        {
            _roomCombiner = roomCombiner;
        }

        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendFullStatus());

            AddAction("/setAutoMode", (id, content) =>
            {
                _roomCombiner.SetAutoMode();
            });

            AddAction("/setManualMode", (id, content) =>
            {
                _roomCombiner.SetManualMode();
            });

            AddAction("/toggleMode", (id, content) =>
            {
                _roomCombiner.ToggleMode();
            });

            AddAction("/togglePartitionState", (id, content) =>
            {
                try
                {
                    var partitionKey = content.ToObject<string>();

                    _roomCombiner.TogglePartitionState(partitionKey);
                }
                catch (Exception e)
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Error toggling partition state: {e}", this);
                }
            });

            AddAction("/setRoomCombinationScenario", (id, content) =>
            {
                try
                {
                    var scenarioKey = content.ToObject<string>();

                    _roomCombiner.SetRoomCombinationScenario(scenarioKey);
                }
                catch (Exception e)
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Error toggling partition state: {e}", this);
                }
            });

            _roomCombiner.RoomCombinationScenarioChanged += (sender, args) =>
            {
                SendFullStatus();
            };

            _roomCombiner.IsInAutoModeFeedback.OutputChange += (sender, args) =>
            {
                var message = new
                {
                    isInAutoMode = _roomCombiner.IsInAutoModeFeedback.BoolValue
                };

                PostStatusMessage(JToken.FromObject(message));
            };

            foreach(var partition in _roomCombiner.Partitions)
            {
                partition.PartitionPresentFeedback.OutputChange += (sender, args) =>
                {
                    var message = new
                    {
                        partitions = _roomCombiner.Partitions
                    };

                    PostStatusMessage(JToken.FromObject(message));
                };
            }
        }

        private void SendFullStatus()
        {
            try
            {
                var rooms = new List<IKeyName>();

                foreach (var room in _roomCombiner.Rooms)
                {
                    rooms.Add(new RoomCombinerRoom{ Key = room.Key, Name = room.Name });
                }

                var message = new IEssentialsRoomCombinerStateMessage
                {
                    IsInAutoMode = _roomCombiner.IsInAutoMode,
                    CurrentScenario = _roomCombiner.CurrentScenario,
                    Rooms = rooms,
                    RoomCombinationScenarios = _roomCombiner.RoomCombinationScenarios,
                    Partitions = _roomCombiner.Partitions
                };

                PostStatusMessage(message);
            }
            catch (Exception e)
            {
                Debug.Console(0, this, "Error sending full status: {0}", e);
            }
        }

        private class RoomCombinerRoom : IKeyName
        {
            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }

    public class IEssentialsRoomCombinerStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("isInAutoMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsInAutoMode { get; set; }

        [JsonProperty("currentScenario", NullValueHandling = NullValueHandling.Ignore)]
        public IRoomCombinationScenario CurrentScenario { get; set; }

        [JsonProperty("rooms", NullValueHandling = NullValueHandling.Ignore)]
        public List<IKeyName> Rooms { get; set; }

        [JsonProperty("roomCombinationScenarios", NullValueHandling = NullValueHandling.Ignore)]
        public List<IRoomCombinationScenario> RoomCombinationScenarios { get; set; }

        [JsonProperty("partitions", NullValueHandling = NullValueHandling.Ignore)]
        public List<IPartitionController> Partitions { get; set; }
    }


}

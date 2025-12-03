using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging functionality for managing room combination scenarios and partition states in an  <see
    /// cref="IEssentialsRoomCombiner"/> instance. Enables external systems to interact with the room combiner  via
    /// predefined actions and status updates.
    /// </summary>
    /// <remarks>This class facilitates communication with an <see cref="IEssentialsRoomCombiner"/> by
    /// exposing actions  for toggling modes, managing partitions, and setting room combination scenarios. It also
    /// listens for  feedback changes and broadcasts status updates to connected systems.   Typical usage involves
    /// registering actions for external commands and handling feedback events to  synchronize state changes.</remarks>
    public class IEssentialsRoomCombinerMessenger : MessengerBase
    {
        private readonly IEssentialsRoomCombiner _roomCombiner;

        /// <summary>
        /// Initializes a new instance of the <see cref="IEssentialsRoomCombinerMessenger"/> class,  which facilitates
        /// messaging for an <see cref="IEssentialsRoomCombiner"/> instance.
        /// </summary>
        /// <remarks>This class is designed to enable communication and interaction with an <see
        /// cref="IEssentialsRoomCombiner"/>  through the specified messaging path. Ensure that the <paramref
        /// name="roomCombiner"/> parameter is not null  when creating an instance.</remarks>
        /// <param name="key">The unique key identifying this messenger instance.</param>
        /// <param name="messagePath">The path used for messaging operations.</param>
        /// <param name="roomCombiner">The <see cref="IEssentialsRoomCombiner"/> instance associated with this messenger.</param>
        public IEssentialsRoomCombinerMessenger(string key, string messagePath, IEssentialsRoomCombiner roomCombiner)
            : base(key, messagePath, roomCombiner as IKeyName)
        {
            _roomCombiner = roomCombiner;
        }

        /// <summary>
        /// Registers actions and event handlers for managing room combination scenarios and partition states.
        /// </summary>
        /// <remarks>This method sets up various actions that can be triggered via specific endpoints,
        /// such as toggling modes,  setting room combination scenarios, and managing partition states. It also
        /// subscribes to feedback events  to update the status when changes occur in room combination scenarios or
        /// partition states.</remarks>
        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/combinerStatus", (id, content) => SendFullStatus(id));

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

            foreach (var partition in _roomCombiner.Partitions)
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

        private void SendFullStatus(string id = null)
        {
            try
            {
                var rooms = new List<IKeyName>();

                foreach (var room in _roomCombiner.Rooms)
                {
                    rooms.Add(new RoomCombinerRoom { Key = room.Key, Name = room.Name });
                }

                var message = new IEssentialsRoomCombinerStateMessage
                {
                    DisableAutoMode = _roomCombiner.DisableAutoMode,
                    IsInAutoMode = _roomCombiner.IsInAutoMode,
                    CurrentScenario = _roomCombiner.CurrentScenario,
                    Rooms = rooms,
                    RoomCombinationScenarios = _roomCombiner.RoomCombinationScenarios,
                    Partitions = _roomCombiner.Partitions
                };

                PostStatusMessage(message, id);
            }
            catch (Exception e)
            {
                this.LogException(e, "Error sending full status");
            }
        }

        private class RoomCombinerRoom : IKeyName
        {
            [JsonProperty("key")]
            /// <summary>
            /// Gets or sets the Key
            /// </summary>
            public string Key { get; set; }

            [JsonProperty("name")]
            /// <summary>
            /// Gets or sets the Name
            /// </summary>
            public string Name { get; set; }
        }
    }

    /// <summary>
    /// Represents the state message for a room combiner system, providing information about the current configuration, 
    /// operational mode, and associated rooms, partitions, and scenarios.
    /// </summary>
    /// <remarks>This class is used to encapsulate the state of a room combiner system, including its current
    /// mode of operation,  active room combination scenario, and the list of rooms and partitions involved. It is
    /// typically serialized  and transmitted to communicate the state of the system.</remarks>
    public class IEssentialsRoomCombinerStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatic mode is disabled.
        /// </summary>
        [JsonProperty("disableAutoMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool DisableAutoMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the system is operating in automatic mode.
        /// </summary>
        [JsonProperty("isInAutoMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsInAutoMode { get; set; }

        /// <summary>
        /// Gets or sets the current room combination scenario.
        /// </summary>
        [JsonProperty("currentScenario", NullValueHandling = NullValueHandling.Ignore)]
        public IRoomCombinationScenario CurrentScenario { get; set; }

        /// <summary>
        /// Gets or sets the collection of rooms associated with the entity.
        /// </summary>
        [JsonProperty("rooms", NullValueHandling = NullValueHandling.Ignore)]
        public List<IKeyName> Rooms { get; set; }

        /// <summary>
        /// Gets or sets the collection of room combination scenarios.
        /// </summary>
        [JsonProperty("roomCombinationScenarios", NullValueHandling = NullValueHandling.Ignore)]
        public List<IRoomCombinationScenario> RoomCombinationScenarios { get; set; }

        /// <summary>
        /// Gets or sets the collection of partition controllers.
        /// </summary>
        [JsonProperty("partitions", NullValueHandling = NullValueHandling.Ignore)]
        public List<IPartitionController> Partitions { get; set; }
    }


}

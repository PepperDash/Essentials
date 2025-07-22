using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement ILevelControls interface
    /// </summary>
    public class ILevelControlsMessenger : MessengerBase
    {
        private ILevelControls levelControlsDevice;

        /// <summary>
        /// Initializes a new instance of the ILevelControlsMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements ILevelControls</param>
        public ILevelControlsMessenger(string key, string messagePath, ILevelControls device) : base(key, messagePath, device as IKeyName)
        {
            levelControlsDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, context) =>
            {
                var message = new LevelControlStateMessage
                {
                    Levels = levelControlsDevice.LevelControlPoints.ToDictionary(kv => kv.Key, kv => new Volume { Level = kv.Value.VolumeLevelFeedback.IntValue, Muted = kv.Value.MuteFeedback.BoolValue })
                };

                PostStatusMessage(message, id);
            });

            foreach (var levelControl in levelControlsDevice.LevelControlPoints)
            {
                // reassigning here just in case of lambda closure issues
                var key = levelControl.Key;
                var control = levelControl.Value;

                AddAction($"/{key}/level", (id, content) =>
                {
                    var request = content.ToObject<MobileControlSimpleContent<ushort>>();

                    control.SetVolume(request.Value);
                });

                AddAction($"/{key}/muteToggle", (id, content) =>
                {
                    control.MuteToggle();
                });

                AddAction($"/{key}/muteOn", (id, content) => control.MuteOn());

                AddAction($"/{key}/muteOff", (id, content) => control.MuteOff());

                AddAction($"/{key}/volumeUp", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => control.VolumeUp(b)));

                AddAction($"/{key}/volumeDown", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => control.VolumeDown(b)));

                control.VolumeLevelFeedback.OutputChange += (o, a) => PostStatusMessage(JToken.FromObject(new
                {
                    levelControls = new Dictionary<string, Volume>
                    {
                        {key, new Volume{Level = a.IntValue} }
                    }
                }));

                control.MuteFeedback.OutputChange += (o, a) => PostStatusMessage(JToken.FromObject(new
                {
                    levelControls = new Dictionary<string, Volume>
                    {
                        {key, new Volume{Muted = a.BoolValue} }
                    }
                }));
            }
        }
    }

    /// <summary>
    /// State message for level controls
    /// </summary>
    public class LevelControlStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the level controls
        /// </summary>
        [JsonProperty("levelControls")]
        public Dictionary<string, Volume> Levels { get; set; }
    }

    /// <summary>
    /// Request message for level control operations
    /// </summary>
    public class LevelControlRequestMessage
    {
        /// <summary>
        /// Gets or sets the control key
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the level
        /// </summary>
        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public ushort? Level { get; set; }
    }
}

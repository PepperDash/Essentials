using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a ILevelControlsMessenger
    /// </summary>
    public class ILevelControlsMessenger : MessengerBase
    {
        private ILevelControls levelControlsDevice;

        public ILevelControlsMessenger(string key, string messagePath, ILevelControls device) : base(key, messagePath, device as IKeyName)
        {
            levelControlsDevice = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, context) => SendFullStatus(id));

            AddAction("/levelStats", (id, content) => SendFullStatus(id));

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

        private void SendFullStatus(string id = null)
        {
            var message = new LevelControlStateMessage
            {
                Levels = levelControlsDevice.LevelControlPoints.ToDictionary(kv => kv.Key, kv => new Volume { Level = kv.Value.VolumeLevelFeedback.IntValue, Muted = kv.Value.MuteFeedback.BoolValue })
            };

            PostStatusMessage(message, id);
        }
    }

    /// <summary>
    /// Represents a LevelControlStateMessage
    /// </summary>
    public class LevelControlStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("levelControls")]
        public Dictionary<string, Volume> Levels { get; set; }
    }

    /// <summary>
    /// Represents a LevelControlRequestMessage
    /// </summary>
    public class LevelControlRequestMessage
    {
        [JsonProperty("key")]
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; set; }

        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public ushort? Level { get; set; }
    }
}

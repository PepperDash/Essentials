using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Lighting;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for lighting scenes devices
    /// </summary>
    public class ILightingScenesMessenger : MessengerBase
    {
        private ILightingScenes lightingScenesDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="ILightingScenesMessenger"/> class.
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="device">Device that implements ILightingScenes</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <exception cref="ArgumentNullException">Thrown when device is null</exception>
        public ILightingScenesMessenger(string key, ILightingScenes device, string messagePath)
            : base(key, messagePath, device as IKeyName)
        {
            lightingScenesDevice = device ?? throw new ArgumentNullException("device");

            lightingScenesDevice.LightingSceneChange += new EventHandler<LightingSceneChangeEventArgs>(LightingDevice_LightingSceneChange);
        }

        private void LightingDevice_LightingSceneChange(object sender, LightingSceneChangeEventArgs e)
        {
            var state = new LightingBaseStateMessage
            {
                CurrentLightingScene = e.CurrentLightingScene
            };

            PostStatusMessage(state);
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/selectScene", (id, content) =>
            {
                var s = content.ToObject<LightingScene>();
                lightingScenesDevice.SelectScene(s);
            });

            if (!(lightingScenesDevice is ILightingScenesDynamic lightingScenesDynamic))
                return;

            lightingScenesDynamic.LightingScenesUpdated += (s, e) => SendFullStatus();
        }


        private void SendFullStatus(string id = null)
        {
            var state = new LightingBaseStateMessage
            {
                Scenes = lightingScenesDevice.LightingScenes,
                CurrentLightingScene = lightingScenesDevice.CurrentLightingScene
            };

            PostStatusMessage(state, id);
        }
    }

    public class LightingBaseStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("scenes", NullValueHandling = NullValueHandling.Ignore)]
        public List<LightingScene> Scenes { get; set; }

        [JsonProperty("currentLightingScene", NullValueHandling = NullValueHandling.Ignore)]
        public LightingScene CurrentLightingScene { get; set; }
    }
}
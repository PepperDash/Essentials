using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Lighting;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a ILightingScenesMessenger
    /// </summary>
    public class ILightingScenesMessenger : MessengerBase
    {
        private ILightingScenes lightingScenesDevice;

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

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/lightingStatus", (id, content) => SendFullStatus(id));

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

    /// <summary>
    /// Represents a LightingBaseStateMessage
    /// </summary>
    public class LightingBaseStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the Scenes
        /// </summary>
        [JsonProperty("scenes", NullValueHandling = NullValueHandling.Ignore)]
        public List<LightingScene> Scenes { get; set; }


        /// <summary>
        /// Gets or sets the CurrentLightingScene
        /// </summary>
        [JsonProperty("currentLightingScene", NullValueHandling = NullValueHandling.Ignore)]
        public LightingScene CurrentLightingScene { get; set; }
    }
}
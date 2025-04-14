using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Lighting;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
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

            AddAction("/fullStatus", (id, content) => SendFullStatus());

            AddAction("/selectScene", (id, content) =>
            {
                var s = content.ToObject<LightingScene>();
                lightingScenesDevice.SelectScene(s);
            });

            if(!(lightingScenesDevice is ILightingScenesDynamic lightingScenesDynamic))
                return;

            lightingScenesDynamic.LightingScenesUpdated += (s, e) => SendFullStatus();
        }


        private void SendFullStatus()
        {
            var state = new LightingBaseStateMessage
            {
                Scenes = lightingScenesDevice.LightingScenes,
                CurrentLightingScene = lightingScenesDevice.CurrentLightingScene
            };

            PostStatusMessage(state);
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
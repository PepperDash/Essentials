using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Lighting;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class ILightingScenesMessenger : MessengerBase
    {
        protected ILightingScenes Device { get; private set; }

        public ILightingScenesMessenger(string key, ILightingScenes device, string messagePath)
            : base(key, messagePath, device as IKeyName)
        {
            Device = device ?? throw new ArgumentNullException("device");
            Device.LightingSceneChange += new EventHandler<LightingSceneChangeEventArgs>(LightingDevice_LightingSceneChange);
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
                Device.SelectScene(s);
            });

            if(!(Device is ILightingScenesDynamic lightingScenesDynamic))
                return;

            lightingScenesDynamic.LightingScenesUpdated += (s, e) => SendFullStatus();
        }


        private void SendFullStatus()
        {
            var state = new LightingBaseStateMessage
            {
                Scenes = Device.LightingScenes,
                CurrentLightingScene = Device.CurrentLightingScene
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
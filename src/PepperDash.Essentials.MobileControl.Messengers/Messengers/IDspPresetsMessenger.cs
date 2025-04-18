﻿using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IDspPresetsMessenger : MessengerBase
    {
        private readonly IDspPresets device;

        public IDspPresetsMessenger(string key, string messagePath, IDspPresets device)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) =>
            {
                var message = new IHasDspPresetsStateMessage
                {
                    Presets = device.Presets
                };

                PostStatusMessage(message);
            });

            AddAction("/recallPreset", (id, content) =>
            {
                var presetKey = content.ToObject<string>();


                if (!string.IsNullOrEmpty(presetKey))
                {
                    device.RecallPreset(presetKey);
                }
            });
        }
    }

    public class IHasDspPresetsStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("presets")]
        public Dictionary<string, IKeyName> Presets { get; set; }
    }
}

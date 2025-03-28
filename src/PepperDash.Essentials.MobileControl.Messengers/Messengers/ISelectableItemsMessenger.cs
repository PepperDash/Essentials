﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class ISelectableItemsMessenger<TKey> : MessengerBase
    {
        private static readonly JsonSerializer serializer = new JsonSerializer { Converters = { new StringEnumConverter() } };
        private readonly ISelectableItems<TKey> itemDevice;

        private readonly string _propName;
        public ISelectableItemsMessenger(string key, string messagePath, ISelectableItems<TKey> device, string propName) : base(key, messagePath, device as IKeyName)
        {
            itemDevice = device;
            _propName = propName;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, context) =>
            {
                SendFullStatus();
            });

            itemDevice.ItemsUpdated += (sender, args) =>
            {
                SendFullStatus();
            };

            itemDevice.CurrentItemChanged += (sender, args) =>
            {
                SendFullStatus();
            };

            foreach (var input in itemDevice.Items)
            {
                var key = input.Key;
                var localItem = input.Value;

                AddAction($"/{key}", (id, content) =>
                {
                    localItem.Select();
                });

                localItem.ItemUpdated += (sender, args) =>
                {
                    SendFullStatus();
                };
            }
        }

        private void SendFullStatus()
        {
            var stateObject = new JObject();
            stateObject[_propName] = JToken.FromObject(itemDevice, serializer);
            PostStatusMessage(stateObject);
        }
    }

}

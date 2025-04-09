using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class ISelectableItemsMessenger<TKey> : MessengerBase
    {        
        private readonly ISelectableItems<TKey> itemDevice;

        private readonly string _propName;

        /// <summary>
        /// Constructs a messenger for a device that implements ISelectableItems<typeparamref name="TKey"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        /// <param name="propName"></param>
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
            try
            {
                this.LogInformation("Sending full status");

                var stateObject = new ISelectableItemsStateMessage<TKey>
                {
                    Items = itemDevice.Items,
                    CurrentItem = itemDevice.CurrentItem
                };

                PostStatusMessage(stateObject);
            }
            catch (Exception e)
            {
                this.LogError("Error sending full status: {0}", e.Message);
            }
        }
    }

    public class ISelectableItemsStateMessage<TKey> : DeviceStateMessageBase
    {
        [JsonProperty("items")]
        public Dictionary<TKey, ISelectableItem> Items { get; set; }

        [JsonProperty("currentItem")]
        public TKey CurrentItem { get; set; }
    }

}

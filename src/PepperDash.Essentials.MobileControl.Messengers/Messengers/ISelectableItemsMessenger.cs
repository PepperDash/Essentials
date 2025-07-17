using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement ISelectableItems interface
    /// </summary>
    /// <typeparam name="TKey">Type of the key used for selectable items</typeparam>
    public class ISelectableItemsMessenger<TKey> : MessengerBase
    {
        private readonly ISelectableItems<TKey> itemDevice;

        private readonly string _propName;

        /// <summary>
        /// Constructs a messenger for a device that implements ISelectableItems<typeparamref name="TKey"/>
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements ISelectableItems</param>
        /// <param name="propName">Property name for JSON serialization</param>
        public ISelectableItemsMessenger(string key, string messagePath, ISelectableItems<TKey> device, string propName) : base(key, messagePath, device as IKeyName)
        {
            itemDevice = device;
            _propName = propName;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, context) =>
            {
                SendFullStatus(id);
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

        private void SendFullStatus(string id = null)
        {
            try
            {
                this.LogInformation("Sending full status");

                var stateObject = new ISelectableItemsStateMessage<TKey>
                {
                    Items = itemDevice.Items,
                    CurrentItem = itemDevice.CurrentItem
                };

                PostStatusMessage(stateObject, id);
            }
            catch (Exception e)
            {
                this.LogError("Error sending full status: {0}", e.Message);
            }
        }
    }

    /// <summary>
    /// State message for selectable items
    /// </summary>
    /// <typeparam name="TKey">Type of the key used for selectable items</typeparam>
    public class ISelectableItemsStateMessage<TKey> : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the items dictionary
        /// </summary>
        [JsonProperty("items")]
        public Dictionary<TKey, ISelectableItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the current item
        /// </summary>
        [JsonProperty("currentItem")]
        public TKey CurrentItem { get; set; }
    }

}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a ISelectableItemsMessenger
    /// </summary>
    public class ISelectableItemsMessenger<TKey> : MessengerBase
    {
        private readonly ISelectableItems<TKey> itemDevice;

        private readonly string _propName;

        private List<string> _itemKeys = new List<string>();

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
                SendFullStatus(id)
            );

            AddAction("/itemsStatus", (id, content) => SendFullStatus(id));

            AddAction("/selectItem", (id, content) =>
            {
                try
                {
                    var key = content.ToObject<TKey>();

                    if (key == null)
                    {
                        this.LogError("No key specified to select");
                        return;
                    }
                    if (itemDevice.Items.ContainsKey((TKey)Convert.ChangeType(key, typeof(TKey))))
                    {
                        itemDevice.Items[(TKey)Convert.ChangeType(key, typeof(TKey))].Select();
                    }
                    else
                    {
                        this.LogError("Key {0} not found in items", key);
                    }
                }
                catch (Exception e)
                {
                    this.LogError("Error selecting item: {0}", e.Message);
                }
            });

            itemDevice.ItemsUpdated += (sender, args) =>
            {
                SetItems();
            };

            itemDevice.CurrentItemChanged += (sender, args) =>
            {
                SendFullStatus();
            };

            SetItems();
        }

        /// <summary>
        /// Sets the items and registers their update events
        /// </summary>
        private void SetItems()
        {
            if (_itemKeys != null && _itemKeys.Count > 0)
            {
                /// Clear out any existing item actions
                foreach (var item in _itemKeys)
                {
                    RemoveAction($"/{item}");
                }

                _itemKeys.Clear();
            }

            foreach (var item in itemDevice.Items)
            {
                var key = item.Key;
                var localItem = item.Value;

                AddAction($"/{key}", (id, content) =>
                {
                    localItem.Select();
                });

                _itemKeys.Add(key.ToString());

                localItem.ItemUpdated -= LocalItem_ItemUpdated;
                localItem.ItemUpdated += LocalItem_ItemUpdated;
            }
        }

        private void LocalItem_ItemUpdated(object sender, EventArgs e)
        {
            SendFullStatus();
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
    /// Represents a ISelectableItemsStateMessage
    /// </summary>
    public class ISelectableItemsStateMessage<TKey> : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the Items
        /// </summary>
        [JsonProperty("items")]
        public Dictionary<TKey, ISelectableItem> Items { get; set; }


        /// <summary>
        /// Gets or sets the CurrentItem
        /// </summary>
        [JsonProperty("currentItem")]
        public TKey CurrentItem { get; set; }
    }

}

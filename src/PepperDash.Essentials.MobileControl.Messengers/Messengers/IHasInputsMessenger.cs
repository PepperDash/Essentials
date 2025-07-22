using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement IHasInputs interface
    /// </summary>
    /// <typeparam name="TKey">Type of the key used for inputs</typeparam>
    public class IHasInputsMessenger<TKey> : MessengerBase
    {
        private readonly IHasInputs<TKey> itemDevice;


        /// <summary>
        /// Constructs a messenger for a device that implements IHasInputs<typeparamref name="TKey"/>
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements IHasInputs</param>
        public IHasInputsMessenger(string key, string messagePath, IHasInputs<TKey> device) : base(key, messagePath, device)
        {
            itemDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, context) =>
            {
                SendFullStatus(id);
            });

            itemDevice.Inputs.ItemsUpdated += (sender, args) =>
            {
                SendFullStatus();
            };

            itemDevice.Inputs.CurrentItemChanged += (sender, args) =>
            {
                SendFullStatus();
            };

            foreach (var input in itemDevice.Inputs.Items)
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

                var stateObject = new IHasInputsStateMessage<TKey>
                {
                    Inputs = new Inputs<TKey>
                    {
                        Items = itemDevice.Inputs.Items,
                        CurrentItem = itemDevice.Inputs.CurrentItem
                    }
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
    /// State message for devices with inputs
    /// </summary>
    /// <typeparam name="TKey">Type of the key used for inputs</typeparam>
    public class IHasInputsStateMessage<TKey> : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the inputs
        /// </summary>
        [JsonProperty("inputs")]
        public Inputs<TKey> Inputs { get; set; }
    }

    /// <summary>
    /// Represents a collection of inputs
    /// </summary>
    /// <typeparam name="TKey">Type of the key used for inputs</typeparam>
    public class Inputs<TKey>
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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IHasInputsMessenger
    /// </summary>
    public class IHasInputsMessenger<TKey> : MessengerBase
    {
        private readonly IHasInputs<TKey> itemDevice;


        /// <summary>
        /// Constructs a messenger for a device that implements IHasInputs<typeparamref name="TKey"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        public IHasInputsMessenger(string key, string messagePath, IHasInputs<TKey> device) : base(key, messagePath, device)
        {
            itemDevice = device;
        }


        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, context) => SendFullStatus(id));

            AddAction("/inputStatus", (id, content) => SendFullStatus(id));

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
    /// Represents a IHasInputsStateMessage
    /// </summary>
    public class IHasInputsStateMessage<TKey> : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the Inputs
        /// </summary>
        [JsonProperty("inputs")]
        public Inputs<TKey> Inputs { get; set; }
    }

    /// <summary>
    /// Represents a Inputs
    /// </summary>
    public class Inputs<TKey>
    {
        /// <summary>
        /// Gets or sets the Items
        /// The key is the input key, and the value is the input name
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
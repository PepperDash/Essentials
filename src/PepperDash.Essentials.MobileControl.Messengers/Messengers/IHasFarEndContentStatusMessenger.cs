using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IHasFarEndContentStatus"/>
    /// </summary>
    public class IHasFarEndContentStatusMessenger : MessengerBase
    {
        private readonly IHasFarEndContentStatus device;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasFarEndContentStatusMessenger"/> class.
        /// </summary>
        public IHasFarEndContentStatusMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            this.device = device as IHasFarEndContentStatus ?? throw new ArgumentException("device must implement IHasFarEndContentStatus", nameof(device));
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/farEndContentStatus", (id, content) => SendFullStatus(id));

            device.ReceivingContent.OutputChange += (sender, args) => PostReceivingContent(args.BoolValue);
        }

        private void PostReceivingContent(bool receivingContent)
        {
            try
            {
                PostStatusMessage(new IHasFarEndContentStatusStateMessage
                {
                    ReceivingContent = receivingContent
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting receiving content");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasFarEndContentStatusStateMessage
                {
                    ReceivingContent = device.ReceivingContent.BoolValue
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting full status");
            }
        }
    }

    /// <summary>
    /// Message class representing the state of a device implementing <see cref="IHasFarEndContentStatus"/>
    /// </summary>
    public class IHasFarEndContentStatusStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Indicates whether the device is currently receiving content from the far end
        /// </summary>
        [JsonProperty("receivingContent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReceivingContent { get; set; }
    }
}

using System;
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
        private readonly IHasFarEndContentStatus _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasFarEndContentStatusMessenger"/> class.
        /// </summary>
        public IHasFarEndContentStatusMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _device = device as IHasFarEndContentStatus ?? throw new ArgumentException("device must implement IHasFarEndContentStatus", nameof(device));
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            _device.ReceivingContent.OutputChange += (sender, args) => PostReceivingContent(args.BoolValue);
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
    }

    public class IHasFarEndContentStatusStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("receivingContent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReceivingContent { get; set; }
    }
}

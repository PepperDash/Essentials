using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement IHasCurrentSourceInfoChange interface
    /// </summary>
    public class IHasCurrentSourceInfoMessenger : MessengerBase
    {
        private readonly IHasCurrentSourceInfoChange sourceDevice;

        /// <summary>
        /// Initializes a new instance of the IHasCurrentSourceInfoMessenger class
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="messagePath">Path for message routing</param>
        /// <param name="device">Device that implements IHasCurrentSourceInfoChange</param>
        public IHasCurrentSourceInfoMessenger(string key, string messagePath, IHasCurrentSourceInfoChange device) : base(key, messagePath, device as IKeyName)
        {
            sourceDevice = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) =>
            {
                var message = new CurrentSourceStateMessage
                {
                    CurrentSourceKey = sourceDevice.CurrentSourceInfoKey,
                    CurrentSource = sourceDevice.CurrentSourceInfo
                };

                PostStatusMessage(message);
            });

            sourceDevice.CurrentSourceChange += (sender, e) =>
            {
                switch (e)
                {
                    case ChangeType.DidChange:
                        {
                            PostStatusMessage(JToken.FromObject(new
                            {
                                currentSourceKey = string.IsNullOrEmpty(sourceDevice.CurrentSourceInfoKey) ? string.Empty : sourceDevice.CurrentSourceInfoKey,
                                currentSource = sourceDevice.CurrentSourceInfo
                            }));
                            break;
                        }
                }
            };
        }
    }

    /// <summary>
    /// State message for current source information
    /// </summary>
    public class CurrentSourceStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the current source key
        /// </summary>
        [JsonProperty("currentSourceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentSourceKey { get; set; }

        /// <summary>
        /// Gets or sets the current source information
        /// </summary>
        [JsonProperty("currentSource")]
        public SourceListItem CurrentSource { get; set; }
    }
}

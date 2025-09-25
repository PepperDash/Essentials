using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IHasCurrentSourceInfoMessenger
    /// </summary>
    public class IHasCurrentSourceInfoMessenger : MessengerBase
    {
        private readonly IHasCurrentSourceInfoChange sourceDevice;

        public IHasCurrentSourceInfoMessenger(string key, string messagePath, IHasCurrentSourceInfoChange device) : base(key, messagePath, device as IKeyName)
        {
            sourceDevice = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/currentSourceInfoStatus", (id, content) => SendFullStatus(id));

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

        private void SendFullStatus(string id = null)
        {
            var message = new CurrentSourceStateMessage
            {
                CurrentSourceKey = sourceDevice.CurrentSourceInfoKey,
                CurrentSource = sourceDevice.CurrentSourceInfo
            };

            PostStatusMessage(message, id);
        }
    }

    /// <summary>
    /// Represents a CurrentSourceStateMessage
    /// </summary>
    public class CurrentSourceStateMessage : DeviceStateMessageBase
    {

        /// <summary>
        /// Gets or sets the CurrentSourceKey
        /// </summary>
        [JsonProperty("currentSourceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentSourceKey { get; set; }


        /// <summary>
        /// Gets or sets the CurrentSource
        /// </summary>
        [JsonProperty("currentSource")]
        public SourceListItem CurrentSource { get; set; }
    }
}

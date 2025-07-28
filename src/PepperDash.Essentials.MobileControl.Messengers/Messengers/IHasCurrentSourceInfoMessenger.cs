using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Routing;

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
    /// Represents a CurrentSourceStateMessage
    /// </summary>
    public class CurrentSourceStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("currentSourceKey", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the CurrentSourceKey
        /// </summary>
        public string CurrentSourceKey { get; set; }

        [JsonProperty("currentSource")]
        /// <summary>
        /// Gets or sets the CurrentSource
        /// </summary>
        public SourceListItem CurrentSource { get; set; }
    }
}

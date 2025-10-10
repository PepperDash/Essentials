using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a IHasPowerControlWithFeedbackMessenger
    /// </summary>
    public class IHasPowerControlWithFeedbackMessenger : MessengerBase
    {
        private readonly IHasPowerControlWithFeedback _powerControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasPowerControlWithFeedbackMessenger"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="messagePath">The message path.</param>
        /// <param name="powerControl">The power control device</param>
        public IHasPowerControlWithFeedbackMessenger(string key, string messagePath, IHasPowerControlWithFeedback powerControl)
            : base(key, messagePath, powerControl as IKeyName)
        {
            _powerControl = powerControl;
        }

        /// <summary>
        /// SendFullStatus method
        /// </summary>
        public void SendFullStatus(string id = null)
        {
            var messageObj = new PowerControlWithFeedbackStateMessage
            {
                PowerState = _powerControl.PowerIsOnFeedback.BoolValue
            };

            PostStatusMessage(messageObj, id);
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/powerStatus", (id, content) => SendFullStatus(id));

            _powerControl.PowerIsOnFeedback.OutputChange += PowerIsOnFeedback_OutputChange; ;
        }

        private void PowerIsOnFeedback_OutputChange(object sender, FeedbackEventArgs args)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                powerState = args.BoolValue
            })
            );
        }
    }

    /// <summary>
    /// Represents a PowerControlWithFeedbackStateMessage
    /// </summary>
    public class PowerControlWithFeedbackStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Power State
        /// </summary>
        [JsonProperty("powerState", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PowerState { get; set; }
    }
}

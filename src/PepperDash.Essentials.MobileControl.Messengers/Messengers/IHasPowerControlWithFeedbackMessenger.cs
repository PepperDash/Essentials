using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for power control operations with feedback.
    /// Handles power on/off commands and power state feedback reporting.
    /// </summary>
    public class IHasPowerControlWithFeedbackMessenger : MessengerBase
    {
        private readonly IHasPowerControlWithFeedback _powerControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasPowerControlWithFeedbackMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="messagePath">The message path for power control messages.</param>
        /// <param name="powerControl">The device that provides power control functionality.</param>
        public IHasPowerControlWithFeedbackMessenger(string key, string messagePath, IHasPowerControlWithFeedback powerControl)
            : base(key, messagePath, powerControl as IKeyName)
        {
            _powerControl = powerControl;
        }

        /// <summary>
        /// Sends the full power control status to connected clients.
        /// </summary>
        public void SendFullStatus(string id = null)
        {
            var messageObj = new PowerControlWithFeedbackStateMessage
            {
                PowerState = _powerControl.PowerIsOnFeedback.BoolValue
            };

            PostStatusMessage(messageObj, id);
        }

        /// <summary>
        /// Registers actions for handling power control operations.
        /// Includes power on, power off, power toggle, and full status reporting.
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

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
    /// Represents a power control state message containing power state information.
    /// </summary>
    public class PowerControlWithFeedbackStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the power state of the device.
        /// </summary>
        [JsonProperty("powerState", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PowerState { get; set; }
    }
}

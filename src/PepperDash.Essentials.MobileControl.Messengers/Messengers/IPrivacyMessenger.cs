using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IPrivacy"/>
    /// </summary>
    public class IPrivacyMessenger : MessengerBase
    {
        private readonly IPrivacy _privacy;

        /// <summary>
        /// Initializes a new instance of the <see cref="IPrivacyMessenger"/> class.
        /// </summary>
        /// <param name="key">The key for the messenger.</param>
        /// <param name="messagePath">The message path for the messenger.</param>
        /// <param name="device">The device implementing <see cref="IPrivacy"/>.</param>
        public IPrivacyMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _privacy = device as IPrivacy ?? throw new ArgumentNullException(nameof(device));
            _privacy.PrivacyModeIsOnFeedback.OutputChange += PrivacyModeIsOnFeedback_OutputChange;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
            AddAction("/privacyStatus", (id, content) => SendFullStatus(id));

            AddAction("/privacyModeOn", (id, content) => _privacy.PrivacyModeOn());
            AddAction("/privacyModeOff", (id, content) => _privacy.PrivacyModeOff());
            AddAction("/privacyModeToggle", (id, content) => _privacy.PrivacyModeToggle());
        }

        private void PrivacyModeIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            try
            {
                PostStatusMessage(new IPrivacyStateMessage
                {
                    PrivacyModeIsOn = e.BoolValue
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting privacy mode state");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IPrivacyStateMessage
                {
                    PrivacyModeIsOn = _privacy.PrivacyModeIsOnFeedback.BoolValue
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending privacy full status");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IPrivacy"/>
    /// </summary>
    public class IPrivacyStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether privacy mode is on. Null if unknown or not
        /// applicable.
        /// </summary>
        [JsonProperty("privacyModeIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrivacyModeIsOn { get; set; }
    }
}

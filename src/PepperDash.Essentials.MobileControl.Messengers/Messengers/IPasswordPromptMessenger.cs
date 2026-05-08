using System;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IPasswordPrompt"/>
    /// </summary>
    public class IPasswordPromptMessenger : MessengerBase
    {
        private readonly IPasswordPrompt _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="IPasswordPromptMessenger"/> class.
        /// </summary>
        public IPasswordPromptMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _device = device as IPasswordPrompt ?? throw new ArgumentException("device must implement IPasswordPrompt", nameof(device));
            _device.PasswordRequired += OnPasswordRequired;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/password", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                _device.SubmitPassword(msg.Value);
            });
        }

        private void OnPasswordRequired(object sender, PasswordPromptEventArgs args)
        {
            PostEventMessage(new PasswordPromptEventMessage
            {
                Message = args.Message,
                LastAttemptWasIncorrect = args.LastAttemptWasIncorrect,
                LoginAttemptFailed = args.LoginAttemptFailed,
                LoginAttemptCancelled = args.LoginAttemptCancelled,
                EventType = "passwordPrompt"
            });
        }
    }

    /// <summary>
    /// Base event message for video codec events
    /// </summary>
    public class VideoCodecBaseEventMessage : DeviceEventMessageBase
    {
    }

    /// <summary>
    /// Event message sent when a password is required
    /// </summary>
    public class PasswordPromptEventMessage : VideoCodecBaseEventMessage
    {
        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the LastAttemptWasIncorrect
        /// </summary>
        [JsonProperty("lastAttemptWasIncorrect", NullValueHandling = NullValueHandling.Ignore)]
        public bool LastAttemptWasIncorrect { get; set; }

        /// <summary>
        /// Gets or sets the LoginAttemptFailed
        /// </summary>
        [JsonProperty("loginAttemptFailed", NullValueHandling = NullValueHandling.Ignore)]
        public bool LoginAttemptFailed { get; set; }

        /// <summary>
        /// Gets or sets the LoginAttemptCancelled
        /// </summary>
        [JsonProperty("loginAttemptCancelled", NullValueHandling = NullValueHandling.Ignore)]
        public bool LoginAttemptCancelled { get; set; }
    }
}

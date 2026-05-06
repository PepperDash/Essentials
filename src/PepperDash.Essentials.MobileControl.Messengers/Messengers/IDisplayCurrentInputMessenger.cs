using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common.Displays;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a messenger for a display device that has current input information.
    /// </summary>
    public class IDisplayCurrentInputMessenger : MessengerBase
    {
        private readonly IDisplayCurrentInput _display;

        /// <summary>
        /// Initializes a new instance of the <see cref="IDisplayCurrentInputMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="display"></param>
        public IDisplayCurrentInputMessenger(string key, string messagePath, IDisplayCurrentInput display)
            : base(key, messagePath, display as IKeyName)
        {
            _display = display;
        }

        #region Overrides of MessengerBase

        /// <summary>
        /// SendFullStatus method
        /// </summary>
        public void SendFullStatus(string id = null)
        {
            var messageObj = new CurrentInputStateMessage
            {
                CurrentInput = _display.CurrentInputFeedback.StringValue
            };

            PostStatusMessage(messageObj, id);
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/currentInputStatus", (id, content) => SendFullStatus(id));

            _display.CurrentInputFeedback.OutputChange += CurrentInputFeedbackOnOutputChange;
        }

        private void CurrentInputFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                currentInput = feedbackEventArgs.StringValue
            })
            );


        }

        #endregion
    }

    /// <summary>
    /// Represents a TwoWayDisplayBaseStateMessage
    /// </summary>
    public class CurrentInputStateMessage : DeviceStateMessageBase
    {
        //[JsonProperty("powerState", NullValueHandling = NullValueHandling.Ignore)]
        //public bool? PowerState { get; set; }


        /// <summary>
        /// Gets or sets the CurrentInput
        /// </summary>
        [JsonProperty("currentInput", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentInput { get; set; }
    }
}
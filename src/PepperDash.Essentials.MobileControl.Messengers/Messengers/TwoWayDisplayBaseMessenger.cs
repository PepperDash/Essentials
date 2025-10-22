using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Displays;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a TwoWayDisplayBaseMessenger
    /// </summary>
    public class TwoWayDisplayBaseMessenger : MessengerBase
    {
        private readonly TwoWayDisplayBase _display;

        public TwoWayDisplayBaseMessenger(string key, string messagePath, TwoWayDisplayBase display)
            : base(key, messagePath, display)
        {
            _display = display;
        }

        #region Overrides of MessengerBase

        /// <summary>
        /// SendFullStatus method
        /// </summary>
        public void SendFullStatus(string id = null)
        {
            var messageObj = new TwoWayDisplayBaseStateMessage
            {
                //PowerState = _display.PowerIsOnFeedback.BoolValue,
                CurrentInput = _display.CurrentInputFeedback.StringValue
            };

            PostStatusMessage(messageObj, id);
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/displayStatus", (id, content) => SendFullStatus(id));

            _display.CurrentInputFeedback.OutputChange += CurrentInputFeedbackOnOutputChange;
            _display.IsCoolingDownFeedback.OutputChange += IsCoolingFeedbackOnOutputChange;
            _display.IsWarmingUpFeedback.OutputChange += IsWarmingFeedbackOnOutputChange;
        }

        private void CurrentInputFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                currentInput = feedbackEventArgs.StringValue
            })
            );
        }

        private void IsWarmingFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                isWarming = feedbackEventArgs.BoolValue
            })
            );
        }

        private void IsCoolingFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                isCooling = feedbackEventArgs.BoolValue
            })
            );


        }

        #endregion
    }

    /// <summary>
    /// Represents a TwoWayDisplayBaseStateMessage
    /// </summary>
    public class TwoWayDisplayBaseStateMessage : DeviceStateMessageBase
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Displays;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for two-way display control operations.
    /// Handles display input changes, power state, and cooling/warming status.
    /// This class extends the MessengerBase to facilitate communication between the display and the mobile control interface.
    /// </summary>
    public class TwoWayDisplayBaseMessenger : MessengerBase
    {
        private readonly TwoWayDisplayBase _display;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoWayDisplayBaseMessenger"/> class.
        /// This constructor sets up the messenger with a key, message path, and the display instance
        /// that it will control and monitor.
        /// The display instance should implement the TwoWayDisplayBase interface to provide the necessary feedback and
        /// control methods for the display device.
        /// The messenger will listen for changes in the display's state and send updates to the mobile control interface.
        /// It also allows for sending commands to the display, such as changing inputs or toggling power states.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="messagePath">The message path for display control messages.</param>
        /// <param name="display">The display instance to control and monitor.</param>
        public TwoWayDisplayBaseMessenger(string key, string messagePath, TwoWayDisplayBase display)
            : base(key, messagePath, display)
        {
            _display = display;
        }

        #region Overrides of MessengerBase


        private void SendFullStatus(string id = null)
        {
            var messageObj = new TwoWayDisplayBaseStateMessage
            {
                //PowerState = _display.PowerIsOnFeedback.BoolValue,
                CurrentInput = _display.CurrentInputFeedback.StringValue
            };

            PostStatusMessage(messageObj, id);
        }

        /// <summary>
        /// Registers actions for handling two-way display operations.
        /// This includes sending full status updates and handling input changes, cooling, and warming feedback.
        /// The actions are registered to respond to specific commands sent from the mobile control interface.
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

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

    public class TwoWayDisplayBaseStateMessage : DeviceStateMessageBase
    {
        //[JsonProperty("powerState", NullValueHandling = NullValueHandling.Ignore)]
        //public bool? PowerState { get; set; }

        [JsonProperty("currentInput", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentInput { get; set; }
    }
}
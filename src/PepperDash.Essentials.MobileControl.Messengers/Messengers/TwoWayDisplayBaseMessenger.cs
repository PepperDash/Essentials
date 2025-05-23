﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Displays;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class TwoWayDisplayBaseMessenger : MessengerBase
    {
        private readonly TwoWayDisplayBase _display;

        public TwoWayDisplayBaseMessenger(string key, string messagePath, TwoWayDisplayBase display)
            : base(key, messagePath, display)
        {
            _display = display;
        }

        #region Overrides of MessengerBase

        public void SendFullStatus()
        {
            var messageObj = new TwoWayDisplayBaseStateMessage
            {
                //PowerState = _display.PowerIsOnFeedback.BoolValue,
                CurrentInput = _display.CurrentInputFeedback.StringValue
            };

            PostStatusMessage(messageObj);
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

            //_display.PowerIsOnFeedback.OutputChange += PowerIsOnFeedbackOnOutputChange;
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


        //private void PowerIsOnFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        //{
        //    PostStatusMessage(JToken.FromObject(new
        //        {
        //            powerState = feedbackEventArgs.BoolValue
        //        })
        //    );
        //}

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
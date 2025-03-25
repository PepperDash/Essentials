using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class CoreTwoWayDisplayBaseMessenger : MessengerBase
    {
        private readonly TwoWayDisplayBase _display;

        public CoreTwoWayDisplayBaseMessenger(string key, string messagePath, Device display)
            : base(key, messagePath, display)
        {
            _display = display as TwoWayDisplayBase;
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

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            base.RegisterActions();
            if (_display == null)
            {
                Debug.Console(0, this, $"Unable to register TwoWayDisplayBase messenger {Key}");
                return;
            }

            AddAction("/fullStatus", (id, content) => SendFullStatus());
        
            _display.PowerIsOnFeedback.OutputChange += PowerIsOnFeedbackOnOutputChange;
            _display.CurrentInputFeedback.OutputChange += CurrentInputFeedbackOnOutputChange;
            _display.IsCoolingDownFeedback.OutputChange += IsCoolingFeedbackOnOutputChange;
            _display.IsWarmingUpFeedback.OutputChange += IsWarmingFeedbackOnOutputChange;
        }

        private void CurrentInputFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            PostStatusMessage(JToken.FromObject(new
                {
                    currentInput = feedbackEventArgs.StringValue
                }));
        }


        private void PowerIsOnFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            PostStatusMessage(JToken.FromObject(new
                {
                    powerState = feedbackEventArgs.BoolValue
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
}

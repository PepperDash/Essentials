using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Shades;
using System;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IShadesOpenCloseStopMessenger : MessengerBase
    {
        private readonly IShadesOpenCloseStop device;

        public IShadesOpenCloseStopMessenger(string key, IShadesOpenCloseStop shades, string messagePath)
            : base(key, messagePath, shades as Device)
        {
            device = shades;
        }

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

            AddAction("/shadeUp", (id, content) =>
                {

                    device.Open();

                });

            AddAction("/shadeDown", (id, content) =>
                {

                    device.Close();

                });

            var stopDevice = device;
            if (stopDevice != null)
            {
                AddAction("/stopOrPreset", (id, content) =>
                {
                    stopDevice.Stop();
                });
            }

            if (device is IShadesOpenClosedFeedback feedbackDevice)
            {
                feedbackDevice.ShadeIsOpenFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>(ShadeIsOpenFeedback_OutputChange);
                feedbackDevice.ShadeIsClosedFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>(ShadeIsClosedFeedback_OutputChange);
            }
        }

        private void ShadeIsOpenFeedback_OutputChange(object sender, Core.FeedbackEventArgs e)
        {
            var state = new ShadeBaseStateMessage
            {
                IsOpen = e.BoolValue
            };

            PostStatusMessage(state);
        }

        private void ShadeIsClosedFeedback_OutputChange(object sender, Core.FeedbackEventArgs e)
        {
            var state = new ShadeBaseStateMessage
            {
                IsClosed = e.BoolValue
            };

            PostStatusMessage(state);
        }


        private void SendFullStatus()
        {
            var state = new ShadeBaseStateMessage();

            if (device is IShadesOpenClosedFeedback feedbackDevice)
            {
                state.IsOpen = feedbackDevice.ShadeIsOpenFeedback.BoolValue;
                state.IsClosed = feedbackDevice.ShadeIsClosedFeedback.BoolValue;
            }

            PostStatusMessage(state);
        }
    }

    public class ShadeBaseStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("middleButtonLabel", NullValueHandling = NullValueHandling.Ignore)]
        public string MiddleButtonLabel { get; set; }

        [JsonProperty("isOpen", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOpen { get; set; }

        [JsonProperty("isClosed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsClosed { get; set; }
    }
}
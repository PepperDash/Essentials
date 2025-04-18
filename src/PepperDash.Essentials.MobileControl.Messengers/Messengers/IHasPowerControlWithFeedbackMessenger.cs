﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class IHasPowerControlWithFeedbackMessenger : MessengerBase
    {
        private readonly IHasPowerControlWithFeedback _powerControl;

        public IHasPowerControlWithFeedbackMessenger(string key, string messagePath, IHasPowerControlWithFeedback powerControl)
            : base(key, messagePath, powerControl as IKeyName)
        {
            _powerControl = powerControl;
        }

        public void SendFullStatus()
        {
            var messageObj = new PowerControlWithFeedbackStateMessage
            {
                PowerState = _powerControl.PowerIsOnFeedback.BoolValue
            };

            PostStatusMessage(messageObj);
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

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

    public class PowerControlWithFeedbackStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("powerState", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PowerState { get; set; }
    }
}

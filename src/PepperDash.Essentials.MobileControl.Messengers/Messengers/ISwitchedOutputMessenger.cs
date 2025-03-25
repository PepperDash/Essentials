using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.Shades;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class ISwitchedOutputMessenger : MessengerBase
    {

        private readonly ISwitchedOutput device;

        public ISwitchedOutputMessenger(string key, ISwitchedOutput device, string messagePath)
            : base(key, messagePath, device as Device)
        {
            this.device = device;
        }

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

            AddAction("/on", (id, content) =>
            {

                device.On();

            });

            AddAction("/off", (id, content) =>
            {

                device.Off();

            });

            device.OutputIsOnFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>((o, a) => SendFullStatus());       
        }

        private void SendFullStatus()
        {
            var state = new ISwitchedOutputStateMessage
            {
                IsOn = device.OutputIsOnFeedback.BoolValue
            };

            PostStatusMessage(state);
        }
    }

    public class ISwitchedOutputStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("isOn")]
        public bool IsOn { get; set; }
    }
}

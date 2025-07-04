using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a ISwitchedOutputMessenger
    /// </summary>
    public class ISwitchedOutputMessenger : MessengerBase
    {

        private readonly ISwitchedOutput device;

        /// <summary>
        /// Initializes a new instance of the <see cref="ISwitchedOutputMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="device"></param>
        /// <param name="messagePath"></param>
        public ISwitchedOutputMessenger(string key, ISwitchedOutput device, string messagePath)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }


        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/switchedOutputStatus", (id, content) => SendFullStatus(id));

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

        private void SendFullStatus(string id = null)
        {
            var state = new ISwitchedOutputStateMessage
            {
                IsOn = device.OutputIsOnFeedback.BoolValue
            };

            PostStatusMessage(state, id);
        }
    }

    /// <summary>
    /// Represents a ISwitchedOutputStateMessage
    /// </summary>
    public class ISwitchedOutputStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the IsOn
        /// </summary>
        [JsonProperty("isOn")]
        public bool IsOn { get; set; }
    }
}
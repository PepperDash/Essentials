using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for ISwitchedOutput devices
    /// </summary>
    public class ISwitchedOutputMessenger : MessengerBase
    {

        private readonly ISwitchedOutput device;

        /// <summary>
        /// Initializes a new instance of the <see cref="ISwitchedOutputMessenger"/> class.
        /// This messenger provides mobile control interface for switched output devices.
        /// It allows sending commands to turn the output on or off, and provides feedback on the
        /// current state of the output.
        /// </summary>
        /// <param name="key">Unique identifier for the messenger</param>
        /// <param name="device">Device that implements ISwitchedOutput</param>
        /// <param name="messagePath">Path for message routing</param>
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
    /// State message for ISwitchedOutput devices
    /// This message contains the current state of the switched output, specifically whether it is on or off.
    /// It is used to communicate the state of the output to clients that subscribe to this messenger.
    /// </summary>
    public class ISwitchedOutputStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the switched output is currently on.
        /// This property is used to convey the current state of the output to clients.
        /// A value of true indicates that the output is on, while false indicates it is off.
        /// </summary>
        [JsonProperty("isOn")]
        public bool IsOn { get; set; }
    }
}

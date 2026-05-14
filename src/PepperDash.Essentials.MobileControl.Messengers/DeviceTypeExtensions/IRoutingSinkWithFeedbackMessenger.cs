using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using System.Linq;
using DisplayBase = PepperDash.Essentials.Devices.Common.Displays.DisplayBase;

namespace PepperDash.Essentials.Room.MobileControl
{
    /// <summary>
    /// Represents a DisplayBaseMessenger
    /// </summary>
    public class IRoutingSinkWithFeedbackMessenger : MessengerBase
    {
        private readonly IRoutingSinkWithFeedback display;

        /// <summary>
        /// Create an instance of the <see cref="IRoutingSinkWithFeedbackMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        public IRoutingSinkWithFeedbackMessenger(string key, string messagePath, IRoutingSinkWithFeedback device) : base(key, messagePath, device)
        {
            display = device;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            /*AddAction("/powerOn", (id, content) => display.PowerOn());
            AddAction("/powerOff", (id, content) => display.PowerOff());
            AddAction("/powerToggle", (id, content) => display.PowerToggle());*/

            AddAction("/inputSelect", (id, content) =>
            {
                var s = content.ToObject<MobileControlSimpleContent<string>>();

                var inputPort = display.InputPorts.FirstOrDefault(i => i.Key == s.Value);

                if (inputPort == null)
                {
                    this.LogWarning("No input named {inputName} found for {deviceKey}", s, display.Key);
                    return;
                }

                display.ExecuteSwitch(inputPort.Selector);
            });

            AddAction("/inputs", (id, content) =>
            {
                var inputsList = display.InputPorts.Select(p => p.Key).ToList();

                var messageObject = new MobileControlMessage
                {
                    Type = MessagePath + "/inputs",
                    Content = JToken.FromObject(new
                    {
                        inputKeys = inputsList,
                    })
                };

                AppServerController.SendMessageObject(messageObject);
            });
        }
    }
}
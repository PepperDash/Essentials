using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using System.Linq;
using DisplayBase = PepperDash.Essentials.Devices.Common.Displays.DisplayBase;

namespace PepperDash.Essentials.Room.MobileControl
{
    public class DisplayBaseMessenger : MessengerBase
    {
        private readonly DisplayBase display;

        public DisplayBaseMessenger(string key, string messagePath, DisplayBase device) : base(key, messagePath, device)
        {
            display = device;
        }

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

using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Linq;
using DisplayBase = PepperDash.Essentials.Core.DisplayBase;

namespace PepperDash.Essentials.Room.MobileControl
{
    public class CoreDisplayBaseMessenger: MessengerBase
    {
        private readonly DisplayBase display;

        public CoreDisplayBaseMessenger(string key, string messagePath, DisplayBase device) : base(key, messagePath, device)
        {
            display = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();            

           /* AddAction("/powerOn", (id, content) => display.PowerOn());
            AddAction("/powerOff", (id, content) => display.PowerOff());
            AddAction("/powerToggle", (id, content) => display.PowerToggle());*/

            AddAction("/inputSelect", (id, content) =>
            {
                var s = content.ToObject<MobileControlSimpleContent<string>>();

                var inputPort = display.InputPorts.FirstOrDefault(i => i.Key == s.Value);

                if (inputPort == null)
                {
                    Debug.Console(1, "No input named {0} found for device {1}", s, display.Key);
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

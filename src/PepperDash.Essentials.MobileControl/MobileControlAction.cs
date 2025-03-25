using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Web.RequestHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials
{
    public class MobileControlAction : IMobileControlAction
    {
        public IMobileControlMessenger Messenger { get; private set; }

        public Action<string, string, JToken> Action  {get; private set; }

        public MobileControlAction(IMobileControlMessenger messenger, Action<string,string, JToken> handler) {
            Messenger = messenger;
            Action = handler;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.Streaming;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM.Endpoints.NVX
{
    public abstract class DmNvxControllerBase: CrestronGenericBaseDevice
    {
        public DmNvx35x DmNvx { get; private set; }

        

        public abstract StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public RoutingInputPortWithVideoStatuses AnyVideoInput { get; protected set; }


        public DmNvxControllerBase(string key, string name, DmNvxBaseClass hardware)
            : base(key, name, hardware)
        {
            AddToFeedbackList(ActiveVideoInputFeedback);
        }
    }
}
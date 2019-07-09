using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Bridges
{
    public interface IBridge
    {
        void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey);
    }
}
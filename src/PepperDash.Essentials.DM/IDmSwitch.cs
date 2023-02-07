using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Core;
using PepperDash.Essentials.Core;
//using PepperDash.Essentials.DM.Cards;

using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM {
    public interface IDmSwitch {
        Switch Chassis { get; }

        Dictionary<uint, string> TxDictionary { get; }
        Dictionary<uint, string> RxDictionary { get; }
    }
}